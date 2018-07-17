using System.Threading;
using Microsoft.Owin;
using Owin;
using Umbraco.Web;

[assembly: OwinStartup(typeof(Compent.Uintra.Startup))]

namespace Compent.Uintra
{
    public class Startup : UmbracoDefaultOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            base.Configuration(app);
            app.MapSignalR();
        }
    }
}
