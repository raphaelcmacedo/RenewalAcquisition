using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportRenewals.Models
{
    public class Response
    {
        public Boolean Success { get; set; }
        public String Message { get; set; }
        public String TypeOfMessage { get; set; }
        public String[] Sample { get; set; }
    }
}