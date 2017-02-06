using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ImportRenewals.Contexts
{
    public class PrionContext : DbContext
    {
        public PrionContext() : base("ImportRenewals")
        {

        }

        public PrionContext(string connection) : base(connection)
        {

        }
        
    }
}