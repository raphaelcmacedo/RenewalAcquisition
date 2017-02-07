using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ImportRenewals.Models
{
    public class VRFValue
    {
        [Key]
        public Int64 VRFValuelId { get;set;}
        public Int64 QuoteLineId { get; set; }
        public Int64 VRFId { get; set; }
        public string Value { get; set; }
        public char VRFLevel { get; set; }//H: Header, L: Line, I: Item

        [ForeignKey("QuoteLineId")]
        public virtual QuoteLine QuoteLine { get; set; }
        [ForeignKey("VRFId")]
        public virtual VRF VRF { get; set; }
    }
}