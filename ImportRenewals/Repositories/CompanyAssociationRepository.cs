using ImportRenewals.Contexts;
using ImportRenewals.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportRenewals.Repositories
{
    public class CompanyAssociationRepository : GenericRepository<CompanyAssociation>
    {
        public CompanyAssociationRepository(): base(new QuoteContext())
        {
            
        }

        public CompanyAssociation FindByVendorCode(string code, long vendorId, string region)
        {
            QuoteContext context = (QuoteContext)DbContext;
            CompanyAssociation companyAssociation = (from c in context.CompanyAssociations
                             where c.VendorCode.Equals(code) && c.Vendor.VendorId.Equals(vendorId) && c.Region.Equals(region)
                             select c).FirstOrDefault();
            return companyAssociation;

        }

    }
}