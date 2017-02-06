using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ImportRenewals.Models
{
    public class QuoteLine
    {
        [Key]
        public Int32 QuoteLineId { get; set; }
        public Int32 QuoteId{ get; set; }
        public String Hash { get; set; }
        public String SKU { get; set; }
        public Int32 Quantity { get; set; }
        public Decimal? ListPrice { get; set; } 
        public Decimal? DiscountPercent { get; set; }
        public Decimal? PurchasePrice { get; set; }
        public Decimal? SellingPrice { get; set; }
        public Decimal? SetGlobalPoints { get; set; } 
        public String ParentLine { get; set; }
        public String Remarks { get; set; }
        
        public double? ContractDuration { get; set; }
        public Char ContractDurationUnit { get; set; }
        public DateTime? EndDate { get; set; }

        [ForeignKey("QuoteId")]
        public virtual Quote Quote { get; set; }

        public virtual IEnumerable<VRFValue> VRFValues { get; set; }
    }
}