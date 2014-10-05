namespace Sample
{
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;

	using Ecng.Collections;
	using Ecng.Common;
	using Ecng.Xaml;

	using StockSharp.BusinessEntities;

	public partial class SecuritiesWindow
	{
		private readonly Timer _timer;
		private readonly SynchronizedDictionary<Security, QuotesWindow> _quotesWindows = new SynchronizedDictionary<Security, QuotesWindow>();

		public SecuritiesWindow()
		{
			Securities = new ObservableCollection<Security>();
			InitializeComponent();

			_timer = ThreadingHelper.Timer(() => _quotesWindows.SyncDo(d =>
			{
				foreach (var p in d)
				{
					var pair = p;

					var wnd = pair.Value;

					wnd.GuiAsync(() =>
					{
						wnd.Quotes.Clear();
						wnd.Quotes.AddRange(MainWindow.Instance.Trader.GetMarketDepth(pair.Key).Select(q => new SampleQuote(q)));
					});
				}
			}))
			.Interval(TimeSpan.FromSeconds(1));
		}

		protected override void OnClosed(EventArgs e)
		{
			_timer.Dispose();

			_quotesWindows.SyncDo(d =>
			{
				foreach (var pair in d)
				{
					MainWindow.Instance.Trader.UnRegisterMarketDepth(pair.Key);

					pair.Value.DeleteHideable();
					pair.Value.Close();
				}
			});

			base.OnClosed(e);
		}

		public ObservableCollection<Security> Securities { get; private set; }

		private void NewOrderClick(object sender, RoutedEventArgs e)
		{
			var security = (Security)SecuritiesDetails.SelectedValue;

			var newOrder = new NewOrderWindow { Title = "Новая заявка на '{0}'".Put(security.Code), Security = security };
			newOrder.ShowModal(this);
		}

		private void SecuritiesDetailsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			NewStopOrder.IsEnabled = NewOrder.IsEnabled =
			Quotes.IsEnabled = SecuritiesDetails.SelectedIndex != -1;
		}

		private void NewStopOrderClick(object sender, RoutedEventArgs e)
		{
			var security = (Security)SecuritiesDetails.SelectedValue;

			var newOrder = new NewStopOrderWindow
			{
				Title = "Новая заявка на '{0}'".Put(security.Code),
				Security = security,
			};
			newOrder.ShowModal(this);
		}

		private void QuotesClick(object sender, RoutedEventArgs e)
		{
			var window = _quotesWindows.SafeAdd((Security)SecuritiesDetails.SelectedValue, security =>
			{
				// начинаем получать котировки стакана
				MainWindow.Instance.Trader.RegisterMarketDepth(security);

				// создаем окно со стаканом
				var wnd = new QuotesWindow { Title = security.Code + " котировки" };
				wnd.MakeHideable();
				return wnd;
			});

			if (window.Visibility == Visibility.Visible)
				window.Hide();
			else
				window.Show();
		}
	}
}