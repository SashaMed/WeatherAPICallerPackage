using Newtonsoft.Json;
using System;
using System.Globalization;

namespace WeatherAPICaller
{
    public class OpenMeteoResponse : BaseResponse
    {
        [JsonProperty("latitude")]
        public float Latitude { get; set; }

        [JsonProperty("longitude")]
        public float Longitude { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("hourly")]
        public OpenMeteoHourly Hourly { get; set; }

        public override WeatherAPIResponse AsWeatherAPIResponse()
        {
            if (!IsSuccess)
            {
                return new WeatherAPIResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ErrorMessage,
                    ServiceName = "OpenMeteo",
                    StatusCode = StatusCode,
                };
            }

            var now = DateTime.UtcNow;

            var bestIndex = -1;
            var smallestDiff = TimeSpan.MaxValue;
            var forecastTime = DateTime.MinValue;

            for (int i = 0; i < Hourly.Time.Length; i++)
            {
                if (DateTime.TryParse(Hourly.Time[i], null, DateTimeStyles.RoundtripKind, out DateTime parsedTime))
                {
                    var diff = (parsedTime - now).Duration();
                    if (diff < smallestDiff)
                    {
                        smallestDiff = diff;
                        bestIndex = i;
                        forecastTime = parsedTime;
                    }
                }
            }


            if (bestIndex == -1)
            {
                forecastTime = now;
                bestIndex = 0; 
            }

            var temperature = Hourly.Temperature2m != null && Hourly.Temperature2m.Length > bestIndex
                ? Hourly.Temperature2m[bestIndex]
                : 0;

            var pressure = Hourly.SurfacePressure != null && Hourly.SurfacePressure.Length > bestIndex
                ? Hourly.SurfacePressure[bestIndex]
                : 0;

            var humidity = Hourly.RelativeHumidity2m != null && Hourly.RelativeHumidity2m.Length > bestIndex
                ? Hourly.RelativeHumidity2m[bestIndex]
                : 0;

            var visibility = Hourly.Visibility != null && Hourly.Visibility.Length > bestIndex
                ? Hourly.Visibility[bestIndex]
                : 0;

            return new WeatherAPIResponse
            {
                IsSuccess = true,
                ServiceName = "OpenMeteo",
                DateTime = forecastTime,
                Temperature = temperature,
                Pressure = pressure,
                Humidity = humidity,
                Visibility = visibility,
                WeatherDescription = "N/A" 
            };
        }
    }

    public class OpenMeteoHourly
    {
        [JsonProperty("time")]
        public string[] Time { get; set; }

        [JsonProperty("temperature_2m")]
        public float[] Temperature2m { get; set; }

        [JsonProperty("relative_humidity_2m")]
        public int[] RelativeHumidity2m { get; set; }

        [JsonProperty("surface_pressure")]
        public float[] SurfacePressure { get; set; }

        [JsonProperty("visibility")]
        public float[] Visibility { get; set; }
    }
}
