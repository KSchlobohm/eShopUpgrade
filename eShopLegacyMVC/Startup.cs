using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using System.Data.Entity;
using log4net;
using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eShopLegacyMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddResponseCaching();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddScoped<CatalogService>();
            services.AddScoped<CatalogServiceMock>();
            services.AddScoped(sp =>
            {
                var useMockData = Configuration.GetValue<bool>("appsettings:UseMockData");
                return useMockData
                    ? sp.GetRequiredService<CatalogServiceMock>() as ICatalogService
                    : sp.GetRequiredService<CatalogService>();
            });
            services.AddScoped(sp => new CatalogDBContext(
                sp.GetRequiredService<IConfiguration>().GetConnectionString("CatalogDBContext")
                ?? string.Empty));
            services.AddScoped<CatalogDBInitializer>();
            services.AddSingleton<CatalogItemHiLoGenerator>();

            // Identity-related services
            services.AddDbContext<IdentityDbContext<ApplicationUser>>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("IdentityDBContext"));
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext<ApplicationUser>>()
                .AddDefaultTokenProviders();

            services.AddSystemWebAdapters();

            // Add bundling and minification services and configuration
            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddJavaScriptBundle("/bundles/jquery", "wwwroot/Scripts/jquery-*.js")
                    .UseContentRoot();

                pipeline.AddJavaScriptBundle("/bundles/jqueryval", "wwwroot/Scripts/jquery.validate*")
                    .UseContentRoot();

                pipeline.AddJavaScriptBundle("/bundles/modernizr", "wwwroot/Scripts/modernizr-*")
                    .UseContentRoot();

                pipeline.AddJavaScriptBundle("/bundles/bootstrap", "wwwroot/Scripts/bootstrap.js", "wwwroot/Scripts/respond.js")
                    .UseContentRoot();

                pipeline.AddCssBundle("/Content/css",
                                      "wwwroot/Content/bootstrap.css",
                                      "wwwroot/Content/custom.css",
                                      "wwwroot/Content/base.css",
                                      "wwwroot/Content/site.css")
                    .UseContentRoot();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Set EF6 DB initializer
            var mockData = Configuration.GetValue<bool>("DataSettings:UseMockData");
            if (!mockData)
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    Database.SetInitializer(services.GetRequiredService<CatalogDBInitializer>());
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpsRedirection();
            app.UseWebOptimizer();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseResponseCaching();
            app.UseSystemWebAdapters();

            app.UseSession();

            // Middleware for implementing functionality that used to live in custom HttpApplication
            app.Use(async (context, next) =>
            {
                var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

                // Set LogicalThreadContext
                LogicalThreadContext.Properties["activityid"] = new ActivityIdHelper();
                LogicalThreadContext.Properties["requestinfo"] = new WebRequestInfo(context);

                log.Debug("WebApplication_BeginRequest");

                // Session_Start
                if (context.Session.IsAvailable && !context.Session.Keys.Contains("SessionStartTime"))
                {
                    context.Session.SetString("MachineName", Environment.MachineName);
                    context.Session.SetString("SessionStartTime", DateTime.Now.ToString());
                }

                await next();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Catalog}/{action=Index}/{id?}");
            });
        }
    }
}