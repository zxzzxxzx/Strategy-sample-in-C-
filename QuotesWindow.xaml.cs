namespace Sample
{
	using System.Collections.ObjectModel;

	public partial class QuotesWindow
	{
		public QuotesWindow()
		{
			Quotes = new ObservableCollection<SampleQuote>();
			InitializeComponent();
		}

		public ObservableCollection<SampleQuote> Quotes { get; private set; }
	}
}