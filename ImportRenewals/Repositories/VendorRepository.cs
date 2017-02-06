using ImportRenewals.Contexts;
using ImportRenewals.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportRenewals.Repositories
{
    public class VendorRepository : GenericRepository<Vendor>
    {
        public VendorRepository(): base(new QuoteContext())
        {
            
        }

        public Vendor FindByName(string name)
        {
            QuoteContext context = (QuoteContext)DbContext;
            Vendor vendor = (from v in context.Vendors
                             where v.Name.Equals(name)
                             select v).FirstOrDefault();
            return vendor;

        }

    }
}