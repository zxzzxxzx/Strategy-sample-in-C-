namespace Sample
{
	using StockSharp.BusinessEntities;

	using Ecng.Xaml;

	public partial class PortfoliosWindow
	{
		public PortfoliosWindow()
		{
			Portfolios = new ThreadSafeObservableCollection<Portfolio>();
			InitializeComponent();
			PortfolioDetails.ItemsSource = Portfolios;
		}

		public ThreadSafeObservableCollection<Portfolio> Portfolios { get; private set; }
	}
}