using ImportRenewals.Models;
using ImportRenewals.Repositories;
using Prion.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace ImportRenewals.Business
{
    public class ImportCsv
    {

        private int count = 0;
        private int success = 0;
        private int error = 0;
        private List<String> message = new List<string>();
        private Vendor vendor = null;
        private Dictionary<string,VRF> VRFs = new Dictionary<string, VRF>();

        public Response CheckFile(string filePath)
        {
            Response resp = new Response();
            
            try
            {
                int index = 0;
                List<String> samples = new List<string>();
                foreach (var line in File.ReadLines(filePath))
                {
                    if (index == 0)
                    {
                        String[] fields = line.Split(',');
                        this.ValidateHeader(fields);
                        index++;
                        continue;
                    }
                    if (index == 11)
                    {
                        break;
                    }

                    samples.Add(line);
                    index++;
                }

                resp.Sample = samples.ToArray();
                resp.Success = true;
                
            }catch(Exception e)
            {
                resp.Success = false;
                resp.Message = e.Message;
                resp.TypeOfMessage = "Error";
            }

            if (resp.Success)
            {
                long fileSize = new System.IO.FileInfo(filePath).Length;
                long limit = 10 * 1024 * 1024;//10 MB

                resp.Success = (fileSize > limit);
                if (!resp.Success)
                {
                    resp.TypeOfMessage = "Warning";
                    resp.Message = "This file is too big, so an email will be sent to you when the process is over";
                }
               
            }

            return resp;            
        }

        public void ValidateHeader(String[] fields)
        {
            List<String> expected = new List<String>()
                {
                    "Distributor ID",//0
                    "Distributor Name",//1
                    "Line Item List Price-Historical",//2
                    "Line Item Net Price-Historical",//3
                    "Contract List Price-Historical",//4
                    "Contract Net Price-Historical",//5
                    "Coverage Start Date",//6
                    "Coverage End Date",//7
                    "Contract #",//8
                    "Service Level",//9
                    "Item Name",//10
                    "Service Part #",//11
                    "Serial Number for Device",//12
                    "Instance ID",//13
                    "Quote Created By",//14
                    "Quote Ordered By",//15
                    "Quote Coverage Conversion Date", //16
                    "Quote Number",//17
                    "Cisco SO Number",//18
                    "End of Support Date",//19
                    "Distributor PO",//20
                    "Distributor Invoice #",//21
                    "Reseller BE GEO ID",//22
                    "Distributor Reseller ID",//23
                    "Reseller Bill To #",//24
                    "Reseller Name",//25
                    "Partner Type",//26
                    "Reseller PO",//27
                    "Reseller Address 1",//28
                    "Reseller Address 2",//29
                    "Reseller City",//30
                    "Reseller State",//31
                    "Reseller Postal Code",//32
                    "Reseller Country",//33
                    "Reseller Contact First Name",//34
                    "Reseller Contact Last Name",//35
                    "Reseller Contact Phone #",//36
                    "Reseller Contact Email",//37
                    "End Customer #",//38
                    "End Customer Name",//39
                    "End Customer Address 1",//40
                    "End Customer Address 2",//41
                    "End Customer City",//42
                    "End Customer State",//43
                    "End Customer Postal Code",//44
                    "End Customer Country",//45
                    "End Customer Contact First Name",//46
                    "End Customer Contact Last Name",//47
                    "End Customer Contact Phone #",//48
                    "End Customer Contact Email",//49
                    "End Customer Vertical",//50
                    "Reseller HQ Party ID",//51
                    "Reseller HQ Party Name",//52
                    "End Customer HQ Party ID",//54
                    "End Customer HQ Party Name",//55
                    "Quarter of Expiration",//56
                    "Install Site ID",//57
                    "End of Service Renewal Date",//58
                    "Quote Type",//59
                    "SN Validation",//60
                    "Install Site Validation",//61
                    "Auto Quote Number"//62
            };

            for(int i=0,len = expected.Count; i < len; i++)
            {
                if(expected[i] != fields[i].Trim())
                {
                    throw new Exception("The field "+ expected[i] + " is misplaced");
                }
            }
           
        }



        public Response ReadFile(string filePath,string region,bool async,string email)
        {
            Response resp = new Response();
            if (async)
            {
                resp.Success = true;
                resp.Message = "You will receive an email with more information when the process ends.";
                System.Threading.Thread thread = new Thread(new ParameterizedThreadStart(s => GetData(filePath, region,email)));
                thread.SetApartmentState(ApartmentState.STA);

                thread.Start();
                thread.Join();
            }
            else
            {
                this.GetData(filePath, region,email);
                resp.Success = true;
                resp.Message = "The data was saved in our database";
            }

            return resp;
           
        }

        public void GetData(string filePath, string region, string email)
        {
            
            
            Dictionary<string, Quote> quoteDictionary = new Dictionary<string, Quote>();

            try
            {
                //Destaca o fabricante Cisco
                
                using (VendorRepository repository = new VendorRepository())
                {
                    vendor = repository.FindByName("Cisco");
                }


                FillVRFs();

                foreach (var line in File.ReadLines(filePath))
                {
                    if (String.IsNullOrEmpty(line) || line.StartsWith("Distributor ID"))
                    {
                        continue;
                    }
                    
                    String[] fields = line.Split(',');
                    int columns = fields.Length;
                    count++;

                    if(fields.Length != columns)
                    {
                        error++;
                        message.Add("Error at line " + count + " - An unexpected character was found.\n" + line);
                        continue;
                    }

                    string quoteNumber = this.AdjustText(fields[17].ToString());

                    Quote quote = null;
                    if (quoteDictionary.ContainsKey(quoteNumber))
                    {//Use the quote already created and stored on the Dictionary
                        quote = quoteDictionary[quoteNumber];
                    }
                    else
                    {//Try to create a new quote
                        try
                        {
                            quote = this.ReadQuote(fields, region);
                            quoteDictionary[quoteNumber] = quote;
                        }
                        catch (Exception e)
                        {
                            error++;
                            message.Add(e.Message);
                            continue;
                        }
                    }

                    QuoteLine quoteLine = this.ReadQuoteLine(fields, quote);
                    if (quote.QuoteLines == null)
                    {
                        quote.QuoteLines = new List<QuoteLine>();
                    }
                    quote.QuoteLines.Add(quoteLine);
                        
                    success++;
                        
                }

                //Save all the quotes on the Repository
                using (QuoteRepository repository = new QuoteRepository())
                {
                    foreach (KeyValuePair<string, Quote> dict in quoteDictionary)
                    {
                        repository.Add(dict.Value);
                    }
                }

                Email.Renewal mail = new Email.Renewal();
                mail.Success(message, success, error, email);
                
            }
            catch (Exception e)          
            {
                Email.Renewal mail = new Email.Renewal();
                mail.Error(e.Message);
            }
        }

        private void FillVRFs()
        {
            using (VRFRepository repository = new VRFRepository())
            {
                VRF serialNumber = repository.FindByName("VRF_SERIAL_NUMBER_2");
                VRF vendorQuoteNumber = repository.FindByName("VRF_VENDOR_QUOTE_NUMBER");
                VRF licStartDate = repository.FindByName("VRF_LIC_START_DATE");
                VRF licEndDate = repository.FindByName("VRF_LIC_END_DATE");
                VRF newRenew = repository.FindByName("VRF_NEW_RENEW");

                VRFs.Add("VRF_SERIAL_NUMBER_2", serialNumber);
                VRFs.Add("VRF_VENDOR_QUOTE_NUMBER", vendorQuoteNumber);
                VRFs.Add("VRF_LIC_START_DATE", licStartDate);
                VRFs.Add("VRF_LIC_END_DATE", licEndDate);
                VRFs.Add("VRF_NEW_RENEW", newRenew);
            }


        }

        private Quote ReadQuote(string[] fields, string region)
        {
            Quote quote = new Quote();
            quote.QuoteNumber = this.AdjustText(fields[17].ToString());

            try
            {
                quote.CloseDate = this.ToDate(this.AdjustText(fields[7].ToString()));
            }
            catch
            {
                throw new Exception("Error at line " + count + " - The Close Date for Quote Number " + quote.QuoteNumber + " wasn't an expected format.\n");
            }

            try
            {
                quote.ExpiryDate = this.ToDate(this.AdjustText(fields[19].ToString()));
            }
            catch
            {
                throw new Exception("Error at line " + count + " - The Expiry Date for Quote Number " + quote.QuoteNumber + " wasn't an expected format.\n");
            }

            quote.QuoteType = "Renewal";
            quote.Vendor = vendor;
            quote.Region = region;
            quote.CountryCode = region;
            quote.QuoteRequesterName = this.AdjustText(fields[14].ToString());//ou 16

            //Companies
            string beGeoId = this.AdjustText(fields[22].ToString());
            if (String.IsNullOrEmpty(beGeoId))
            {
                throw new Exception("Error at line " + count + " - BE GEO ID was not found for Quote Number: " + quote.QuoteNumber + ".\n");
            }

            Company reseller = null;
            //Verify if there is a company with this vendor code
            using (CompanyRepository repository = new CompanyRepository())
            {
                reseller = repository.FindByAssociationVendorCode(beGeoId, vendor.VendorId, region);
            }
            if (reseller == null)
            {
                reseller = new Company();
                reseller.Name = this.AdjustText(fields[25].ToString());
                reseller.Line1 = this.AdjustText(fields[28].ToString());
                reseller.Line2 = this.AdjustText(fields[29].ToString());
                reseller.State = this.AdjustText(fields[31].ToString());
                reseller.City = this.AdjustText(fields[30].ToString());
                reseller.Country = this.AdjustText(fields[33].ToString());
                reseller.ZipCode = this.AdjustText(fields[32].ToString());
                reseller.ContactName = this.AdjustText(fields[34].ToString()) + " " + this.AdjustText((fields[35].ToString()));
                reseller.ContactEmail = this.AdjustText(fields[37].ToString());
            }
            

            quote.Reseller = reseller;
            quote.ShipTo = reseller;
            quote.BillTo = reseller;

            Company endUser = new Company();
            endUser.Name = this.AdjustText(fields[39].ToString());
            endUser.Line1 = this.AdjustText(fields[40].ToString());
            endUser.Line2 = this.AdjustText(fields[41].ToString());
            endUser.City = this.AdjustText(fields[42].ToString());
            endUser.State = this.AdjustText(fields[43].ToString());
            endUser.Country = this.AdjustText(fields[45].ToString());
            endUser.ZipCode = this.AdjustText(fields[44].ToString());
            endUser.ContactName = this.AdjustText(fields[46].ToString()) + " " + this.AdjustText(fields[41].ToString());
            endUser.ContactEmail = this.AdjustText(fields[48].ToString());

            quote.EndUser = endUser;

            return quote;
        }

        private QuoteLine ReadQuoteLine(string[] fields, Quote quote)
        {
            QuoteLine quoteLine = new QuoteLine();

            quoteLine.SKU = this.AdjustText(fields[11].ToString());
            DateTime start = this.ToDate(this.AdjustText(fields[6].ToString()));
            DateTime end = this.ToDate(this.AdjustText(fields[7].ToString()));
            double contractDuration = (end - start).TotalDays;

            quoteLine.ContractDuration = (Decimal) contractDuration;
            quoteLine.ContractDurationUnit = "D";
            quoteLine.Quantity = 1;//Campo não encontrado

            //VRFs
            List<VRFValue> vrfValues = new List<VRFValue>();

            //VRF Serial Number
            string serialNumber = fields[12].ToString();
            vrfValues.Add(this.CreateVRFValue("VRF_SERIAL_NUMBER_2", serialNumber, "I"));

            //VRF Vendor Quote Number
            vrfValues.Add(this.CreateVRFValue("VRF_VENDOR_QUOTE_NUMBER", quote.QuoteNumber , "L"));

            //VRF Lic Start and End Date
            DateTime licStartDate = end.AddDays(1);
            DateTime licEndDate = end.AddDays(contractDuration);
            vrfValues.Add(this.CreateVRFValue("VRF_LIC_START_DATE", licStartDate.ToString(), "L"));
            vrfValues.Add(this.CreateVRFValue("VRF_LIC_END_DATE", licEndDate.ToString(), "L"));

            //VRF Renew
            vrfValues.Add(this.CreateVRFValue("VRF_NEW_RENEW", "Renew", "L"));

            quoteLine.VRFValues = vrfValues;
            

            return quoteLine;
        }

        private VRFValue CreateVRFValue(string vrf, string value, string level)
        {
            VRFValue vrfValue = new VRFValue();
            vrfValue.VRF = VRFs[vrf];
            vrfValue.Value = value;
            vrfValue.VRFLevel = level;

            return vrfValue;
        }

        private DateTime ToDate(string value)
        {
            return Conversor.ToDateTime(AdjustText(value));
        }

        private decimal ToDecimal(string value)
        {
            //Limpa a string
            value = value.Replace("$", "");
            value = value.Replace("-", "");
            value = value.Replace("%", "");
            value = value.Replace("(", "");
            value = value.Replace(")", "");
            value = value.Trim();
            value = AdjustText(value);

            //Realiza a conversão
            if (!String.IsNullOrEmpty(value))
            {
                return decimal.Parse(value, new CultureInfo("en-US"));
            }
            else
            {
                return 0;
            }
        }

        private int ToInt(string value)
        {
            //Limpa a string
            value = value.Replace(".00", "");
            value = value.Replace(".0", "");
            value = value.Replace("-", "");
            value = value.Trim();
            value = AdjustText(value);

            //Realiza a conversão
            if (!String.IsNullOrEmpty(value))
            {
                return int.Parse(value, new CultureInfo("en-US"));
            }
            else
            {
                return 0;
            }
        }

        private string AdjustText(string text)
        {
            if (String.IsNullOrEmpty(text)) return "";

            text = text.Contains("\n") ? text.Replace("\n", "") : text;
            text = text.Contains("\t") ? text.Replace("\t", "") : text;
            text = text.Contains("\r") ? text.Replace("\r", "") : text;
            text = text.Contains("&nbsp;") ? text.Replace("&nbsp;", "") : text;
            text = text.Contains("\"") ? text.Replace("\"", "") : text;

            return text.Trim();
        }
    }
}