using eShopLegacyMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using log4net;
using System.Reflection;
using System.Data.Entity;
using eShopLegacyMVC.Models.Infrastructure;
using System.Runtime.Versioning;
using Microsoft.Extensions.FileProviders;
using eShopLegacyMVC.Services;

namespace eShopLegacyMVC
{
    [SupportedOSPlatform("windows")]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.AddCatalogServices();
            builder.Services.Configure<MessageQueueSettings>(builder.Configuration.GetSection("MessageQueueSettings"));
            builder.Services.AddControllersWithViews();
            builder.Services.AddResponseCaching();

            // Add session-related services
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            // Identity-related services
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add bundling and minification services and configuration
            builder.Services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddJavaScriptBundle("/bundles/jquery", "Scripts/jquery-*.js")
                    .UseContentRoot();

                pipeline.AddJavaScriptBundle("/bundles/jqueryval", "Scripts/jquery.validate*")
                    .UseContentRoot();

                pipeline.AddJavaScriptBundle("/bundles/modernizr", "Scripts/modernizr-*")
                    .UseContentRoot();

                pipeline.AddJavaScriptBundle("/bundles/bootstrap", "Scripts/bootstrap.js", "Scripts/respond.js")
                    .UseContentRoot();

                pipeline.AddCssBundle("/Content/css",
                                      "Content/bootstrap.css",
                                      "Content/custom.css",
                                      "Content/base.css",
                                      "Content/site.css")
                    .UseContentRoot();
            });

            var app = builder.Build();

            // Set EF6 DB initializer
            var mockData = app.Configuration.GetValue<bool>("DataSettings:UseMockData");
            if (!mockData)
            {
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                Database.SetInitializer(services.GetRequiredService<CatalogDBInitializer>());
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseWebOptimizer();

            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/fonts",
                FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "fonts"))
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/Images",
                FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Images"))
            });

            app.UseRouting();
            app.UseAuthorization();
            app.UseResponseCaching();

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

            app.MapControllerRoute(name: "Default", pattern: "{controller=Catalog}/{action=Index}/{id?}");

            app.Run();
        }
    }
}