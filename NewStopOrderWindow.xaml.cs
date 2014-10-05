namespace Sample
{
	using System;
	using System.Windows;
	using System.Windows.Controls;

	using Ecng.Common;
	using Ecng.ComponentModel;

	using StockSharp.Quik;
	using StockSharp.BusinessEntities;

	public partial class NewStopOrderWindow
	{
		private readonly bool _initialized;

		public NewStopOrderWindow()
		{
			InitializeComponent();
			Portfolio.Trader = MainWindow.Instance.Trader;
			_initialized = true;
			RefreshControls();
			OtherSecurities.ItemsSource = MainWindow.Instance.Trader.Securities;
			OtherSecurities.SelectedIndex = 0;
			ActiveTimeFrom.Value = new DateTime(new TimeSpan(10, 0, 0).Ticks);
			ActiveTimeTo.Value = new DateTime(new TimeSpan(23, 50, 0).Ticks);
			Offset.Value = new Unit();
			Spread.Value = new Unit();
		}

		public Security Security { get; set; }

		private Order _conditionOrder;

		public Order ConditionOrder
		{
			get { return _conditionOrder; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				_conditionOrder = value;

				Security = value.Security;

				// типы стоп-заявок "со связанной заявкой" и "по другой бумаге" недоступны при стоп-заявке по исполнению
				LinkedOrderType.IsEnabled = OtherSecurityType.IsEnabled = false;

				// заявка условие всегда имеет противоположное направление
				IsBuy.IsChecked = (value.Direction == OrderDirections.Buy);

				// пользователь не может выбрать направление завки
				IsBuy.IsEnabled = IsSell.IsEnabled = false;

				// выбираем по умолчанию тип стоп-лимит
				StopOrderType.SelectedIndex = 1;
				
				PartiallyMatched.IsEnabled = UseMatchedBalance.IsEnabled = true;
				PartiallyMatched.IsChecked = UseMatchedBalance.IsChecked = true;
			}
		}

		private void SendClick(object sender, RoutedEventArgs e)
		{
			Order stopOrder;

			// если это обычная стоп-заявка
			if (ConditionOrder == null)
			{
				switch (StopOrderType.SelectedIndex)
				{
					case 0:
						stopOrder = CreateLinkedOrder();
						break;
					case 1:
						stopOrder = CreateStopLimit();
						break;
					case 2:
						stopOrder = CreateOtherSecurity();
						break;
					case 3:
						stopOrder = CreateTakeProfit();
						break;
					case 4:
						stopOrder = CreateTakeProfitAndStopLimit();
						break;
					default:
						throw new InvalidOperationException("Выбран неизвестный тип стоп-заявки.");
				}
			}
			else // если это стоп-заявка "по исполнению"
			{
				switch (StopOrderType.SelectedIndex)
				{
					case 1:
						stopOrder = CreateConditionStopLimit();
						break;
					case 3:
						stopOrder = CreateConditionTakeProfit();
						break;
					case 4:
						stopOrder = CreateConditionTakeProfitAndStopLimit();
						break;
					default:
						throw new InvalidOperationException("Выбран неизвестный тип стоп-заявки.");
				}
			}

			stopOrder.Portfolio = Portfolio.SelectedPortfolio;

			MainWindow.Instance.Trader.RegisterOrder(stopOrder);
			DialogResult = true;
		}

		private Order CreateLinkedOrder()
		{
			return new Order
			{
				Type = OrderTypes.Conditional,
				Volume = Volume.Text.To<decimal>(),
				Price = Price.Text.To<decimal>(),
				Security = Security,
				Direction = IsBuy.IsChecked == true ? OrderDirections.Buy : OrderDirections.Sell,
				ExpiryDate = ExpirationDate.Value,
				Condition = new QuikOrderCondition
				{
					Type = QuikOrderConditionTypes.LinkedOrder,
					LinkedOrderPrice = LinkedOrderPrice.Text.To<decimal>(),
					LinkedOrderCancel = LinkedOrderCancel.IsChecked == true,
					StopPrice = StopPrice.Text.To<decimal>(),
					ActiveTime = ActiveTime,
				},
			};
		}

		private Order CreateStopLimit()
		{
			return new Order
			{
				Type = OrderTypes.Conditional,
				Volume = Volume.Text.To<decimal>(),
				Price = Price.Text.To<decimal>(),
				Security = Security,
				Direction = IsBuy.IsChecked == true ? OrderDirections.Buy : OrderDirections.Sell,
				ExpiryDate = ExpirationDate.Value,
				Condition = new QuikOrderCondition
				{
					Type = QuikOrderConditionTypes.StopLimit,
					StopPrice = StopPrice.Text.To<decimal>(),
					ActiveTime = ActiveTime,
				},
			};
		}

		private Order CreateOtherSecurity()
		{
			return new Order
			{
				Type = OrderTypes.Conditional,
				Volume = Volume.Text.To<decimal>(),
				Price = Price.Text.To<decimal>(),
				Security = Security,
				Direction = IsBuy.IsChecked == true ? OrderDirections.Buy : OrderDirections.Sell,
				ExpiryDate = ExpirationDate.Value,
				Condition = new QuikOrderCondition
				{
					Type = QuikOrderConditionTypes.OtherSecurity,
					StopPriceCondition = StopPriceCondition.Text == ">=" ? QuikStopPriceConditions.MoreOrEqual : QuikStopPriceConditions.LessOrEqual,
					StopPrice = StopPrice.Text.To<decimal>(),
					OtherSecurity = (Security)OtherSecurities.SelectedValue,
					ActiveTime = ActiveTime,
				},
			};
		}

		private Order CreateTakeProfit()
		{
			return new Order
			{
				Type = OrderTypes.Conditional,
				Volume = Volume.Text.To<decimal>(),
				Security = Security,
				Direction = IsBuy.IsChecked == true ? OrderDirections.Buy : OrderDirections.Sell,
				ExpiryDate = ExpirationDate.Value,
				Condition = new QuikOrderCondition
				{
					Type = QuikOrderConditionTypes.TakeProfit,
					StopPrice = StopPrice.Text.To<decimal>(),
					Offset = Offset.Value.Clone().SetSecurity(Security),
					Spread = Spread.Value.Clone().SetSecurity(Security),
					ActiveTime = ActiveTime,
				},
			};
		}

		private Order CreateTakeProfitAndStopLimit()
		{
			return new Order
			{
				Type = OrderTypes.Conditional,
				Volume = Volume.Text.To<decimal>(),
				Price = Price.Text.To<decimal>(),
				Security = Security,
				Direction = IsBuy.IsChecked == true ? OrderDirections.Buy : OrderDirections.Sell,
				ExpiryDate = ExpirationDate.Value,
				Condition = new QuikOrderCondition
				{
					Type = QuikOrderConditionTypes.TakeProfitStopLimit,
					StopPrice = StopPrice.Text.To<decimal>(),
					StopLimitPrice = StopLimitPrice.Text.To<decimal>(),
					Offset = Offset.Value.Clone().SetSecurity(Security),
					Spread = Spread.Value.Clone().SetSecurity(Security),
					ActiveTime = ActiveTime,
				},
			};
		}

		private Order CreateConditionStopLimit()
		{
			var stopLimit = CreateStopLimit();
			var condition = (QuikOrderCondition)stopLimit.Condition;
			condition.ConditionOrder = ConditionOrder;
			condition.ConditionOrderPartiallyMatched = PartiallyMatched.IsChecked;
			condition.ConditionOrderUseMatchedBalance = UseMatchedBalance.IsChecked;
			return stopLimit;
		}

		private Order CreateConditionTakeProfit()
		{
			var stopLimit = CreateTakeProfit();
			var condition = (QuikOrderCondition)stopLimit.Condition;
			condition.ConditionOrder = ConditionOrder;
			condition.ConditionOrderPartiallyMatched = PartiallyMatched.IsChecked;
			condition.ConditionOrderUseMatchedBalance = UseMatchedBalance.IsChecked;
			return stopLimit;
		}

		private Order CreateConditionTakeProfitAndStopLimit()
		{
			var stopLimit = CreateTakeProfitAndStopLimit();
			var condition = (QuikOrderCondition)stopLimit.Condition;
			condition.ConditionOrder = ConditionOrder;
			condition.ConditionOrderPartiallyMatched = PartiallyMatched.IsChecked;
			condition.ConditionOrderUseMatchedBalance = UseMatchedBalance.IsChecked;
			return stopLimit;
		}

		private void StopOrderTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RefreshControls();
		}

		private void RefreshControls()
		{
			if (_initialized)
			{
				LinkedOrderCancel.IsEnabled = LinkedOrderPrice.IsEnabled = StopOrderType.SelectedIndex == 0;
				StopPriceCondition.IsEnabled = OtherSecurities.IsEnabled = StopOrderType.SelectedIndex == 2;
				Offset.IsReadOnly = Spread.IsReadOnly = StopOrderType.SelectedIndex == 3 || StopOrderType.SelectedIndex == 4;
				Price.IsEnabled = StopOrderType.SelectedIndex != 3;
				StopLimitPrice.IsEnabled = StopOrderType.SelectedIndex == 4;
			}
		}

		private Range<DateTime> ActiveTime
		{
			get
			{
				if (IsActiveTime.IsChecked == true)
					return new Range<DateTime>(DateTime.Today + ActiveTimeFrom.Value.Value.TimeOfDay, DateTime.Today + ActiveTimeTo.Value.Value.TimeOfDay);
				else
					return null;
			}
		}

		private void IsActiveTimeChecked(object sender, RoutedEventArgs e)
		{
			ActiveTimeFrom.IsEnabled = ActiveTimeTo.IsEnabled = IsActiveTime.IsChecked == true;
		}
	}
}