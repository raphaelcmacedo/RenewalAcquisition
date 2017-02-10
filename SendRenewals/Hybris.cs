using ImportRenewals.Models;
using ImportRenewals.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendRenewals
{
    public class Hybris
    {
        private static int limit = 500;

        public void SendQuotes(string vendorName, string region)
        {
            using (QuoteRepository repository = new QuoteRepository())
            {
                List<Quote> quotes = repository.FindByVendorAndRegion(vendorName, region, limit);
                foreach (Quote quote in quotes)
                {
                    this.ConvertQuote(quote);
                }
            }
        }

        public void ConvertQuote(Quote quote)
        {

        }
    }
}
