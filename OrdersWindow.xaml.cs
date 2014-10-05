namespace Sample
{
	using System.Collections.ObjectModel;
	using System.Windows;
	using System.Windows.Controls;

	using Ecng.Common;
	using Ecng.Xaml;

	using StockSharp.BusinessEntities;

	public partial class OrdersWindow
	{
		public OrdersWindow()
		{
			Orders = new ObservableCollection<Order>();
			InitializeComponent();
		}

		public ObservableCollection<Order> Orders { get; private set; }

		private Order SelectedOrder
		{
			get { return OrdersDetails.SelectedValue as Order; }
		}

		private void CancelOrderClick(object sender, RoutedEventArgs e)
		{
			MainWindow.Instance.Trader.CancelOrder(SelectedOrder);
		}

		private void CancelGroupOrdersClick(object sender, RoutedEventArgs e)
		{
			MainWindow.Instance.Trader.CancelOrders();
		}

		private void OrdersDetailsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var order = SelectedOrder;
			ExecConditionOrder.IsEnabled = CancelOrder.IsEnabled = (order != null && order.State == OrderStates.Active);
		}

		private void ExecConditionOrderClick(object sender, RoutedEventArgs e)
		{
			var order = SelectedOrder;

			var newOrder = new NewStopOrderWindow
			{
				Title = "Новая условная заявка на исполнение заявки '{0}'".Put(order.Id),
				ConditionOrder = order,
			};
			newOrder.ShowModal(this);
		}
	}
}