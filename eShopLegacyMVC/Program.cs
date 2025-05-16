
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using eShopLegacyMVC.Services;
using System.Data.Entity;
using System.Diagnostics;

    namespace eShopLegacyMVC
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add connection strings from Web.config
                // Register CatalogDBContext without passing connection string directly
                builder.Services.AddScoped<CatalogDBContext>();

                // Store connection strings for Entity Framework to use
                builder.Configuration.GetSection("ConnectionStrings").GetChildren().ToList().ForEach(connectionString =>
                {
                    string name = connectionString.Key;
                    string value = connectionString.Value;
                    System.Configuration.ConfigurationManager.ConnectionStrings.Add(
                        new System.Configuration.ConnectionStringSettings(name, value));
                });

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;

                // Add services to the container (formerly ConfigureServices)
                builder.Services.AddControllersWithViews();

                // Initialize filter providers
                builder.Services.AddSingleton<ILoggerFactory>(services => LoggerFactory.Create(builder => builder.AddConsole().AddDebug()));

                // Register modules based on configuration
                bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");
                if (useMockData)
                {
                    // Temporarily commenting out until CatalogServiceMock is available
                    builder.Services.AddScoped<ICatalogService, CatalogService>();
                    Console.WriteLine("Warning: Using CatalogService instead of CatalogServiceMock because the mock service class couldn't be found.");
                }
                else
                {
                    builder.Services.AddScoped<ICatalogService, CatalogService>();
                    builder.Services.AddScoped<CatalogDBInitializer>();
                    builder.Services.AddScoped<CatalogDBContext>();
                }

                // Add session support
                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                
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

                // Set globalization from web.config
                var supportedCultures = new[] { "en-US" };
                var localizationOptions = new RequestLocalizationOptions()
                    .SetDefaultCulture(supportedCultures[0])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);

                app.UseRequestLocalization(localizationOptions);
                
                app.UseHttpsRedirection();
                app.UseStaticFiles();
                
                app.UseSession();

                app.Use(async (context, next) =>
                {
                    // Add tracking info similar to Session_Start
                    context.Session.SetString("MachineName", Environment.MachineName);
                    context.Session.SetString("SessionStartTime", DateTime.Now.ToString());

                    // Setup Activity ID for logging
                    if (Trace.CorrelationManager.ActivityId == Guid.Empty)
                    {
                        Trace.CorrelationManager.ActivityId = Guid.NewGuid();
                    }

                    // Add configuration for files path from Web.config
                    string filesBasePath = app.Configuration.GetValue<string>("files:BasePath");
                    if (!string.IsNullOrEmpty(filesBasePath))
                    {
                        context.Items["FilesBasePath"] = filesBasePath;
                    }

                    // Configure weather API key from Web.config
                    string weatherApiKey = app.Configuration.GetValue<string>("weather:ApiKey");
                    if (!string.IsNullOrEmpty(weatherApiKey))
                    {
                        context.Items["WeatherApiKey"] = weatherApiKey;
                    }

                    // Configure queue path from Web.config
                    string newItemQueuePath = app.Configuration.GetValue<string>("NewItemQueuePath");
                    if (!string.IsNullOrEmpty(newItemQueuePath))
                    {
                        context.Items["NewItemQueuePath"] = newItemQueuePath;
                    }

                    await next();
                });

                app.UseRouting();

                app.UseAuthorization();
                
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Configure database
                if (!app.Configuration.GetValue<bool>("UseMockData"))
                {
using (var scope = app.Services.CreateScope())
                    {
                        var dbInitializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
                        Database.SetInitializer(dbInitializer);

                        // Ensure database is created and seeded
                        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
                        dbContext.Database.Initialize(force: true);
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