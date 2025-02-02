using System.Threading;
using System.Threading.Tasks;

namespace WeatherAPICaller.Services
{
    public class OpenMeteoService : BaseService<OpenMeteoResponse>
    {
        protected override string BuildRequestUrl(double lat, double lon) => 
            $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&hourly=temperature_2m,relative_humidity_2m,apparent_temperature,surface_pressure,visibility&forecast_days=1";

    }
}
