using System.Threading;
using System.Threading.Tasks;

namespace WeatherAPICaller.Services
{
    public class OpenWeatherMapService : BaseService<OpenWeatherCurrentResponse>
    {

        private string appid;
        public OpenWeatherMapService(string apiKey) 
        {
            appid = apiKey;
        }

        protected override string BuildRequestUrl(double lat, double lon) => 
            $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&exclude=minutely,alerts,daily,hourly&units=metric&appid={appid}";

    }
}
