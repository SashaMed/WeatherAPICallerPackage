using System.Collections.Generic;

namespace WeatherAPICaller
{
    public class Weather
    {
        public List<WeatherAPIResponse> Results { get; private set; }

        public Weather()
        {
            Results = new List<WeatherAPIResponse>();
        }
    }
}
