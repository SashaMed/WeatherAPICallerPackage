using System.Threading;
using System.Threading.Tasks;

namespace WeatherAPICaller
{
    public interface IWeatherService
    {
        Task<WeatherAPIResponse> GetWeatherAsync(double lat, double lon, float timeout, CancellationToken token);
    }
}