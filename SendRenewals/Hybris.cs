using ImportRenewals.Models;
using ImportRenewals.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SendRenewals
{
    public class Hybris
    {
        private static int limit = 10;
        CultureInfo cultureInfo = new CultureInfo("en-US");
        private List<string> allowedSKUs = new List<string>() {
            "CON -SNT-A85S2F20","PVDM4-32U64","L-N55-LAN1K9=","PWR-C45-2800ACV","CON-ECDN-CTSSX860","CON-SNTP-A85S2K9","L-ASA5585-20-TA1Y","ISE-5VM-K9=","CON-ECMU-MIGCUC87",
            "PVDM3-16U64","DCNM-LAN-N5K-K9","SPA-4XOC3-POS-V2","MCP-BASE-10X-LIC","SM-ES2-16-P","WSA-WSP-SMS-1","WS-C2960X-48TDL-RF","SM-ES2-24=","PI-UCS-APL-K9","FP7050-TA-1Y",
            "N3K-C3524-X-SPL3","FL-CME-SRST-25=","CTS-CTRL-DV10=","FL-SL-IPV-POL-100=","ISE-ADV-3YR-1K","UCS-SA-C240M3S-101","FLSASR1-IOSRED","N2K-C2248TF-1GE","CWS-1Y-S4",
            "PVDM4-128=","L-CCX-90-A-AQM-LIC","SFP-10G-LR-X=","WS-C4507R+E=","CON-SU3-SMS-1000","SASR1R1-AESK9-313S","CAB-STK-E-1M=","UPG-6K-VM","WS-C2960XR-24PD-I",
            "WS-X4712-SFP+E","L-WBX-TOLLUSER-NY2","C6800-48P-SFP","FP8260-TA-1Y"
        };


        public void SendQuotes(string vendorName, string region)
        {
            string log = Environment.CurrentDirectory + "\\Log.txt";          
            try
            {
                using (QuoteRepository repository = new QuoteRepository())
                {
                    List<Quote> quotes = repository.FindByVendorAndRegion(vendorName, region, limit);

                    HybrisWebReference.B2BDAQuotesImplService webService = new HybrisWebReference.B2BDAQuotesImplService();
                    //X509Certificate2 cert = new X509Certificate2();
                    //string certPath = Environment.CurrentDirectory + "\\ClientKeyStore.pfx";
                    //cert.Import(certPath, "123456", X509KeyStorageFlags.DefaultKeySet);
                    //webService.ClientCertificates.Add(cert);
                    int i = 0;
                    foreach (Quote quote in quotes)
                    {
                        try
                        {
                            HybrisWebReference.createQuote arg = this.ConvertQuote(quote, vendorName);      
                            i++;
                            Console.WriteLine(i + "- Items: " + arg.Quote.QuoteLines.Length.ToString());
                            webService.createQuote(arg);
                            File.AppendAllText(log, i + " - Quote " + quote.QuoteNumber + " generated at " + DateTime.Now.ToString() + System.Environment.NewLine);
                        }
                        catch(Exception e)
                        {
                            if(e.Message.Contains("Duplicate unique ID \n"))
                            {
                                File.AppendAllText(log, e.Message);
                                continue;
                            }else { throw e; }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                File.AppendAllText(log, e.Message);
                throw e;
            }
        }

        private HybrisWebReference.createQuote ConvertQuote(Quote quote,String vendorName)
        {
            HybrisWebReference.createDAQuote wsQuote = new HybrisWebReference.createDAQuote();
            wsQuote.UniqueID = quote.Hash;
            wsQuote.ExpiryDate = quote.ExpiryDate;
            wsQuote.CloseDate = quote.CloseDate;
            wsQuote.CloseDateSpecified = true;
            //wsQuote.Currency = quote.Currency;
            wsQuote.Currency = "USD";
            wsQuote.InternalRemarks = quote.InternalRemarks;
            wsQuote.ExternalRemarks = quote.ExternalRemarks;
            //wsQuote.QuoteType = quote.QuoteType;
            wsQuote.CountryCode = quote.CountryCode;
            //wsQuote.SalesOrg = quote.SalesOrg;
            wsQuote.SalesOrg = "2000";
            wsQuote.VendorName = vendorName;
            //Requester
            wsQuote.QuoteRequester = new HybrisWebReference.createDAQuoteQuoteRequester();
            wsQuote.QuoteRequester.name = quote.QuoteRequesterName;
            wsQuote.QuoteRequester.email = "test@mail.com";//quote.QuoteRequesterEmail;

            //Reseller
            if (quote.Reseller != null)
            {
                wsQuote.Reseller = new HybrisWebReference.createDAQuoteReseller();
                if (quote.Reseller.CompanyAssociation != null)
                {
                    wsQuote.Reseller.SAPcompanyID = "0001001030";//quote.Reseller.CompanyAssociation.WestconCode.PadLeft(10,'0');
                }
            }

            //Bill To
            if (quote.BillTo != null)
            {
                wsQuote.BillTo = new HybrisWebReference.createDAQuoteBillTo();
                if (quote.BillTo.CompanyAssociation != null)
                {
                    wsQuote.BillTo.SAPcompanyID = "0001001030";//quote.BillTo.CompanyAssociation.WestconCode.PadLeft(10, '0');
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
                    wsQuote.ShipTo.SAPcompanyID = "0001001030";//quote.ShipTo.CompanyAssociation.WestconCode.PadLeft(10, '0');
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
                    wsQuote.EndUser.SAPcompanyID = quote.EndUser.CompanyAssociation.WestconCode.PadLeft(10, '0');
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
            Random rnd = new Random();
            foreach (QuoteLine quoteLine in quoteLines)
            {
                HybrisWebReference.createDAQuoteQuoteLine wsQuoteLine = new HybrisWebReference.createDAQuoteQuoteLine();
                wsQuoteLine.SKU = allowedSKUs[rnd.Next(0, allowedSKUs.Count)]; //quoteLine.SKU;
                wsQuoteLine.Quantity = quoteLine.Quantity;
                wsQuoteLine.ListPrice = quoteLine.ListPrice;
                wsQuoteLine.DiscountPercent = quoteLine.DiscountPercent;
                wsQuoteLine.PurchasePrice = quoteLine.PurchasePrice;
                wsQuoteLine.SellingPrice = quoteLine.SellingPrice;

                if (quoteLine.SetGlobalPoints != null)
                {
                    decimal globalPoints = quoteLine.SetGlobalPoints ?? 0;
                    wsQuoteLine.GlobalPoints = globalPoints.ToString(cultureInfo);
                    wsQuoteLine.GlobalPoints = quoteLine.SetGlobalPoints.ToString();
                }
               
                wsQuoteLine.ParentLine = quoteLine.ParentLine;
                wsQuoteLine.InternalRemarks = quoteLine.Remarks;
                if (quoteLine.ContractDuration != null)
                {
                    decimal contractDuration = quoteLine.ContractDuration ?? 0;
                    wsQuoteLine.ContractDuration = contractDuration.ToString(cultureInfo);
                    wsQuoteLine.ContractDurationUnit = quoteLine.ContractDurationUnit;
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
