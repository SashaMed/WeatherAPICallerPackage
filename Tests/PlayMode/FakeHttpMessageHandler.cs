using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace WeatherAPICaller.Tests
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _fakeResponse;
        private readonly int _delayMilliseconds;


        public FakeHttpMessageHandler(HttpResponseMessage fakeResponse, int delayMilliseconds = 0)
        {
            _fakeResponse = fakeResponse;
            _delayMilliseconds = delayMilliseconds;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_delayMilliseconds > 0)
            {
                await Task.Delay(_delayMilliseconds, cancellationToken);
            }
            return _fakeResponse;
        }
    }
}