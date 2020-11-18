using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QuizzApplication.Startup))]
namespace QuizzApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
