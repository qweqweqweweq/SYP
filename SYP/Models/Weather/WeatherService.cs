using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Models.Weather
{
    public static class WeatherService
    {
        private static readonly string apiKey = "3abb5fa043ebc0b6bb6df5299680b452";
        private static readonly string city = "Perm";

        public static async Task<WeatherInfo> GetWeatherAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";
                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherInfo>(response);
            }
        }
    }
}
