namespace Sample
{
	using System.Collections.ObjectModel;

	using StockSharp.BusinessEntities;

	public partial class TradesWindow
	{
		public TradesWindow()
		{
			Trades = new ObservableCollection<Trade>();
			InitializeComponent();
		}

		public ObservableCollection<Trade> Trades { get; private set; }
	}
}