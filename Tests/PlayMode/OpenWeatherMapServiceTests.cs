using NUnit.Framework;
using System.Net.Http;
using System.Net;
using System.Threading;
using UnityEngine.TestTools;
using UnityEngine;
using WeatherAPICaller.Services;
using System.Collections;

namespace WeatherAPICaller.Tests
{

    [TestFixture]
    public class OpenWeatherMapServiceTests
    {

        private string openMeteoTestJson = @"
        {
          ""lat"": 41.6434,
          ""lon"": 41.6399,
          ""timezone"": ""Asia/Tbilisi"",
          ""timezone_offset"": 14400,
          ""current"": {
            ""dt"": 1738492066,
            ""sunrise"": 1738470274,
            ""sunset"": 1738506579,
            ""temp"": 11.99,
            ""feels_like"": 10.76,
            ""pressure"": 1026,
            ""humidity"": 58,
            ""dew_point"": 3.99,
            ""uvi"": 1.95,
            ""clouds"": 0,
            ""visibility"": 10000,
            ""wind_speed"": 2.57,
            ""wind_deg"": 260,
            ""weather"": [
              {
                ""id"": 800,
                ""main"": ""Clear"",
                ""description"": ""clear sky"",
                ""icon"": ""01d""
              }
            ]
          }
        }";

        private string dummyApiKey = "dummy_key";

        [UnityTest]
        public IEnumerator OpenWeatherMapService_HappyCase_ReturnsCorrectResponse()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(openMeteoTestJson)
            };
            var fakeHandler = new FakeHttpMessageHandler(fakeResponse);
            var httpClient = new HttpClient(fakeHandler);
            var testService = new TestOpenWeatherMapService(httpClient, dummyApiKey);


            var task = testService.GetWeatherAsync(41.625, 41.625, 5f, CancellationToken.None);

            yield return new WaitUntil(() => task.IsCompleted);

            var result = task.Result;
            Debug.Log(result);
            Assert.IsTrue(result.IsSuccess,"Should be true");
            Assert.AreEqual("OpenWeatherMap", result.ServiceName);
            Assert.AreEqual(11.99f, result.Temperature);
            Assert.AreEqual(1026, result.Pressure);
            Assert.AreEqual(58, result.Humidity);
            Assert.AreEqual(10000, result.Visibility);
        }

        [UnityTest]
        public IEnumerator OpenWeatherMapService_BrokenJson_ReturnsCorrectResponse()
        {
            string brokenJson = @"
        {
            ""latitude"": 41.625,
            ""longitude"": 41.625,
            ""timezone"": ""GMT""
            ""hourly"": {
                ""time"": [ ""2025-02-01T00:00:00Z"" ],
                ""temperature_2m"": [ 5.4 ],
                ""relative_humidity_2m"": [ 70 ],
                ""surface_pressure"": [ 1023.7 ],
                ""visibility"": [ 55100 ]
            }
        }";

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(brokenJson)
            };
            var fakeHandler = new FakeHttpMessageHandler(fakeResponse);
            var httpClient = new HttpClient(fakeHandler);
            var testService = new TestOpenWeatherMapService(httpClient, dummyApiKey);


            var task = testService.GetWeatherAsync(41.625, 41.625, 5f, CancellationToken.None);

            yield return new WaitUntil(() => task.IsCompleted);

            var result = task.Result;
            Assert.IsTrue(!result.IsSuccess);
            Assert.AreEqual(500, result.StatusCode);
        }

        [UnityTest]
        public IEnumerator OpenWeatherMapService_ErrorResponse_ReturnsErrorResponse()
        {
            var errorContent = "Bad Request";
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorContent)
            };
            var fakeHandler = new FakeHttpMessageHandler(fakeResponse);
            var httpClient = new HttpClient(fakeHandler);

            var testService = new TestOpenWeatherMapService(httpClient, dummyApiKey);

            var task = testService.GetWeatherAsync(41.625, 41.625, 5f, CancellationToken.None);
            yield return new WaitUntil(() => task.IsCompleted);

            WeatherAPIResponse result = task.Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(errorContent, result.ErrorMessage);
        }

        [Test]
        public void OpenWeatherMapService_BuildRequestUrl_ReturnsCorrectUrl()
        {
            var service = new TestOpenWeatherMapService(new HttpClient(new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK))), dummyApiKey);
            var url = service.ExposeBuildRequestUrl(41.625, 41.625);
            var expectedUrl = $"https://api.openweathermap.org/data/3.0/onecall?lat=41.625&lon=41.625&exclude=minutely,alerts,daily,hourly&units=metric&appid={dummyApiKey}";
            Assert.AreEqual(expectedUrl, url);
        }

        [UnityTest]
        public IEnumerator OpenWeatherMapService_Timeout_ReturnsTimeoutError()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(openMeteoTestJson)
            };
            var fakeHandler = new FakeHttpMessageHandler(fakeResponse, delayMilliseconds: 3000);
            var httpClient = new HttpClient(fakeHandler);
            var testService = new TestOpenWeatherMapService(httpClient, dummyApiKey);


            var task = testService.GetWeatherAsync(41.625, 41.625, 1f, CancellationToken.None);
            yield return new WaitUntil(() => task.IsCompleted);

            WeatherAPIResponse result = task.Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(408, result.StatusCode);
        }


        [UnityTest]
        public IEnumerator OpenWeatherMapService_CancellationToken_ReturnsTimeoutError()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(openMeteoTestJson)
            };
            var fakeHandler = new FakeHttpMessageHandler(fakeResponse, delayMilliseconds: 3000);
            var httpClient = new HttpClient(fakeHandler);
            var testService = new TestOpenWeatherMapService(httpClient, dummyApiKey);

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(100);

            var task = testService.GetWeatherAsync(41.625, 41.625, 1f, cts.Token);
            yield return new WaitUntil(() => task.IsCompleted);

            WeatherAPIResponse result = task.Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(408, result.StatusCode);
        }

    }

    public class TestOpenWeatherMapService : OpenWeatherMapService
    {
        private readonly HttpClient _httpClient;

        public TestOpenWeatherMapService(HttpClient httpClient, string apiKkey) : base(apiKkey) 
        {
            _httpClient = httpClient;
        }

        protected override HttpClient CreateHttpClient(float timeout)
        {
            _httpClient.Timeout = System.TimeSpan.FromSeconds(timeout);
            return _httpClient;
        }

        public string ExposeBuildRequestUrl(double lat, double lon)
        {
            return BuildRequestUrl(lat, lon);
        }
    }

}