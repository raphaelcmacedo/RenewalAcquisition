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
        public Response CheckFile(Byte[] file,string fileName,string fileExtension)
        {
            Response resp = new Response();
            int count = 0;
            try
            {
                using (StreamReader reader = new StreamReader(new MemoryStream(file)))
                {
                    String line = reader.ReadLine();
                    String[] fields = line.Split(',');
                    List<String> samples = new List<String>();

                    this.ValidateHeader(fields);
                    line = reader.ReadLine();
                    int index = 0;

                    while (!String.IsNullOrEmpty(line) && index < 10)
                    {
                        samples.Add(line);
                        line = reader.ReadLine();
                        index++;
                    }

                    resp.Sample = samples.ToArray();
                    resp.Success = true;
                }
            }catch(Exception e)
            {
                resp.Success = false;
                resp.Message = e.Message;
                resp.TypeOfMessage = "Error";
            }

            if (resp.Success)
            {
                using (StreamReader reader = new StreamReader(new MemoryStream(file)))
                {
                    String full = reader.ReadToEnd();
                    count = full.Split('\n').Length - 1;

                    resp.Success = (count < 10000) ? true : false;
                    if (!resp.Success)
                    {
                        resp.TypeOfMessage = "Warning";
                        resp.Message = "This file may contain tens of thousand lines, so an email will be sent to you when the process is over";
                    }
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



        public Response ReadFile(Byte[] file,string region,bool async)
        {
            Response resp = new Response();
            if (async)
            {
                resp.Success = true;
                resp.Message = "You will receive an email with more information when the process ends.";
                System.Threading.Thread thread = new Thread(new ParameterizedThreadStart(s => GetData(file, region)));
                thread.SetApartmentState(ApartmentState.STA);

                thread.Start();
                thread.Join();
            }else
            {
                this.GetData(file, region);
                resp.Success = true;
                resp.Message = "The data was saved in our database";
            }

            return resp;
           
        }

        public void GetData(Byte[] file, string region)
        {
            Int32 success = 0, error = 0, count = 0;
            List<String> message = new List<string>();
            try
            {
                //Destaca o fabricante Cisco
                Vendor vendor = null;
                using (VendorRepository repository = new VendorRepository())
                {
                    vendor = repository.FindByName("Cisco");
                }

                VRF vrfSerialNumber = null;
                using (VRFRepository repository = new VRFRepository())
                {
                    vrfSerialNumber = repository.FindByName("VRF_SERIAL_NUMBER_2");
                }


                using (StreamReader reader = new StreamReader(new MemoryStream(file)))
                {
                    String line = reader.ReadLine();
                    String[] fields = line.Split(',');
                    Quote quote = new Quote();
                    QuoteLine quoteLine = new QuoteLine();
                    DateTime start, end;

                    string beGeoId;

                    line = reader.ReadLine();
                    int columns = line.Split(',').Length;

                    line = reader.ReadLine();

                    while (!String.IsNullOrEmpty(line))
                    {                        
                        fields = line.Split(',');
                        count++;

                        if(fields.Length != columns)
                        {
                            error++;
                            message.Add("Error at line " + count + " - An unexpected character was found.\n" + line);
                            line = reader.ReadLine();
                            continue;
                        }

                        quote.QuoteNumber = this.AdjustText(fields[17].ToString());
                        try
                        {
                            quote.CloseDate = this.ToDate(this.AdjustText(fields[19].ToString()));
                        }
                        catch
                        {
                            error++;
                            message.Add("Error at line " + count + " - The Close Date for Quote Number " + quote.QuoteNumber + " wasn't an expected format.\n" + line);
                            line = reader.ReadLine();
                            continue;
                        }

                        try
                        {
                            quote.ExpiryDate = this.ToDate(this.AdjustText(fields[57].ToString()));
                        }
                        catch
                        {
                            error++;
                            message.Add("Error at line " + count + " - The Expiry Date for Quote Number " + quote.QuoteNumber + " wasn't an expected format.\n" + line);
                            line = reader.ReadLine();
                            continue;
                        }

                        quote.QuoteType = "Renewal";
                        quote.Vendor = vendor;
                        quote.CountryCode = region;
                        quote.QuoteRequesterName = this.AdjustText(fields[14].ToString());//ou 16

                        //Quoteline
                        quoteLine.SKU = this.AdjustText(fields[11].ToString());
                        start = this.ToDate(this.AdjustText(fields[6].ToString()));
                        end = this.ToDate(this.AdjustText(fields[7].ToString()));
                        quoteLine.ContractDuration = (end - start).TotalDays;
                        quoteLine.ContractDurationUnit = 'D';
                        quoteLine.Quantity = 1;//Campo não encontrado

                        //VRFs
                        List<VRFValue> vrfValues = new List<VRFValue>();

                        //VRF Serial Number
                        string serialNumber = fields[12].ToString();
                        VRFValue vrfValue = new VRFValue();
                        vrfValue.VRF = vrfSerialNumber;
                        vrfValues.Add(vrfValue);

                        quoteLine.VRFValues = vrfValues;

                        if (quote.QuoteLines == null)
                        {
                            quote.QuoteLines = new List<QuoteLine>();
                        }
                        quote.QuoteLines.Add(quoteLine);

                        //Companies
                        beGeoId = this.AdjustText(fields[22].ToString());
                        if (String.IsNullOrEmpty(beGeoId))
                        {
                            error++;
                            message.Add("Error at line " + count + " - BE GEO ID was not found for Quote Number: " + quote.QuoteNumber + ".\n" + line);
                            line = reader.ReadLine();
                            continue;
                        }

                        Company reseller = new Company();
                        //reseller.VendorKey = beGeoId;
                        reseller.Name = this.AdjustText(fields[25].ToString());
                        reseller.Line1 = this.AdjustText(fields[28].ToString());
                        reseller.Line2 = this.AdjustText(fields[29].ToString());
                        reseller.State = this.AdjustText(fields[31].ToString());
                        reseller.City = this.AdjustText(fields[30].ToString());
                        reseller.Country = this.AdjustText(fields[33].ToString());
                        reseller.ZipCode = this.AdjustText(fields[32].ToString());
                        reseller.ContactName = this.AdjustText(fields[34].ToString()) + " " + this.AdjustText((fields[35].ToString()));
                        reseller.ContactEmail = this.AdjustText(fields[37].ToString());

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

                        //Grava a quote
                        using (QuoteRepository repository = new QuoteRepository())
                        {
                            repository.Add(quote);
                        }

                        success++;
                        line = reader.ReadLine();
                    }

                }
            }catch(Exception e)          
            {
                throw e;
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