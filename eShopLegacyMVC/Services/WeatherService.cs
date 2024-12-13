using eShopLegacyMVC.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;

namespace eShopLegacyMVC.Services
{
    public class WeatherService
    {
        const int DefaultZipCode = 98052;
        const string RequestFormatString = "http://api.weatherapi.com/v1/current.json?key={0}&q={1}&aqi=no";
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public WeatherService(string apiKey, HttpClient httpClient)
        {
            _apiKey = apiKey ?? Environment.GetEnvironmentVariable("WEATHER_API_KEY");
            _httpClient = httpClient;
        }

        public async Task<int?> GetUserCurrentTemperature(ApplicationUser user, bool celsius)
        {
            var zipCode = (await user.GetZipCode(_httpClient)) ?? DefaultZipCode;

            return await GetTemperature(zipCode, celsius);
        }

        private async Task<int?> GetTemperature(int zipCode, bool celsius)
        {
            var response = await _httpClient.GetStringAsync(string.Format(RequestFormatString, _apiKey, zipCode));

            if (!string.IsNullOrEmpty(response))
            {
                var weatherData = JsonConvert.DeserializeAnonymousType(response, new { Current = new { Temp_C = 0, Temp_F = 0 } });
                if (weatherData != null)
                {
                    return celsius ? weatherData.Current.Temp_C : weatherData.Current.Temp_F;
                }
            }

            return null;
        }
    }
}