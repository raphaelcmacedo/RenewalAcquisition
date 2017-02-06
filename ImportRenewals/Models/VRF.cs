using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImportRenewals.Models
{
    public class VRF
    {
        [Key]
        public Int32 VRFId { get; set; }
        public String Name { get; set; }
    }
}