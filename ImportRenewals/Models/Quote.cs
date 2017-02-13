using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ImportRenewals.Models
{
    public class Quote
    {
        [Key]
        public Int64 QuoteId { get; set; }
        public String QuoteNumber { get; set; }
        public String Hash { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CloseDate { get; set; }
        public String Currency { get; set; }
        public String InternalRemarks { get; set; }
        public String ExternalRemarks { get; set; }
        public String QuoteType { get; set; }
        public String CountryCode { get; set; }
        public String SalesOrg { get; set; }
        public String Region { get; set; }

        public string QuoteRequesterName { get; set; }
        public string QuoteRequesterEmail { get; set; }

        public Int64? EndUserId { get; set; }
        public Int64? BillToId { get; set; }
        public Int64? ShipToId { get; set; }
        public Int64? ResellerId { get; set; }
        public Int64? VendorId { get; set; }
        
        [ForeignKey("EndUserId")]
        public virtual Company EndUser { get; set; }
        [ForeignKey("BillToId")]
        public virtual Company BillTo { get; set; }
        [ForeignKey("ShipToId")]
        public virtual Company ShipTo { get; set; }
        [ForeignKey("ResellerId")]
        public virtual Company Reseller { get; set; }
        [ForeignKey("VendorId")]
        public virtual Vendor Vendor { get; set; }

        public virtual ICollection<QuoteLine> QuoteLines { get; set; }

    }
}