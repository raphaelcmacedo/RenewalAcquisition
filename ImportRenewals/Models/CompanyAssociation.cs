using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ImportRenewals.Models
{
    public class CompanyAssociation
    {
        [Key]
        public Int64 CompanyAssociationId { get; set; }
        public Int64 VendorId { get; set; }
        public String Region { get; set; }
        public String VendorCode { get; set; }
        public String WestconCode { get; set; }

        [ForeignKey("VendorId")]
        public virtual Vendor Vendor { get; set; }

    }
}
