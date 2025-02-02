using Newtonsoft.Json;
using System;

namespace WeatherAPICaller
{
    public class OpenWeatherCurrentResponse : BaseResponse
    {
        [JsonProperty("lat")]
        public float Latitude { get; set; }

        [JsonProperty("lon")]
        public float Longitude { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("timezone_offset")]
        public int TimezoneOffset { get; set; }

        [JsonProperty("current")]
        public OpenWeatherCurrent Current { get; set; }

        public override WeatherAPIResponse AsWeatherAPIResponse()
        {
            if (!IsSuccess)
            {
                return new WeatherAPIResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ErrorMessage,
                    ServiceName = "OpenWeatherMap",
                    StatusCode = StatusCode,
                };
            }

            DateTime dt = DateTimeOffset.FromUnixTimeSeconds(Current.Dt).UtcDateTime;

            return new WeatherAPIResponse
            {
                IsSuccess = true,
                ServiceName = "OpenWeatherMap",
                DateTime = dt,
                Temperature = Current.Temp,
                Pressure = Current.Pressure,
                Humidity = Current.Humidity,
                Visibility = Current.Visibility,
                WeatherDescription = (Current.Weather != null && Current.Weather.Length > 0)
                    ? Current.Weather[0].Description
                    : "N/A"
            };
        }
    }

    public class OpenWeatherCurrent
    {
        [JsonProperty("dt")]
        public long Dt { get; set; }

        [JsonProperty("temp")]
        public float Temp { get; set; }

        [JsonProperty("pressure")]
        public float Pressure { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        [JsonProperty("visibility")]
        public float Visibility { get; set; }

        [JsonProperty("weather")]
        public OpenWeatherWeather[] Weather { get; set; }
    }

    public class OpenWeatherWeather
    {
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}