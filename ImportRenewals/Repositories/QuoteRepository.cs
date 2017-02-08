using ImportRenewals.Contexts;
using ImportRenewals.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ImportRenewals.Repositories
{
    public class QuoteRepository : GenericRepository<Quote>
    {
        public QuoteRepository(): base(new QuoteContext())
        {
            
        }

        public override void Add(Quote quote)
        {
            //Generate Hash
            if (string.IsNullOrEmpty(quote.Hash))
            {
                quote.Hash = Util.GenerateHash();
            }
            foreach(QuoteLine quoteLine in quote.QuoteLines)
            {
                if (string.IsNullOrEmpty(quoteLine.Hash))
                {
                    quoteLine.Hash = Util.GenerateHash();
                }
            }

            //Avoid vendor changes
            base.DbContext.Entry(quote.Vendor).State = EntityState.Unchanged;

            //Add quote
            DbContext.Set<Quote>().Add(quote);
            
            //Add the VRFValues
            foreach (QuoteLine quoteLine in quote.QuoteLines)
            {
                foreach (VRFValue vrfValue in quoteLine.VRFValues)
                {
                    //Avoid VRF changes
                    base.DbContext.Entry(vrfValue.VRF).State = EntityState.Unchanged;
                    vrfValue.QuoteLine = quoteLine;
                    DbContext.Set<VRFValue>().Add(vrfValue);
                }
            }
            
            DbContext.SaveChanges();
        }


    }
}