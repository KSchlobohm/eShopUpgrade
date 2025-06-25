using eShopLegacy.Models;
using eShopLegacyMVC.Services;
using Microsoft.Extensions.Options;

const string FileSettingsSectionName = "Files";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters()
    .AddJsonSessionSerializer(options =>
    {
        options.RegisterKey<string>("MachineName");
        options.RegisterKey<DateTime>("SessionStartTime");
        options.RegisterKey<SessionDemoModel>("DemoItem");
    })
    .AddRemoteAppClient(options =>
    {
        options.RemoteAppUrl = new(builder.Configuration["ProxyTo"]);
        options.ApiKey = builder.Configuration["RemoteAppApiKey"];
    })
    .AddSessionClient()
    .AddAuthenticationClient(true);

builder.Services.AddSession();
builder.Services.AddResponseCaching();
builder.Services.AddHttpForwarder();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<FileServiceConfiguration>(builder.Configuration.GetSection(FileSettingsSectionName));
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IOptions<FileServiceConfiguration>>().Value;
    return new FileService(config);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCaching();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSystemWebAdapters();

app.MapControllerRoute("Default", "{controller=Catalog}/{action=Index}/{id?}")
    .RequireSystemWebAdapterSession();

app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.Run();
