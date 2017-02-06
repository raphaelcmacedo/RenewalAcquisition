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
using System.Web;

namespace ImportRenewals.Business
{
    public class ImportCsv
    {
        public Response CheckFile(Byte[] file,string fileName,string fileExtension)
        {
            Response resp = new Response();

            try
            {
                //Destaca o fabricante Cisco
                Vendor vendor = null;
                using (VendorRepository repository = new VendorRepository())
                {
                    vendor = repository.FindByName("Cisco");
                }

                using (StreamReader reader = new StreamReader(new MemoryStream(file)))
                {
                    String line = reader.ReadLine();
                    String[] fields = line.Split(',');
                    List<String> samples = new List<String>();

                    this.ValidateHeader(fields);
                    line = reader.ReadLine();

                    Regex RE = new Regex("\\n", RegexOptions.Multiline);
                    int count = RE.Matches(reader.ReadToEnd()).Count,
                        index = 0;

                    while (!String.IsNullOrEmpty(line) && index < 10)
                    {
                        samples.Add(line);
                    }

                    resp.Sample = samples.ToArray();
                    resp.Success = (count < 10000) ? true : false;
                    if (!resp.Success)
                    {
                        resp.TypeOfMessage = "Warning";
                        resp.Message = "This file may contain tens of thousand lines, so an email will be sent to you when the process is over";
                    }
                }
            }catch(Exception e)
            {
                resp.Success = false;
                resp.Message = e.Message;
                resp.TypeOfMessage = "Error";
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
                    "Quote Number",//16
                    "Cisco SO Number",//17
                    "End of Support Date",//18
                    "Distributor PO",//19
                    "Distributor Invoice #",//20
                    "Reseller BE GEO ID",//21
                    "Distributor Reseller ID",//22
                    "Reseller Bill To #",//23
                    "Reseller Name",//24
                    "Partner Type",//25
                    "Reseller PO",//26
                    "Reseller Address 1",//27
                    "Reseller Address 2",//28
                    "Reseller City",//29
                    "Reseller State",//30
                    "Reseller Postal Code",//31
                    "Reseller Country",//32
                    "Reseller Contact First Name",//33
                    "Reseller Contact Last Name",//34
                    "Reseller Contact Phone #",//35
                    "Reseller Contact Email",//36
                    "End Customer #",//37
                    "End Customer Name",//38
                    "End Customer Address 1",//39
                    "End Customer Address 2",//40
                    "End Customer City",//41
                    "End Customer State",//42
                    "End Customer Postal Code",//43
                    "End Customer Country",//44
                    "End Customer Contact First Name",//45
                    "End Customer Contact Last Name",//46
                    "End Customer Contact Phone #",//47
                    "End Customer Contact Email",//48
                    "End Customer Vertical",//49
                    "Reseller HQ Party ID",//50
                    "Reseller HQ Party Name",//51
                    "End Customer HQ Party ID",//52
                    "End Customer HQ Party Name",//53
                    "Quarter of Expiration",//54
                    "Install Site ID",//55
                    "End of Service Renewal Date",//56
                    "Quote Type",//57
                    "SN Validation",//58
                    "Install Site Validation",//59
                    "Auto Quote Number"//60
            };

            for(int i=0,len = expected.Count; i < len; i++)
            {
                if(expected[i] != fields[i].Trim())
                {
                    throw new Exception("The field "+ expected[i] + " is misplaced");
                }
            }
           
        }



        public void ReadFile(Byte[] file,string region)
        {

            try
            {
                using (StreamReader reader = new StreamReader(new MemoryStream(file)))
                {
                    String line = reader.ReadLine();
                    String[] fields = line.Split(',');
                    Quote quote = new Quote();
                    QuoteLine quoteLine = new QuoteLine();
                    DateTime start, end;
                    VRFValue item;
                    string beGeoId;

                    line = reader.ReadLine();                                      

                    while (!String.IsNullOrEmpty(line))
                    {
                        line = reader.ReadLine();
                        fields = line.Split(',');
                        quote.QuoteNumber = fields[17].ToString();
                        quote.CloseDate = this.ToDate(fields[19].ToString());
                        quote.QuoteType = "Renewal";
                        quote.CountryCode = region;
                        quote.ExpiryDate = this.ToDate(fields[58].ToString());
                        quote.QuoteRequesterName = fields[14].ToString();//ou 16

                        //Quoteline
                        quoteLine.SKU = fields[11].ToString();
                        start = this.ToDate(fields[6].ToString());
                        end = this.ToDate(fields[7].ToString());
                        quoteLine.ContractDuration = (end - start).TotalDays;
                        quoteLine.ContractDurationUnit = 'D';
                        quoteLine.Quantity = 1;//Campo não encontrado


                        //VRF Serial Number
                        /*string serialNumber = fields[12].ToString();
                        quoteLine.ItemLevel = new List<VRFValue>();
                        item = new VRFValue();
                        item.Value = serialNumber;
                        quoteLine.ItemLevel.Add(item);
                        quote.QuoteLines.Add(quoteLine);*/

                        //Companies
                        beGeoId = fields[22].ToString();
                        Company reseller = new Company();
                        reseller.VendorKey = beGeoId;
                        reseller.Name = fields[25].ToString();
                        reseller.Line1 = fields[28].ToString();
                        reseller.Line2 = fields[29].ToString();
                        reseller.State = fields[31].ToString();
                        reseller.City = fields[30].ToString();
                        reseller.Country = fields[33].ToString();
                        reseller.ZipCode = fields[32].ToString();
                        reseller.ContactName = fields[34].ToString() + " " + fields[35].ToString();
                        reseller.ContactEmail = fields[37].ToString();

                        quote.Reseller = reseller;
                        quote.ShipTo = reseller;
                        quote.BillTo = reseller;

                        Company endUser = new Company();
                        endUser.Name = fields[39].ToString();
                        endUser.Line1 = fields[40].ToString();
                        endUser.Line2 = fields[41].ToString();
                        endUser.City = fields[42].ToString();
                        endUser.State = fields[43].ToString();
                        endUser.Country = fields[45].ToString();
                        endUser.ZipCode = fields[44].ToString();
                        endUser.ContactName = fields[46].ToString() + " " + fields[41].ToString();
                        endUser.ContactEmail = fields[48].ToString();

                        quote.EndUser = endUser;

                    }

                }
            }
            catch (Exception e)
            {
               
            }
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

            return text.Trim();
        }
    }
}