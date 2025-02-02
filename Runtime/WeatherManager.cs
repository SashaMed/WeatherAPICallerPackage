using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WeatherAPICaller.Services;

namespace WeatherAPICaller
{
    public class WeatherManager
    {
        private readonly List<IWeatherService> services = new List<IWeatherService>();

        private readonly object _lock = new object();

        public WeatherManager(string openWeatherMapServiceApiKey = null, bool initByDefaults = true)
        {
            if (initByDefaults)
            {
                AddService(new OpenMeteoService());
                if (!string.IsNullOrEmpty(openWeatherMapServiceApiKey))
                {
                    AddService(new OpenWeatherMapService(openWeatherMapServiceApiKey));
                }
                else
                {
                    Debug.LogWarning("OpenWeatherMapServiceApiKey is null or empty, service is skipped");
                }
            }
        }

        public void AddService(IWeatherService service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            lock (_lock)
            {
                services.Add(service);
            }
        }


        public async Task<Weather> GetWeather(double lat, double lon, float timeout, CancellationToken ctoken)
        {
            var weather = new Weather();
            List<IWeatherService> servicesSnapshot;

            lock (_lock)
            {
                servicesSnapshot = new List<IWeatherService>(services);
            }

            var tasks = new List<Task<WeatherAPIResponse>>();
            foreach (var service in servicesSnapshot)
            {
                tasks.Add(service.GetWeatherAsync(lat, lon, timeout, ctoken));
            }

            try
            {
                var results = await Task.WhenAll(tasks);
                weather.Results.AddRange(results);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error fetching weather data: {ex.Message}");
            }

            return weather;
        }
    }
}