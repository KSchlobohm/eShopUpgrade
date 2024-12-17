
using eShopLegacyMVC;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Data.Entity;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.AddCatalogServices();
builder.Services.AddResponseCaching();

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

// Static files were not migrated under wwwroot, so serve them from content root subdirectories to minimize changes
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

app.MapControllerRoute("Default", "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
