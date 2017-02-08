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
            using (DbContextTransaction t = base.DbContext.Database.BeginTransaction())
            {

                //Delete old version
                this.RemoveOldQuote(quote);

                //Generate Hash
                if (string.IsNullOrEmpty(quote.Hash))
                {
                    quote.Hash = Util.GenerateHash();
                }
                foreach (QuoteLine quoteLine in quote.QuoteLines)
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
                t.Commit();
            }
        }

        public Quote FindByNumber(string quoteNumber)
        {
            QuoteContext context = (QuoteContext)DbContext;
            return (from q in context.Quotes
                    where q.QuoteNumber.Equals(quoteNumber)
                    select q).FirstOrDefault();
        }

        public void RemoveOldQuote(Quote newQuote)
        {
            Quote old = this.FindByNumber(newQuote.QuoteNumber);
            if (old != null)
            {
                //Delete the lines
                QuoteContext context = (QuoteContext)DbContext;

                List<QuoteLine> lines = new List<QuoteLine>();
                lines.AddRange(old.QuoteLines);

                foreach (QuoteLine quoteLine in lines)
                {
                    //Delete the VRFs
                    List<VRFValue> vrfValues = new List<VRFValue>();
                    vrfValues.AddRange(quoteLine.VRFValues);
                    foreach (VRFValue vrfValue in vrfValues)
                    {
                        DbContext.Set<VRFValue>().Remove(vrfValue);
                    }

                    DbContext.Set<QuoteLine>().Remove(quoteLine);
                }

                //Delete the quotes
                DbContext.Set<Quote>().Remove(old);
            }
        }

    }
}