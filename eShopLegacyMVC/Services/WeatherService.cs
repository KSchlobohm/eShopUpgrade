using eShopLegacyMVC.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Text;

namespace eShopLegacyMVC.Services
{
    public class WeatherService
    {
        const int DefaultZipCode = 98052;
        const string RequestFormatString = "http://api.weatherapi.com/v1/current.json?key={0}&q={1}&aqi=no";
        private readonly string _apiKey;
        private readonly IKeyVaultService _keyVaultService;

        public WeatherService(string apiKey)
        {
            _apiKey = apiKey ?? Environment.GetEnvironmentVariable("WEATHER_API_KEY");
        }

        public WeatherService(IKeyVaultService keyVaultService)
        {
            _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
        }

        public static WeatherService Create()
        {
            // Try to get API key from configuration first (backward compatibility)
            var apiKey = ConfigurationManager.AppSettings["weather:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                return new WeatherService(apiKey);
            }

            // Use Key Vault service
            return new WeatherService(new KeyVaultService());
        }

        public int? GetUserCurrentTemperature(ApplicationUser user, bool celsius)
        {
            var zipCode = user.ZipCode ?? DefaultZipCode;

            return GetTemperature(zipCode, celsius);
        }

        private int? GetTemperature(int zipCode, bool celsius)
        {
            var apiKey = GetApiKey();
            
            using (var client = new WebClient())
            {
                var data = client.DownloadData(string.Format(RequestFormatString, apiKey, zipCode));

                if (data != null && data.Length > 0)
                {
                    var weatherData = JsonConvert.DeserializeAnonymousType(Encoding.UTF8.GetString(data), new { Current = new { Temp_C = 0, Temp_F = 0 } });
                    if (weatherData != null)
                    {
                        return celsius ? weatherData.Current.Temp_C : weatherData.Current.Temp_F;
                    }
                }
            }

            return null;
        }

        private string GetApiKey()
        {
            // If API key was provided directly via constructor
            if (!string.IsNullOrEmpty(_apiKey))
            {
                return _apiKey;
            }

            // If Key Vault service is available, get from Key Vault
            if (_keyVaultService != null)
            {
                try
                {
                    return _keyVaultService.GetSecret("weather-api-key");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to retrieve weather API key from Key Vault", ex);
                }
            }

            throw new InvalidOperationException("Weather API key is not configured. Set weather:ApiKey in appSettings or configure Key Vault.");
        }
    }
}