using ImportRenewals.Models;
using ImportRenewals.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
                limit = 1;
                List<Quote> quotes = repository.FindByVendorAndRegion(vendorName, region, limit);

                HybrisWebReference.B2BDAQuotesImplService webService = new HybrisWebReference.B2BDAQuotesImplService();
                X509Certificate2 cert = new X509Certificate2();
                string certPath = Environment.CurrentDirectory + "\\ClientKeyStore.jks";
                cert.Import(certPath, "123456", X509KeyStorageFlags.DefaultKeySet);
                webService.ClientCertificates.Add(cert);
                
                foreach (Quote quote in quotes)
                {
                    HybrisWebReference.createQuote arg = this.ConvertQuote(quote);
                    webService.createQuote(arg);
                }
            }
        }

        private HybrisWebReference.createQuote ConvertQuote(Quote quote)
        {
            HybrisWebReference.createDAQuote wsQuote = new HybrisWebReference.createDAQuote();
            wsQuote.UniqueID = quote.QuoteNumber;
            wsQuote.ExpiryDate = quote.ExpiryDate;
            wsQuote.CloseDate = quote.CloseDate;
            wsQuote.Currency = quote.Currency;
            wsQuote.InternalRemarks = quote.InternalRemarks;
            wsQuote.ExternalRemarks = quote.ExternalRemarks;
            //wsQuote.QuoteType = quote.QuoteType;
            wsQuote.CountryCode = quote.CountryCode;
            wsQuote.SalesOrg = quote.SalesOrg;

            //Requester
            wsQuote.QuoteRequester = new HybrisWebReference.createDAQuoteQuoteRequester();
            wsQuote.QuoteRequester.name = quote.QuoteRequesterName;
            wsQuote.QuoteRequester.email = quote.QuoteRequesterEmail;

            //Reseller
            if (quote.Reseller != null)
            {
                wsQuote.Reseller = new HybrisWebReference.createDAQuoteReseller();
                if (quote.Reseller.CompanyAssociation != null)
                {
                    wsQuote.Reseller.SAPcompanyID = quote.Reseller.CompanyAssociation.WestconCode;
                }
            }

            //Bill To
            if (quote.BillTo != null)
            {
                wsQuote.BillTo = new HybrisWebReference.createDAQuoteBillTo();
                if (quote.BillTo.CompanyAssociation != null)
                {
                    wsQuote.BillTo.SAPcompanyID = quote.BillTo.CompanyAssociation.WestconCode;
                }
                else
                {
                    wsQuote.BillTo.Address = new HybrisWebReference.createDAQuoteBillToAddress();
                    wsQuote.BillTo.Address.AddressLine1 = quote.BillTo.Line1;
                    wsQuote.BillTo.Address.AddressLine2 = quote.BillTo.Line2;
                    wsQuote.BillTo.Address.AddressLine3 = quote.BillTo.Line3;
                    wsQuote.BillTo.Address.City = quote.BillTo.City;
                    wsQuote.BillTo.Address.State = quote.BillTo.State;
                    wsQuote.BillTo.Address.Zipcode = quote.BillTo.ZipCode;
                    wsQuote.BillTo.Address.Country = quote.BillTo.Country;
                    wsQuote.BillTo.CompanyName = quote.BillTo.Name;

                }
            }

            //Ship To
            if (quote.ShipTo != null)
            {
                wsQuote.ShipTo = new HybrisWebReference.createDAQuoteShipTo();
                if (quote.ShipTo.CompanyAssociation != null)
                {
                    wsQuote.ShipTo.SAPcompanyID = quote.ShipTo.CompanyAssociation.WestconCode;
                }
                else
                {
                    wsQuote.ShipTo.Address = new HybrisWebReference.createDAQuoteShipToAddress();
                    wsQuote.ShipTo.Address.AddressLine1 = quote.ShipTo.Line1;
                    wsQuote.ShipTo.Address.AddressLine2 = quote.ShipTo.Line2;
                    wsQuote.ShipTo.Address.AddressLine3 = quote.ShipTo.Line3;
                    wsQuote.ShipTo.Address.City = quote.ShipTo.City;
                    wsQuote.ShipTo.Address.State = quote.ShipTo.State;
                    wsQuote.ShipTo.Address.Zipcode = quote.ShipTo.ZipCode;
                    wsQuote.ShipTo.Address.Country = quote.ShipTo.Country;
                    wsQuote.ShipTo.CompanyName = quote.ShipTo.Name;

                }
            }

            //End User
            if (quote.EndUser != null)
            {
                wsQuote.EndUser = new HybrisWebReference.createDAQuoteEndUser();
                if (quote.EndUser.CompanyAssociation != null)
                {
                    wsQuote.EndUser.SAPcompanyID = quote.EndUser.CompanyAssociation.WestconCode;
                }
                wsQuote.EndUser.CompanyName = quote.EndUser.Name;
            }
            
            

            HybrisWebReference.createQuote arg = new HybrisWebReference.createQuote();
            arg.Quote = wsQuote;

            return arg;
        }

        private HybrisWebReference.createDAQuoteQuoteLine[] ConvertLines(ICollection<QuoteLine> quoteLines)
        {
            List<HybrisWebReference.createDAQuoteQuoteLine> results = new List<HybrisWebReference.createDAQuoteQuoteLine>();
            foreach (QuoteLine quoteLine in quoteLines)
            {
                HybrisWebReference.createDAQuoteQuoteLine wgQuoteLine = new HybrisWebReference.createDAQuoteQuoteLine();

            }

            return results.ToArray();

        }
    }
}
