namespace Sample
{
	using System.Collections.ObjectModel;
	using System.Windows;
	using System.Windows.Controls;

	using StockSharp.BusinessEntities;

	public partial class StopOrderWindow
	{
		public StopOrderWindow()
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

		private void OrdersDetailsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var order = SelectedOrder;
			CancelOrder.IsEnabled = order != null && order.State == OrderStates.Active;
		}
	}
}
