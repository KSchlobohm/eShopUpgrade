using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// Configuration
var useMockData = builder.Configuration.GetValue<bool>("UseMockData");

// EF6 CatalogDBContext - register as scoped (EF6 contexts are not thread-safe)
var catalogConnectionString = builder.Configuration.GetConnectionString("CatalogDBContext");
builder.Services.AddScoped<CatalogDBContext>(sp => new CatalogDBContext(catalogConnectionString));
builder.Services.AddSingleton<CatalogItemHiLoGenerator>();
builder.Services.AddScoped<CatalogDBInitializer>();

// Catalog service
if (useMockData)
{
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}
else
{
    builder.Services.AddScoped<ICatalogService, CatalogService>();
}

// ASP.NET Core Identity with EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDBContext")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

// FileService configuration via DI
builder.Services.AddScoped<FileService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new FileService(new FileServiceConfiguration
    {
        BasePath = config["Files:BasePath"],
        ServiceAccountUsername = config["Files:ServiceAccountUsername"],
        ServiceAccountDomain = config["Files:ServiceAccountDomain"],
        ServiceAccountPassword = config["Files:ServiceAccountPassword"]
    });
});

// Wire up WebHelper user agent accessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure DB (EF6 initializer for non-mock mode)
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
    System.Data.Entity.Database.SetInitializer(initializer);
}

// Middleware to set WebHelper user agent (no session needed here)
app.Use(async (context, next) =>
{
    eShopLegacy.Utilities.WebHelper.SetUserAgentAccessor(() => context.Request.Headers.UserAgent.ToString());
    await next(context);
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Middleware to initialize session values (after UseSession)
app.Use(async (context, next) =>
{
    // Initialize session values (equivalent to Session_Start in Global.asax)
    if (string.IsNullOrEmpty(context.Session.GetString("MachineName")))
    {
        context.Session.SetString("MachineName", Environment.MachineName);
        context.Session.SetString("SessionStartTime", DateTime.Now.ToString());
    }

    await next(context);
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
