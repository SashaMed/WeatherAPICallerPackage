using System;

namespace WeatherAPICaller
{
    public class WeatherAPIResponse
    {
        public string ServiceName { get; set; }

        public DateTime DateTime { get; set; }

        public float Temperature { get; set; }

        public float Pressure { get; set; }

        public int Humidity { get; set; }

        public float Visibility { get; set; }

        public string WeatherDescription { get; set; }

        public bool IsSuccess { get; set; }

        public int StatusCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}
