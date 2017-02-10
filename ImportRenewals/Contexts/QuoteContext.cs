using ImportRenewals.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ImportRenewals.Contexts
{
    public class QuoteContext:PrionContext
    {
        //Sets
        public DbSet<Company> Companies { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteLine> QuoteLines { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VRF> VRFs { get; set; }
        public DbSet<VRFValue> VRFValues { get; set; }
        public DbSet<CompanyAssociation> CompanyAssociations { get; set; }
    }
}