using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;
using Microsoft.Extensions.Options;
using System.Runtime.Versioning;

namespace eShopLegacyMVC
{
    [SupportedOSPlatform("windows")]
    public static class Extensions
    {
        public const string DataSettingsSectionName = "DataSettings";
        public const string FileSettingsSectionName = "FileSettings";

        public static void AddCatalogServices(this IHostApplicationBuilder builder)
        {
            builder.Services.Configure<DataSettings>(builder.Configuration.GetSection(DataSettingsSectionName));
            builder.Services.AddScoped<CatalogService>();
            builder.Services.AddScoped<CatalogServiceMock>();
            builder.Services.AddScoped<ICatalogService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<DataSettings>>().Value;
                return settings.UseMockData
                    ? sp.GetRequiredService<CatalogServiceMock>()
                    : sp.GetRequiredService<CatalogService>();
            });
            builder.Services.AddScoped(sp => new CatalogDBContext(
                sp.GetRequiredService<IConfiguration>().GetConnectionString("CatalogConnection")
                ?? string.Empty));
            builder.Services.AddScoped<CatalogDBInitializer>();
            builder.Services.AddSingleton<CatalogItemHiLoGenerator>();
        }

        public static void AddFileServices(this IHostApplicationBuilder builder)
        {
            builder.Services.Configure<FileSettings>(builder.Configuration.GetSection(FileSettingsSectionName));
            builder.Services.AddSingleton<FileService>();
        }
    }
}