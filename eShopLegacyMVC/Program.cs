
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Data.Entity;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using eShopLegacyMVC.Services;
using System.Reflection;
using System.Diagnostics;

    namespace eShopLegacyMVC
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add connection strings from Web.config
                builder.Configuration["ConnectionStrings:CatalogDBContext"] = "Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog=Microsoft.eShopOnContainers.Services.CatalogDb; Integrated Security=True; MultipleActiveResultSets=True;";
                builder.Configuration["ConnectionStrings:IdentityDBContext"] = "Data Source=(LocalDb)\\MSSQLLocalDB; Initial Catalog=Microsoft.eShopOnContainers.Services.IdentityDb; Integrated Security=True";

                // Add app settings from Web.config
                builder.Configuration["AppSettings:UseMockData"] = "false";
                builder.Configuration["AppSettings:UseCustomizationData"] = "false";
                builder.Configuration["AppSettings:files:BasePath"] = "\\\\bvtsrv2\\Team\\MikeRou\\eShopFiles";
                builder.Configuration["AppSettings:files:ServiceAccountId"] = "";
                builder.Configuration["AppSettings:files:ServiceAccountDomain"] = "";
                builder.Configuration["AppSettings:files:ServiceAccountPassword"] = "";
                builder.Configuration["AppSettings:weather:ApiKey"] = "";
                builder.Configuration["AppSettings:NewItemQueuePath"] = "FormatName:DIRECT=OS:servername\\private$\\newitems";

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;

                // Add services to the container (formerly ConfigureServices)
                builder.Services.AddControllersWithViews();

                // Configure globalization based on Web.config
                builder.Services.Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US");
                });

                // Register application services
                var useMockData = bool.Parse(builder.Configuration["AppSettings:UseMockData"] ?? "false");
                if (useMockData)
                {
// Register mock services when using mock data
                    builder.Services.AddScoped<ICatalogService, CatalogServiceMock>();
                }
                else
                {
                    // Register real services with database connection
                    builder.Services.AddScoped<ICatalogService, CatalogService>();
                    builder.Services.AddScoped<CatalogDBContext>();
                    builder.Services.AddScoped<CatalogDBInitializer>();
                }

                // Add session support (InProc as specified in Web.config)
                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                // Add Application Insights if present in Web.config
                builder.Services.AddApplicationInsightsTelemetry();

                var app = builder.Build();

                // Configure the HTTP request pipeline (formerly Configure method)
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                // Use request localization
                app.UseRequestLocalization();

                // Use session middleware
                app.UseSession();

                app.Use(async (context, next) =>
                {
                    // Configure activity ID for request tracing (similar to Application_BeginRequest)
                    if (Trace.CorrelationManager.ActivityId == Guid.Empty)
                    {
                        Trace.CorrelationManager.ActivityId = Guid.NewGuid();
                    }

                    // Log request information
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogDebug("WebApplication_BeginRequest: {path}", context.Request.Path);

                    await next();
                });

                app.UseRouting();

                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Initialize database if needed
                if (!useMockData)
                {
using (var scope = app.Services.CreateScope())
                    {
                        var dbInitializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
                        Database.SetInitializer(dbInitializer);
                    }
                }

                app.Run();
            }
        }

        public class ConfigurationManager
        {
            public static IConfiguration Configuration { get; set; }
        }
    }