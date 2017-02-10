using ImportRenewals.Contexts;
using ImportRenewals.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportRenewals.Repositories
{
    public class CompanyRepository : GenericRepository<Company>
    {
        public CompanyRepository(): base(new QuoteContext())
        {
            
        }

        public Company FindByAssociationVendorCode(string code, long vendorId, string region)
        {
            QuoteContext context = (QuoteContext)DbContext;
            Company company = (from c in context.Companies
                             where c.CompanyAssociation.VendorCode.Equals(code) && c.CompanyAssociation.Vendor.VendorId.Equals(vendorId) && c.CompanyAssociation.Region.Equals(region)
                             select c).FirstOrDefault();
            return company;

        }

    }
}