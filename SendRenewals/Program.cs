using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendRenewals
{
    class Program
    {
        static void Main(string[] args)
        {
            string vendorName = "Cisco";
            string region = "USA";

            Hybris hybris = new Hybris();
            hybris.SendQuotes(vendorName, region);
        }
    }
}
