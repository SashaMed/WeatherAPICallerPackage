using System;

namespace WeatherAPICaller
{

    public abstract class BaseResponse
    {
        public bool IsSuccess { get; set; }

        public int StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public abstract WeatherAPIResponse AsWeatherAPIResponse();
    }
}
