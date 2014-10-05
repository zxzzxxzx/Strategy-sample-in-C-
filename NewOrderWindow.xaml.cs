namespace Sample
{
	using System.Windows;

	using Ecng.Common;

	using StockSharp.BusinessEntities;

	public partial class NewOrderWindow
	{
		public NewOrderWindow()
		{
			InitializeComponent();
			Portfolio.Trader = MainWindow.Instance.Trader;
		}

		public Security Security { get; set; }

		private void SendClick(object sender, RoutedEventArgs e)
		{
			var order = new Order
			{
				Portfolio = Portfolio.SelectedPortfolio,
				Volume = Volume.Text.To<decimal>(),
				Price = Price.Text.To<decimal>(),
				Security = Security,
				Direction = IsBuy.IsChecked == true ? OrderDirections.Buy : OrderDirections.Sell,
			};
			
			MainWindow.Instance.Trader.RegisterOrder(order);
			DialogResult = true;
		}
	}
}
