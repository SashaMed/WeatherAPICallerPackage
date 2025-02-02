using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherAPICaller.Services
{
    public abstract class BaseService<T> : IWeatherService where T : BaseResponse, new()
    {
        protected abstract string BuildRequestUrl(double lat, double lon);


        protected virtual HttpClient CreateHttpClient(float timeout)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeout);
            return client;
        }


        public async Task<WeatherAPIResponse> GetWeatherAsync(double lat, double lon, float timeout, CancellationToken token)
        {
            using (HttpClient client = CreateHttpClient(timeout))
            {
                try
                {
                    var url = BuildRequestUrl(lat, lon);
                    HttpResponseMessage response = await client.GetAsync(url, token);
                    T result;
                    string content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        result = JsonConvert.DeserializeObject<T>(content);
                        result.IsSuccess = true;
                        result.StatusCode = (int)response.StatusCode;
                        return result.AsWeatherAPIResponse();
                    }
                    return CreateUnsuccessfulResponse((int)response.StatusCode, content).AsWeatherAPIResponse();
                }
                catch (TaskCanceledException ex)
                {
                    //Debug.LogError($"Request was canceled: {ex.Message}");
                    return CreateUnsuccessfulResponse(408, ex.Message).AsWeatherAPIResponse();
                }
                catch (Exception ex)
                {
                    //Debug.LogError($"Error in BaseService: {ex.Message}");
                    return CreateUnsuccessfulResponse(500, ex.Message).AsWeatherAPIResponse();
                }
            }
        }


        private T CreateUnsuccessfulResponse(int code, string message)
        {
            return new T
            {
                IsSuccess = false,
                StatusCode = code,
                ErrorMessage = message
            };
        }
    }
}
