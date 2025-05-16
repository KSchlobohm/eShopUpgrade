using System;
using Microsoft.AspNetCore.Owin;
using Microsoft.Owin;
using Owin;



[assembly: OwinStartupAttribute(typeof(eShopLegacyMVC.Startup))]
namespace eShopLegacyMVC
{
    public partial class Startup
    {
        // ConfigureAuth method definition
        public void ConfigureAuth(IAppBuilder app)
        {
            // Authentication configuration code will go here
            // For now, leaving it empty to fix the compilation error
        }

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}