using Wintellect.PowerCollections;

namespace Sample
{
	using System;
	using System.ComponentModel;
	using System.Collections.Generic;
	using System.Collections.Concurrent;
	using System.Threading;

	using System.Windows;
	using System.Windows.Forms;
	using System.Media;
	
	using MessageBox = System.Windows.MessageBox;

	using Ecng.Collections;
	using Ecng.Common;
	using Ecng.Xaml;

	using StockSharp.BusinessEntities;
	using StockSharp.Quik;

	public partial class MainWindow
	{
		public QuikTrader Trader;

		public string ConnectedSound			= "Waves\\Connected.wav";
		public string ConnectionLostSound		= "Waves\\ConnectionLost.wav";
		public string AnnouncementSound			= "Waves\\Announcement.wav";
		public string CheckedSound				= "Waves\\AutoChase.wav";
		public string SuspendedSound			= "Waves\\Alert4.wav";
		public string LiveDataConnectSound		= "Waves\\AG_Kuiq.wav";
		public string DataRequestExceptionSound = "Waves\\Announcement.wav";
		public string ConnectedLive				= "Waves\\AG_Kuiq.wav";
		public string DisconnectedLive			= "Waves\\AG_Instr_Del.wav";
		public string ConnectionFault			= "Waves\\ConnectionFault.wav";


		private readonly SecuritiesWindow _securitiesWindow = new SecuritiesWindow();
		private readonly TradesWindow _tradesWindow = new TradesWindow();
		private readonly MyTradesWindow _myTradesWindow = new MyTradesWindow();
		private readonly OrdersWindow _ordersWindow = new OrdersWindow();
		private readonly PortfoliosWindow _portfoliosWindow = new PortfoliosWindow();
		private readonly PositionsWindow _positionsWindow = new PositionsWindow();
		private readonly StopOrderWindow _stopOrderWindow = new StopOrderWindow();

		public MainWindow()
		{
			InitializeComponent();
			MainWindow.Instance = this;

			_ordersWindow.MakeHideable();
			_myTradesWindow.MakeHideable();
			_tradesWindow.MakeHideable();
			_securitiesWindow.MakeHideable();
			_stopOrderWindow.MakeHideable();
			_positionsWindow.MakeHideable();
			_portfoliosWindow.MakeHideable();

			// попробовать сразу найти месторасположение Quik по запущенному процессу
			Path.Text = QuikTerminal.GetDefaultPath();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			_ordersWindow.DeleteHideable();
			_myTradesWindow.DeleteHideable();
			_tradesWindow.DeleteHideable();
			_securitiesWindow.DeleteHideable();
			_stopOrderWindow.DeleteHideable();
			_positionsWindow.DeleteHideable();
			_portfoliosWindow.DeleteHideable();
			
			_securitiesWindow.Close();
			_tradesWindow.Close();
			_myTradesWindow.Close();
			_stopOrderWindow.Close();
			_ordersWindow.Close();
			_positionsWindow.Close();
			_portfoliosWindow.Close();

			if (Trader != null)
			{
				if (_isDdeStarted)
					StopDde();

				Trader.Dispose();
			}

			base.OnClosing(e);
		}

		public static MainWindow Instance { get; private set; }

		private void FindPathClick(object sender, RoutedEventArgs e)
		{
			var dlg = new FolderBrowserDialog();

			if (!Path.Text.IsEmpty())
				dlg.SelectedPath = Path.Text;

			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Path.Text = dlg.SelectedPath;
			}
		}

		private bool _isConnected;

		private void ConnectClick(object sender, RoutedEventArgs e)
		{
			if (!_isConnected)
			{
				if (Path.Text.IsEmpty())
					MessageBox.Show(this, "Путь к Quik не выбран");
				else
				{
					if (Trader == null)
					{
						// создаем шлюз
						Trader = new QuikTrader(Path.Text);
						
						//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
						Trader.SupportManualOrders = true;
						//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

						// возводим флаг, что соединение установлено
						_isConnected = true;

						// инициализируем механизм переподключения (будет автоматически соединяться
						// каждые 10 секунд, если шлюз потеряется связь с сервером)
						Trader.ReConnectionSettings.Interval = TimeSpan.FromSeconds(10);

						// переподключение будет работать только во время работы биржи РТС
						// (чтобы отключить переподключение когда торгов нет штатно, например, ночью)
						Trader.ReConnectionSettings.WorkingTime = ExchangeBoard.Forts.WorkingTime;

						// подписываемся на событие об успешном восстановлении соединения
						Trader.ReConnectionSettings.ConnectionRestored += () => this.GuiAsync(() => MessageBox.Show(this, "Соединение восстановлено"));

						// подписываемся на событие разрыва соединения

						Trader.ConnectionError += error => this.GuiAsync(() => MessageBox.Show(this, error.ToString()));

						Trader.NewSecurities	+= securities => this.GuiAsync(() => _securitiesWindow.Securities.AddRange(securities));
						Trader.NewMyTrades		+= trades => this.GuiAsync(() => _myTradesWindow.Trades.AddRange(trades));
						Trader.NewTrades		+= trades => this.GuiAsync(() => _tradesWindow.Trades.AddRange(trades));
						Trader.NewOrders		+= orders => this.GuiAsync(() => _ordersWindow.Orders.AddRange(orders));
						Trader.NewStopOrders	+= orders => this.GuiAsync(() => _stopOrderWindow.Orders.AddRange(orders));
						Trader.NewPortfolios	+= portfolios => this.GuiAsync(() => _portfoliosWindow.Portfolios.AddRange(portfolios));
						Trader.NewPositions		+= positions => this.GuiAsync(() => _positionsWindow.Positions.AddRange(positions));
						Trader.ProcessDataError += ex => System.Diagnostics.Debug.WriteLine(ex);
						Trader.Connected		+= () => this.GuiAsync(() => 
																		{
																			ExportDde.IsEnabled = true;
																			if (!Trader.IsExportStarted)
																			{
																				Trader.SecuritiesTable.Columns.Add(DdeSecurityColumns.MinStepPrice);
																				Trader.SecuritiesTable.Columns.Add(DdeSecurityColumns.Strike);
																				Trader.SecuritiesTable.Columns.Add(DdeSecurityColumns.OptionType);
																				Trader.SecuritiesTable.Columns.Add(DdeSecurityColumns.ExpiryDate);
																				Trader.SecuritiesTable.Columns.Add(DdeSecurityColumns.UnderlyingSecurity);
																				Trader.StartExport();
																			}
																			(new SoundPlayer(ConnectedSound)).Play();
																		});

						ShowSecurities.IsEnabled = ShowTrades.IsEnabled =
						ShowMyTrades.IsEnabled = ShowOrders.IsEnabled =
						ShowPortfolios.IsEnabled = ShowStopOrders.IsEnabled = btnStart.IsEnabled = true;
					}

					Trader.Connect();

					_isConnected = true;
					ConnectBtn.Content = "Отключиться";
				}
			}
			else
			{
				Trader.Disconnect();

				_isConnected = false;
				ConnectBtn.Content = "Подключиться";
			}
		}

		private void ShowSecuritiesClick(object sender, RoutedEventArgs e)
		{
			ShowOrHide(_securitiesWindow);
		}

		private void ShowTradesClick(object sender, RoutedEventArgs e)
		{
			ShowOrHide(_tradesWindow);
		}

		private void ShowMyTradesClick(object sender, RoutedEventArgs e)
		{
			ShowOrHide(_myTradesWindow);
		}

		private void ShowOrdersClick(object sender, RoutedEventArgs e)
		{
			ShowOrHide(_ordersWindow);
		}

		private void ShowPortfoliosClick(object sender, RoutedEventArgs e)
		{
			ShowOrHide(_portfoliosWindow);
			ShowOrHide(_positionsWindow);
		}

		private void ShowStopOrdersClick(object sender, RoutedEventArgs e)
		{
			ShowOrHide(_stopOrderWindow);
		}

		private static void ShowOrHide(Window window)
		{
			if (window == null)
				throw new ArgumentNullException("window");

			if (window.Visibility == Visibility.Visible)
				window.Hide();
			else
				window.Show();
		}

		private bool _isDdeStarted;

		private void StartDde()
		{
			Trader.StartExport();
			_isDdeStarted = true;
		}

		private void StopDde()
		{
			Trader.StopExport();
			_isDdeStarted = false;
		}

		private void ExportDdeClick(object sender, RoutedEventArgs e)
		{
			if (_isDdeStarted)
				StopDde();
			else
				StartDde();
		}

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			Strategy frmStrategy = new Strategy(this);
			frmStrategy.Show();
		}
	}
}
