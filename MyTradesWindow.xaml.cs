namespace Sample
{
	using System.Collections.ObjectModel;

	using StockSharp.BusinessEntities;

	public partial class MyTradesWindow
	{
		public MyTradesWindow()
		{
			Trades = new ObservableCollection<MyTrade>();
			InitializeComponent();
		}

		public ObservableCollection<MyTrade> Trades { get; private set; }
	}
}
