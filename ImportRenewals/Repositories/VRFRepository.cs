using ImportRenewals.Contexts;
using ImportRenewals.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportRenewals.Repositories
{
    public class VRFRepository : GenericRepository<VRF>
    {
        public VRFRepository(): base(new QuoteContext())
        {
            
        }

        public VRF FindByName(string name)
        {
            QuoteContext context = (QuoteContext)DbContext;
            VRF vrf = (from v in context.VRFs
                             where v.Name.Equals(name)
                             select v).FirstOrDefault();
            return vrf;

        }

    }
}