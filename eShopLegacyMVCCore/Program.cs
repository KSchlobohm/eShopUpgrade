using eShopLegacyMVC.Services;
using Microsoft.Extensions.Options;

const string FileSettingsSectionName = "Files";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();
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

app.UseRouting();
app.UseAuthorization();
app.UseSystemWebAdapters();

app.MapDefaultControllerRoute();
app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.MapControllerRoute("Default", "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
