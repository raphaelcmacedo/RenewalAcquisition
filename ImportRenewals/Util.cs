using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportRenewals
{
    public static class Util
    {
        public static string GenerateHash()
        {
            return Guid.NewGuid().ToString();
        }
    }
}