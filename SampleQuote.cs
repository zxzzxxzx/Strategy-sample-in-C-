namespace Sample
{
	using System;

	using StockSharp.BusinessEntities;

	public class SampleQuote
	{
		public SampleQuote(Quote quote)
		{
			if (quote == null)
				throw new ArgumentNullException("quote");

            Price = quote.Price;

			if (quote.OrderDirection == OrderDirections.Buy)
			{
				Bid = quote.Volume.ToString();
			}
			else
			{
				Ask = quote.Volume.ToString();
			}
		}

		public string Bid { get; private set; }
		public string Ask { get; private set; }
		public decimal Price { get; private set; }
	}
}