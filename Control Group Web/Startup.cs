using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Control_Group_Web.Startup))]
namespace Control_Group_Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
