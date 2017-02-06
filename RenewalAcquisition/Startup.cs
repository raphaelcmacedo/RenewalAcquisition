using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RenewalAcquisition.Startup))]
namespace RenewalAcquisition
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
