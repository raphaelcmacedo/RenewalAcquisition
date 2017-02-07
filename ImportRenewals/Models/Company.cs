using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImportRenewals.Models
{
    public class Company
    {
        [Key]
        public Int64 CompanyId { get; set; }
        public string Hash { get; set; }
        public string WestconKey { get; set; }//Para os casos do arquivo DePara, representa o Be GEO ID
        public string VendorKey { get; set; }//Para os casos do arquivo DePara, representa o Be ID                                         
        public String Name { get; set; }
        public String Line1 { get; set; }
        public String Line2 { get; set; }
        public String Line3 { get; set; }
        public String City { get; set; }
        public String State { get; set; }
        public String Country { get; set; }
        public String ZipCode { get; set; }       

        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
    }
}