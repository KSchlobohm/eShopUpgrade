using eShopLegacyMVC.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace eShopLegacyMVC.Services
{
    public class WeatherService
    {
        const int DefaultZipCode = 98052;
        const string RequestFormatString = "http://api.weatherapi.com/v1/current.json?key={0}&q={1}&aqi=no";
        private readonly string _apiKey;

        public WeatherService(string apiKey)
        {
            _apiKey = apiKey ?? Environment.GetEnvironmentVariable("WEATHER_API_KEY");
        }

        public int? GetTemperature(int zipCode, bool celsius)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(string.Format(RequestFormatString, _apiKey, zipCode)).Result;
                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content.ReadAsByteArrayAsync().Result;

                    if (data != null && data.Length > 0)
                    {
                        var weatherData = JsonConvert.DeserializeAnonymousType(Encoding.UTF8.GetString(data), new { Current = new { Temp_C = 0, Temp_F = 0 } });
                        if (weatherData != null)
                        {
                            return celsius ? weatherData.Current.Temp_C : weatherData.Current.Temp_F;
                        }
                    }
                }
            }

            return null;
        }
    }
}