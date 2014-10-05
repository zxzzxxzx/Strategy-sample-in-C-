using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using StockSharp.Quik;
using StockSharp.Algo;
using StockSharp.BusinessEntities;

namespace Sample
{
	/// <summary>
	/// Логика взаимодействия для Strategy.xaml
	/// </summary>
	public partial class Strategy : Window
	{
		MainWindow mainWindow;
		QuikTrader trader;
		public Strategy(MainWindow mWnd)
		{
			mainWindow = mWnd;
			InitializeComponent();

			trader = mainWindow.Trader;
			cbSecurity.ItemsSource = trader.Securities;
			cbPortfolio.ItemsSource = trader.Portfolios;

			txtSteps.Text = countOfSteps.ToString();
		}

		public enum StrategyOperation
		//================================================================================================================================================
		{//enum StrategyOperation
			AddOrder,
			DeleteOrder,
			UpdateOrder,
			ExecuteOrder,
			MatchedOrder,
			CanceledOrder,
			Reorganization,
		}//enum StrategyOperation
		//================================================================================================================================================

		internal class AnalizationObjects
		//================================================================================================================================================
		{//internal class AnalizationObjects
			internal StrategyOperation operation;
			internal object obj;
		}//internal class AnalizationObjects
		//================================================================================================================================================

		Thread threadStrategy;
		Thread threadTrader;

		public int processStrategyRate = 100;
		public SortedList<long, MyOrder> orders = null;
		public Dictionary<string, MyOrder> ordersStrategy;

		public int countOfSteps = 5;
		public float startPrice = 0f;
		public int startSpread = 20;
		public int stepPoints = 50;
		private int stepVolume = 1;
		public int StepVolume
		{
			get { return stepVolume; }
			set
			{
				if (value != stepVolume)
				{
					stepVolume = value;
					RegisterAnalizationObject(StrategyOperation.Reorganization, null);
					//ordersReorganisation = true;
				}
			}
		}


		Security security = null;
		Portfolio portfolio = null;

		Trade lastTrade = null;

		public enum OrderStratetgyType
		{
			Step,
			Delute,
			Take
		}

		public class OrderExtensionInfo
		//===========================================================================================================================================================================
		{//private class OrderExtensionInfo
			public string name;
			public int stepNumber;
			public OrderStratetgyType type;
		}//private class OrderExtensionInfo
		//===========================================================================================================================================================================

		public enum MyOrderStatus : short
		//================================================================================================================================================
		{//enum OrderStatus
			Undefined			= 0,
			Created				= 15,	//создан, не отправлен в торговую систему
			Pending				= 16,	//в торговой системе
			SentToServer		= 17,	//в торговой системе
			AcceptedByServer	= 18,	//в торговой системе
			PartlyFilled		= 19,
			Filled				= 20,
			Cancelling			= 21,	//отменен пользователем
			SentToCancel		= 22,	//отменен пользователем
			Cancelled			= 23,	//отменен пользователем
			Failed				= 24	//отменен пользователем
		}//enum OrderStatus
		//================================================================================================================================================

		public class OrderEventArg : EventArgs
		//================================================================================================================================================
		{//
			public MyOrder order;
		}//
		//================================================================================================================================================

		public class OrderFailEventArg : EventArgs
		//================================================================================================================================================
		{//
			public MyOrder order;
			public OrderFail fail;
		}//
		//================================================================================================================================================

		public delegate void OrderChangedEventHandler(object sender, EventArgs arg);
		public delegate void OrderFailEventHandler(object sender, OrderFailEventArg arg);

		public delegate void OrderEventHandler(object sender, OrderEventArg arg);

		public class OrderFail
		//================================================================================================================================================
		{//
			public StockSharp.BusinessEntities.OrderFail ssFail;
			public override string ToString() { return ssFail.Error.ToString(); }
		}//
		//================================================================================================================================================

		public class MyOrder
		//================================================================================================================================================
		{
			public float price;
			public int amount;

			public long id;
			public MyOrder() 
			{
				//if (l == null) throw new Exception("Lock object undefined");
				Status	= MyOrderStatus.Created;
			}

			public bool IsCancelling { get { return ((status == MyOrderStatus.Cancelling) || (status == MyOrderStatus.SentToCancel) || (status == MyOrderStatus.Cancelled)) ? true : false; } }
			private System.Threading.ReaderWriterLockSlim lockerStatus = new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.SupportsRecursion);

			private MyOrderStatus	status;
			public MyOrderStatus	Status 
			{
				get { lockerStatus.EnterReadLock(); 
                      MyOrderStatus st = status; 
                      lockerStatus.ExitReadLock(); 
                      return st; }

				set { lockerStatus.EnterWriteLock(); 
                      status = value; 
                      AddStage(status.ToString()); 
                      lockerStatus.ExitWriteLock(); } 
			}

			internal string Stage;
			internal void ClearStage() { Stage = ""; }
			internal void AddStage(string st) { Stage += string.Format("{0,-12:hh:mm:ss.fff}:{1}", DateTime.Now, st) + Environment.NewLine; }

			public Order ssOrder;
			public StockSharp.BusinessEntities.OrderStatus? ssOrderStatus	{ get { return (ssOrder == null) ? StockSharp.BusinessEntities.OrderStatus.NotSupported : ssOrder.Status; } }
			public StockSharp.BusinessEntities.OrderStates	ssOrderState	{ get { return (ssOrder == null) ? StockSharp.BusinessEntities.OrderStates.None : ssOrder.State; } }
			
			public OrderExtensionInfo extensionInfo = null;

			public override string ToString()
			{
				return string.Format("ID:{0} {1} price:{2}, vol={3}, status={4}, ssOrder={5}", (ssOrder == null) ? "?" : ssOrder.TransactionId.ToString(), "BUY", price, amount, status.ToString(), (ssOrder == null) ? "NULL" : ssOrder.ToString());
			}

			#region Events
			public event OrderEventHandler OrderRegistered;
			public event OrderFailEventHandler OrderRegisterFailed;

			public event OrderEventHandler OrderCancelled;
			public event OrderFailEventHandler OrderCancelFailed;

			public event OrderEventHandler OrderPartlyMatched;
			public event OrderEventHandler OrderMatched;

			public void OnOrderRegistered() { if (OrderRegistered != null) OrderRegistered(this, null); }
			public void OnOrderCancelled() { if (OrderCancelled != null) OrderCancelled(this, null); }

			public void OnOrderMatched() { if (OrderMatched != null) OrderMatched(this, null); }
			public void OnOrderPartlyMatched() { if (OrderPartlyMatched != null) OrderPartlyMatched(this, null); }

			public void OnOrderRegisterFailed(OrderFailEventArg arg)	{ if (OrderRegisterFailed != null)	OrderRegisterFailed(this, arg); }
			public void OnOrderCancelFailed(OrderFailEventArg arg)		{ if (OrderCancelFailed != null)	OrderCancelFailed(this, arg); }
			#endregion
		}
		//================================================================================================================================================

		internal ConcurrentQueue<AnalizationObjects> analizationPool = new ConcurrentQueue<AnalizationObjects>(); //пул параметров появляющихся в системе для алгоритма анализа стратегией
		public void RegisterAnalizationObject(StrategyOperation oper, object o) { analizationPool.Enqueue(new AnalizationObjects() { operation = oper, obj = o }); }
		public bool ordersReorganisation = true;
		public bool refreshObjects = false;	

		public void ProcessStrategy()
		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		{//void ProcessStrategy()
			if (analizationPool.Count != 0)
			//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			{//анализируем потупивший набор объектов
				AnalizationObjects objAnalize = null;
				//GISMO.Trade.Order ord = null;
				while (analizationPool.TryDequeue(out objAnalize))
				//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
				{//while (analizationPool.TryDequeue(out objAnalize))
					switch (objAnalize.operation)
					//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
					{//switch (objAnalize.operation)
						case StrategyOperation.Reorganization:
							ordersReorganisation = true;
							break;
						case StrategyOperation.CanceledOrder:
							if (objAnalize.obj.GetType() != typeof(MyOrder)) throw new Exception("Unexpected type");
							MyOrder ord = objAnalize.obj as MyOrder;
							ordersStrategy[ord.extensionInfo.name] = null;
							ordersReorganisation = true;
							break;
						case StrategyOperation.DeleteOrder:
							if (objAnalize.obj.GetType() != typeof(MyOrder)) throw new Exception("Unexpected type");
							ord = objAnalize.obj as MyOrder;
							ordersStrategy[ord.extensionInfo.name] = null;
							ordersReorganisation = true;
							break;
					}//switch (objAnalize.operation)
					//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
				}//while (analizationPool.TryDequeue(out objAnalize))
				//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			}//анализируем потупивший набор объектов
			//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

			List<MyOrder> lstCancel = new List<MyOrder>();
			List<MyOrder> lstSend = new List<MyOrder>();

			if (ordersReorganisation)
			//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			{//реорганизация ордеров
				if (ordersStrategy == null)
				{//создадим структуру для хранения ордеров
					ordersStrategy = new Dictionary<string, MyOrder>();
					ordersStrategy.Add("TAKE", null);
					ordersStrategy.Add("DELUTE", null);
					for (int i = 0; i < countOfSteps; i++) ordersStrategy.Add("STEP" + i.ToString(), null);
				}//создадим структуру для хранения ордеров

				for (int i = 0; i < countOfSteps; i++)
				//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
				{//for (int i = 0; i < countOfSteps; i++)
					if (ordersStrategy["STEP" + i.ToString()] == null)
					//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
					{//if (ordersStrategy["STEP" + i.ToString()] == null)
						MyOrder myOrder = new MyOrder()
						{
							price = startPrice + stepPoints * i * (-1),
							amount = stepVolume,
							extensionInfo = new OrderExtensionInfo()
							{
								name = "STEP" + i.ToString(),
								stepNumber = i,
								type = OrderStratetgyType.Step
							}
						};
						myOrder.OrderCancelled += (object sender, OrderEventArg arg) =>
													{
														myOrder.Status = MyOrderStatus.Cancelled;
														//SendMessage("ORDER[" + ((sender as Order).extensionInfo as OrderExtensionInfo).stepNumber.ToString() + "]=>CANCELED", "Order canceled: " + (sender as Order).ToString());
														RegisterAnalizationObject(StrategyOperation.CanceledOrder, (sender as MyOrder));
														refreshObjects = true;
													};
						myOrder.OrderRegistered += (object sender, OrderEventArg arg) =>
													{
														//SendMessage("ORDER[" + ((sender as Order).extensionInfo as OrderExtensionInfo).stepNumber.ToString() + "]=>REGISTERED", "Order registered: " + (sender as Order).ToString());
														refreshObjects = true;
													};

						myOrder.OrderMatched += (object sender, OrderEventArg arg) =>
													{
										//				SendMessage("ORDER[" + ((sender as Order).extensionInfo as OrderExtensionInfo).stepNumber.ToString() + "]=>MATCHED", "Order matched: " + (sender as Order).ToString());
														RegisterAnalizationObject(StrategyOperation.MatchedOrder, (sender as MyOrder));
														refreshObjects = true;
													};

						myOrder.OrderCancelFailed += (object sender, OrderFailEventArg arg) =>
													{
										//				SendMessage("ORDER[" + ((sender as Order).extensionInfo as OrderExtensionInfo).stepNumber.ToString() + "]=>CANCEL FAIL", "Order cancel Fail: " + (sender as Order).ToString() + ", fail = " + arg.fail.ToString());
														ordersReorganisation = true;
														refreshObjects = true;
													};
						myOrder.OrderRegisterFailed += (object sender, OrderFailEventArg arg) =>
													{
										//				SendMessage("ORDER[" + ((sender as Order).extensionInfo as OrderExtensionInfo).stepNumber.ToString() + "]=>REGISTER FAIL", "Order register Fail: " + (sender as Order).ToString() + ", fail = " + arg.fail.ToString());
														ordersReorganisation = true;
														refreshObjects = true;
													};

						ordersStrategy["STEP" + i.ToString()] = myOrder;
						lstSend.Add(myOrder);
					}//if (ordersStrategy["STEP" + i.ToString()] == null)
					//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
					else
					//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
					{//ордер шага есть, но проверим его параметры
						MyOrder myOrd = ordersStrategy["STEP" + i.ToString()];
			//			if ((myOrd.AmountLeft == ord.amount) && (ord.price != (currentStartPrice + stepPoints * i * (longShort ? -1 : 1))))
			//			{//сменились ценовые уровни
			//				if (!ord.IsCancelling && (ord.ssOrderState == StockSharp.BusinessEntities.OrderStates.Active))
			//				{//удалим не требующийся ордер
			//					ord.rwLock.EnterWriteLock();
			//					if (!ord.IsCancelling && (ord.ssOrderState == StockSharp.BusinessEntities.OrderStates.Active))
			//					{
			//						ord.AddStage("Cancelling on price or volume dismatched with requirements");
			//						ord.Status = Order.OrderStatus.Cancelling;
			//						lstCancel.Add(ord);
			//					}
			//					ord.rwLock.ExitWriteLock();
			//				}//удалим не требующийся ордер
			//			}//сменились ценовые уровни
			//			else 
						if (myOrd.amount != stepVolume)
						{//сменились объемные параметры 
							if (!myOrd.IsCancelling && (myOrd.Status == MyOrderStatus.AcceptedByServer))
							{//удалим не требующийся ордер
								//myOrd.rwLock.EnterWriteLock();
								if (!myOrd.IsCancelling && (myOrd.Status == MyOrderStatus.AcceptedByServer))
								{
									myOrd.AddStage("Cancelling on price or volume dismatched with requirements");
									myOrd.Status = MyOrderStatus.Cancelling;
									lstCancel.Add(myOrd);
								}
								//ord.rwLock.ExitWriteLock();
							}//удалим не требующийся ордер
						}//сменились объемные параметры 
					}//ордер шага есть, но проверим его параметры
					//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
				}//for (int i = 0; i < countOfSteps; i++)
				//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

				if (lstCancel.Count != 0)
				{
					//TradeConnection.SyncCancelOrders(lstCancel);
					SyncCancelOrders(lstCancel);
					//foreach (Order o in lstCancel) SendMessage("CANCEL_ORDER", "Order cancel: " + o.ToString());
					refreshObjects = true;
				}
				if (lstSend.Count != 0)
				{
					//TradeConnection.SyncSendOrders(lstSend);
					SyncSendOrders(lstSend);
					//foreach (Order o in lstSend) SendMessage("SEND_ORDER", "Order sent: " + o.ToString());
					refreshObjects = true;
				}
				ordersReorganisation = false;
			}//реорганизация ордеров
			//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		}//void ProcessStrategy()
		//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		ConcurrentQueue<MyOrder> ordersSend = new ConcurrentQueue<MyOrder>();
		ConcurrentQueue<MyOrder> ordersCancel = new ConcurrentQueue<MyOrder>();

		private void SyncSendOrders(List<MyOrder> orders)
		//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		{
			foreach (MyOrder o in orders) ordersSend.Enqueue(o);
		}
		//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		private void SyncCancelOrders(List<MyOrder> orders)
		//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		{
			foreach (MyOrder o in orders) ordersCancel.Enqueue(o);
		}
		//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		private void butStartStrategy_Click(object sender, RoutedEventArgs e)
		//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		{
			if (threadStrategy == null)
			//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			{//if (threadStrategy == null)
				threadStrategy = new Thread
								(
									new ThreadStart
									(() =>
									{
										try
										{
											while (true)
											//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
											{//бесконечный цикл обработки стратегии
												if (analizationPool.Count != 0) ProcessStrategy();
												Thread.Sleep(processStrategyRate);
											}//бесконечный цикл обработки стратегии
											//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
										}
										catch (Exception ex)
										{
											//SendMessage("ERROR", "Error on Thread startegy:>>" + ex.ToString());
										}
										finally
										{
											//status = StrategyStatus.Stopped;
										}

									}
									)
								);
				threadStrategy.Name = "StrategyThread_" + DateTime.Now.Ticks.ToString();
				threadStrategy.IsBackground = true;
				threadStrategy.Priority = ThreadPriority.Normal;
			}//if (threadStrategy == null)
			//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			if (threadTrader == null)
			//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
			{//if (threadTrader == null)
				threadTrader = new Thread
								(
									new ThreadStart
									(() =>
									{
										try
										{
											while (true)
											//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
											{//бесконечный цикл обработки стратегии
												if (ordersCancel.Count != 0)
												//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
												{//отмена ордеров
													MyOrder myOrder;
													while (ordersCancel.TryDequeue(out myOrder))
													{
														if (myOrder.ssOrder == null)
														{
															throw new Exception("Unexpected state of order");
															continue;
														}
														//lock (order)
														//{
														//if (order.Status == Trade.Order.OrderStatus.Cancelling) 
														//{
														myOrder.Status = MyOrderStatus.SentToCancel;
														trader.CancelOrder(myOrder.ssOrder);
														//}
														//}
														//Thread.Sleep(1000);
													}
												}//отмена ордеров
												//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
												if (ordersSend.Count != 0)
												//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
												{//отправка ордеров
													MyOrder myOrder;
													while (ordersSend.TryDequeue(out myOrder))
													{
														//_o.rwLock.EnterWriteLock();
														MyOrder _order = myOrder;
														StockSharp.BusinessEntities.Order ssOrd = new StockSharp.BusinessEntities.Order()
														{
															Trader		= trader,
															Portfolio	= portfolio,
															Security	= security,
															Comment		= "GISMO",
															Direction	= StockSharp.BusinessEntities.OrderDirections.Buy, // : StockSharp.BusinessEntities.OrderDirections.Sell,
															Price		= (decimal)_order.price,
															Type		= StockSharp.BusinessEntities.OrderTypes.Limit,
															Volume		= _order.amount
														};
														_order.ssOrder	= ssOrd;
														//_order.rulerContainer = StockSharp.Algo.MarketRuleHelper.DefaultRuleContainer;

														var ruleRegFail = ssOrd.WhenRegisterFailed();
														var ruleReg = ssOrd.WhenRegistered();
														//_order.RegisterToken(ruleRegFail.Token);
														//_order.RegisterToken(ruleReg.Token);
														ruleReg.Do((StockSharp.BusinessEntities.Order _ssOrd) =>
																	{
																		try
																		{
																			//GISMO.Trade.Order __order = QTrader._orders[_ssOrd.TransactionId];
																			//__order.rwLock.EnterWriteLock();
																			_order.Status = MyOrderStatus.AcceptedByServer;
																			_order.AddStage("WhenRegistered:OrderRegistered");
																			//__order.rwLock.ExitWriteLock();

																			_order.OnOrderRegistered();
																		}
																		catch (Exception ex)
																		{
																			int i = 0;
																		}
																	})
																	.Once()
																	.Apply(StockSharp.Algo.MarketRuleHelper.DefaultRuleContainer)
																	.Exclusive(ruleRegFail);

														ruleRegFail.Do((StockSharp.BusinessEntities.OrderFail f) =>
																	{
																		try
																		{
																			//GISMO.Trade.Order __order = QTrader._orders[f.Order.TransactionId];
																			//__order.rwLock.EnterWriteLock();
																			_order.Status = MyOrderStatus.Failed;
																			_order.AddStage("WhenRegisteredFail:OrderRegisteredFail");
																			//__order.rwLock.ExitWriteLock();

																			_order.OnOrderRegisterFailed(new OrderFailEventArg()
																			{
																				order = _order,
																				fail = new OrderFail()
																				{
																					ssFail = f
																				}
																			});
																		}
																		catch (Exception ex)
																		{
																			int i = 0;
																		}
																	})
																	.Once()
																	.Apply(StockSharp.Algo.MarketRuleHelper.DefaultRuleContainer)
																	.Exclusive(ruleReg);

														var ruleCancelled = ssOrd.WhenCanceled();
														var ruleCancelledFail = ssOrd.WhenCancelFailed();
														//_order.RegisterToken(ruleCancelled.Token);
														//_order.RegisterToken(ruleCancelledFail.Token);
														ruleCancelled.Do((StockSharp.BusinessEntities.Order _ssOrd) =>
																		{
																			try
																			{
																				//GISMO.Trade.Order __order = QTrader._orders[_ssOrd.TransactionId];
																				//__order.rwLock.EnterWriteLock();
																				_order.Status = MyOrderStatus.Cancelled;
																				//__order.rwLock.ExitWriteLock();

																				//StockSharp.Algo.MarketRuleHelper.DefaultRuleContainer.Rules.RemoveRulesByToken(ruleCancelled.Token, ruleCancelled);
																				_order.OnOrderCancelled();
																			}
																			catch (Exception ex)
																			{
																				int i = 0;
																			}
																		})
																		.Once()
																		.Apply(StockSharp.Algo.MarketRuleHelper.DefaultRuleContainer)
																		.Exclusive(ruleCancelledFail);

														ruleCancelledFail.Do((StockSharp.BusinessEntities.OrderFail f) =>
																				{
																					try
																					{
																						//GISMO.Trade.Order __order = QTrader._orders[f.Order.TransactionId];
																						//__order.rwLock.EnterWriteLock();
																						_order.Status = MyOrderStatus.Failed;
																						_order.AddStage("WhenCancelFail:OrderCancelFail");
																						//__order.rwLock.ExitWriteLock();

																						_order.OnOrderCancelFailed(new OrderFailEventArg()
																						{
																							order = _order,
																							fail = new OrderFail()
																							{
																								ssFail = f
																							}
																						});
																					}
																					catch (Exception ex)
																					{
																						int i = 0;
																					}
																				})
																				.Once()
																				.Apply(StockSharp.Algo.MarketRuleHelper.DefaultRuleContainer)
																				.Exclusive(ruleCancelled);

														var ruleMatched = _order.ssOrder.WhenMatched();
														//_order.RegisterToken(ruleMatched.Token);
														ruleMatched.Do(() =>
																		{
																			try
																			{
																				//_order.rwLock.EnterWriteLock();
																				_order.Status = MyOrderStatus.Filled;
																				//_order.rwLock.ExitWriteLock();

																				StockSharp.Algo.MarketRuleHelper.DefaultRuleContainer.Rules.RemoveRulesByToken(ruleMatched.Token, ruleMatched);
																				_order.OnOrderMatched();
																			}
																			catch (Exception ex)
																			{
																				int i = 0;
																			}
																		})
																		.Once()
																		.Apply();

														var rulePartMatched = _order.ssOrder.WhenPartiallyMatched();
														//_order.RegisterToken(rulePartMatched.Token);
														rulePartMatched.Do(() =>
																		{
																			try
																			{
																				//_order.rwLock.EnterWriteLock();
																				_order.Status = MyOrderStatus.PartlyFilled;
																				//_order.rwLock.ExitWriteLock();

																				_order.OnOrderPartlyMatched();
																			}
																			catch (Exception ex)
																			{
																				int i = 0;
																			}
																		}).Apply();

														var ruleNewTrades = _order.ssOrder.WhenNewTrades();
														//_order.RegisterToken(ruleNewTrades.Token);
														ruleNewTrades.Do((IEnumerable<MyTrade> trades) =>
																	{
																		try
																		{
																			//List<OrderExecution> execs = new List<OrderExecution>();
																			//foreach (MyTrade mt in trades) execs.Add(new OrderExecution() { ssMyTrade = mt });
																			//_order.OnOrderTrades(new OrderExecutionEventArg()
																			//{
																			//	order = _order,
																			//	executions = execs
																			//});
																		}
																		catch (Exception ex)
																		{
																			int i = 0;
																		}
																	})
																	.Apply();
														_order.Status = MyOrderStatus.SentToServer;
														trader.RegisterOrder(ssOrd);
														//Thread.Sleep(1000);
														//QTrader._orders.Add(ssOrd.TransactionId, _order);

														//_o.rwLock.ExitWriteLock();
														//if (_order.Strategy != null) _order.Strategy.SendMessage("QSEND", "Order price " + _order.price.ToString() + " Sent (TransID = " + ssOrd.TransactionId + ")");
													}
												}//отправка ордеров
												//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
												Thread.Sleep(processStrategyRate);
											}//бесконечный цикл обработки стратегии
											//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
										}
										catch (Exception ex)
										{
											//SendMessage("ERROR", "Error on Thread startegy:>>" + ex.ToString());
										}
										finally
										{
											//status = StrategyStatus.Stopped;
										}
									}
									)
								);
				threadTrader.Name = "TraderThread_" + DateTime.Now.Ticks.ToString();
				threadTrader.IsBackground = true;
				threadTrader.Priority = ThreadPriority.Normal;
			}//if (threadTrader == null)
			//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

			try
			{
				//bool tRunning = threadStrategy...ThreadState.HasFlag(ThreadState.Running);
				if (!threadStrategy.IsAlive)	threadStrategy.Start();
				if (!threadTrader.IsAlive)		threadTrader.Start();
			}
			catch (Exception ex)
			{

			}
		}
		//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		private void butVolumePlus_Click(object sender, RoutedEventArgs e)
		{
			StepVolume = stepVolume + 1;
			txtVolume.Text = StepVolume.ToString();
		}

		private void butVolumeMinus_Click(object sender, RoutedEventArgs e)
		{
			if (StepVolume <= 1) return;
			StepVolume = stepVolume - 1;
			txtVolume.Text = StepVolume.ToString();
		}


		private void cbSecurity_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cbSecurity.SelectedValue != null)
			{
				security = cbSecurity.SelectedValue as Security;
				lblSecurityStepPrice.Content = security.MinStepPrice.ToString();
				var ruleNewTrade = security.WhenNewTrades();
				ruleNewTrade.Do((IEnumerable<Trade> trades) =>
								{
									foreach(Trade tr in trades) lastTrade = tr;
									this.Dispatcher.BeginInvoke((Action)(() => 
																{ 
																	txtLastPrice.Text = lastTrade.Price.ToString();
																	lblLastTrade.Content = "at " + lastTrade.Time.ToLongTimeString();
																	startPrice = (float)lastTrade.Price - startSpread * (float)security.MinStepSize;
																	txtFirstStepPrice.Text = startPrice.ToString();
																	if ((DateTime.Now - lastTrade.Time).Minutes > 2)
																	{
																		butStartStrategy.IsEnabled = false;
																		lblStatus.Content = "waiting for loading data ...";
																	}
																	else
																	{
																		butStartStrategy.IsEnabled = true;
																		lblStatus.Content = "ready to start strategy";
																	}
																}));
								}).Apply();
			}
		}

		private void cbPortfolio_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cbPortfolio.SelectedValue != null) portfolio = cbPortfolio.SelectedValue as Portfolio;
		}

		private void txtVolume_KeyDown(object sender, KeyEventArgs e)
		{
			//if (e.Key == System.Windows.Input.Key.Up) txtSpread.Text = (startSpread++).ToString();
			//else if (e.Key == System.Windows.Input.Key.Down) txtSpread.Text = (startSpread--).ToString();

		}

		private void txtVolume_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Up)
			{
				StepVolume		= stepVolume + 1;
				txtVolume.Text	= StepVolume.ToString();
			}
			else if (e.Key == Key.Down)
			{
				if (StepVolume <= 1) return;
				StepVolume = stepVolume - 1;
				txtVolume.Text = StepVolume.ToString();
			}
		}
		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

	}
}
