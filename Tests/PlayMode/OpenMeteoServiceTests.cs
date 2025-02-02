using System.Net;
using System.Net.Http;
using NUnit.Framework;
using System.Threading;
using WeatherAPICaller.Services;
using UnityEngine.TestTools;
using UnityEngine;
using System.Collections;


namespace WeatherAPICaller.Tests
{

    [TestFixture]
    public class OpenMeteoServiceTests
    {

        private string openMeteoTestJson = @"
    {
        ""latitude"": 41.625,
        ""longitude"": 41.625,
        ""timezone"": ""GMT"",
        ""hourly"": {
            ""time"": [ ""2025-02-01T00:00:00Z"" ],
            ""temperature_2m"": [ 5.4 ],
            ""relative_humidity_2m"": [ 70 ],
            ""surface_pressure"": [ 1023.7 ],
            ""visibility"": [ 55100 ]
        }
    }";


        [UnityTest]
        public IEnumerator OpenMeteoService_HappyCase_ReturnsCorrectResponse()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(openMeteoTestJson)
            };
            var fakeHandler = new FakeHttpMessageHandler(fakeResponse);
            var httpClient = new HttpClient(fakeHandler);
            var testService = new TestOpenMeteoService(httpClient);


            var task = testService.GetWeatherAsync(41.625, 41.625, 5f, CancellationToken.None);

            yield return new WaitUntil(() => task.IsCompleted);

            var result = task.Result;
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("OpenMeteo", result.ServiceName);
            Assert.AreEqual(5.4f, result.Temperature);
            Assert.AreEqual(1023.7f, result.Pressure);
            Assert.AreEqual(70, result.Humidity);
            Assert.AreEqual(55100f, result.Visibility);
        }

        [UnityTest]
        public IEnumerator OpenMeteoService_BrokenJson_ReturnsCorrectResponse()
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
            var testService = new TestOpenMeteoService(httpClient);


            var task = testService.GetWeatherAsync(41.625, 41.625, 5f, CancellationToken.None);

            yield return new WaitUntil(() => task.IsCompleted);

            var result = task.Result;
            Assert.IsTrue(!result.IsSuccess);
            Assert.AreEqual(500, result.StatusCode);
        }

        [UnityTest]
        public IEnumerator OpenMeteoService_ErrorResponse_ReturnsErrorResponse()
        {
            var errorContent = "Bad Request";
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorContent)
            };
            var fakeHandler = new FakeHttpMessageHandler(fakeResponse);
            var httpClient = new HttpClient(fakeHandler);

            var testService = new TestOpenMeteoService(httpClient);

            var task = testService.GetWeatherAsync(41.625, 41.625, 5f, CancellationToken.None);
            yield return new WaitUntil(() => task.IsCompleted);

            WeatherAPIResponse result = task.Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(errorContent, result.ErrorMessage);
        }

        [Test]
        public void OpenMeteoService_BuildRequestUrl_ReturnsCorrectUrl()
        {
            var service = new TestOpenMeteoService(new HttpClient(new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK))));
            var url = service.ExposeBuildRequestUrl(41.625, 41.625);
            var expectedUrl = $"https://api.open-meteo.com/v1/forecast?latitude=41.625&longitude=41.625&hourly=temperature_2m,relative_humidity_2m,apparent_temperature,surface_pressure,visibility&forecast_days=1";
            Assert.AreEqual(expectedUrl, url);
        }

        [UnityTest]
        public IEnumerator OpenMeteoService_Timeout_ReturnsTimeoutError()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(openMeteoTestJson)
            };
            var fakeHandler = new FakeHttpMessageHandler(fakeResponse, delayMilliseconds: 3000);
            var httpClient = new HttpClient(fakeHandler);
            var testService = new TestOpenMeteoService(httpClient);


            var task = testService.GetWeatherAsync(41.625, 41.625, 1f, CancellationToken.None);
            yield return new WaitUntil(() => task.IsCompleted);

            WeatherAPIResponse result = task.Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(408, result.StatusCode);
        }


        [UnityTest]
        public IEnumerator OpenMeteoService_CancellationToken_ReturnsTimeoutError()
        {
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(openMeteoTestJson)
            };
            var fakeHandler = new FakeHttpMessageHandler(fakeResponse, delayMilliseconds: 3000);
            var httpClient = new HttpClient(fakeHandler);
            var testService = new TestOpenMeteoService(httpClient);

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(100);

            var task = testService.GetWeatherAsync(41.625, 41.625, 1f, cts.Token);
            yield return new WaitUntil(() => task.IsCompleted);

            WeatherAPIResponse result = task.Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(408, result.StatusCode);
        }

    }

    public class TestOpenMeteoService : OpenMeteoService
    {
        private readonly HttpClient _httpClient;

        public TestOpenMeteoService(HttpClient httpClient)
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