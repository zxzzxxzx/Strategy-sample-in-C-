namespace Sample
{
	using Ecng.Xaml;

	using StockSharp.BusinessEntities;

	public partial class PositionsWindow
	{
		public PositionsWindow()
		{
			Positions = new ThreadSafeObservableCollection<Position>();
			InitializeComponent();
			PositionDetails.ItemsSource = Positions;
		}

		public ThreadSafeObservableCollection<Position> Positions { get; private set; }
	}
}