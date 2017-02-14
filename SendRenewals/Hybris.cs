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
                string certPath = Environment.CurrentDirectory + "\\ClientKeyStore.pfx";
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

            wsQuote.QuoteLines = this.ConvertLines(quote.QuoteLines);

            HybrisWebReference.createQuote arg = new HybrisWebReference.createQuote();
            arg.Quote = wsQuote;

            return arg;
        }

        private HybrisWebReference.createDAQuoteQuoteLine[] ConvertLines(ICollection<QuoteLine> quoteLines)
        {
            List<HybrisWebReference.createDAQuoteQuoteLine> results = new List<HybrisWebReference.createDAQuoteQuoteLine>();
            foreach (QuoteLine quoteLine in quoteLines)
            {
                HybrisWebReference.createDAQuoteQuoteLine wsQuoteLine = new HybrisWebReference.createDAQuoteQuoteLine();
                wsQuoteLine.SKU = quoteLine.SKU;
                wsQuoteLine.Quantity = quoteLine.Quantity;
                wsQuoteLine.ListPrice = quoteLine.ListPrice;
                wsQuoteLine.DiscountPercent = quoteLine.DiscountPercent;
                wsQuoteLine.PurchasePrice = quoteLine.PurchasePrice;
                wsQuoteLine.SellingPrice = quoteLine.SellingPrice;
                if (quoteLine.SetGlobalPoints != null)
                {
                    wsQuoteLine.GlobalPoints = quoteLine.SetGlobalPoints.ToString();
                }
               
                wsQuoteLine.ParentLine = quoteLine.ParentLine;
                wsQuoteLine.InternalRemarks = quoteLine.Remarks;
                if (quoteLine.ContractDuration != null)
                {
                    wsQuoteLine.ContractDuration = quoteLine.ContractDuration.ToString();
                    wsQuoteLine.ContractDurationUnit = quoteLine.ContractDurationUnit.ToString();
                }
                //wsQuoteLine.EndDate = quoteLine.EndDate;


                //VRFs
                List<HybrisWebReference.createDAQuoteQuoteLineVRF> itemLevel = new List<HybrisWebReference.createDAQuoteQuoteLineVRF>();
                List<HybrisWebReference.createDAQuoteQuoteLineVRF1> qtyLevel = new List<HybrisWebReference.createDAQuoteQuoteLineVRF1>();
                foreach (VRFValue vrfValue in quoteLine.VRFValues)
                {
                    if (vrfValue.VRFLevel.Equals("L"))//Line
                    {
                        HybrisWebReference.createDAQuoteQuoteLineVRF wsVrfValue = new HybrisWebReference.createDAQuoteQuoteLineVRF();
                        wsVrfValue.Field = vrfValue.VRF.Name;
                        wsVrfValue.Value = vrfValue.Value;
                        itemLevel.Add(wsVrfValue);
                    }
                    else if (vrfValue.VRFLevel.Equals("I"))//Line
                    {
                        HybrisWebReference.createDAQuoteQuoteLineVRF1 wsVrfValue = new HybrisWebReference.createDAQuoteQuoteLineVRF1();
                        wsVrfValue.Field = vrfValue.VRF.Name;
                        wsVrfValue.Value = vrfValue.Value;
                        qtyLevel.Add(wsVrfValue);
                    }
                }
                wsQuoteLine.ItemLevel = itemLevel.ToArray();
                wsQuoteLine.QtyLevel = qtyLevel.ToArray();

                results.Add(wsQuoteLine);
            }

            return results.ToArray();
        }
    }
}
