using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using Assert = NUnit.Framework.Assert;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;

namespace WeatherAPICaller.Tests
{

    [TestFixture]
    public class WeatherManagerTests
    {

        private WeatherManager weatherManager;

        [SetUp]
        public void Setup()
        {
            weatherManager = new WeatherManager(initByDefaults: false);
        }

        [Test]
        public void AddService_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => weatherManager.AddService(null));
        }


        [UnityTest]
        public IEnumerator GetWeather_HappyCase_ReturnsAggregatedResults()
        {
            var mockService1 = new Mock<IWeatherService>();
            var mockService2 = new Mock<IWeatherService>();

            var response1 = new WeatherAPIResponse
            {
                IsSuccess = true,
                ServiceName = "MockService1",
                DateTime = DateTime.UtcNow,
                Temperature = 22.5f,
                Pressure = 1012,
                Humidity = 60,
                Visibility = 10000,
                WeatherDescription = "Clear sky"
            };

            var response2 = new WeatherAPIResponse
            {
                IsSuccess = true,
                ServiceName = "MockService2",
                DateTime = DateTime.UtcNow,
                Temperature = 21.0f,
                Pressure = 1010,
                Humidity = 55,
                Visibility = 9000,
                WeatherDescription = "Partly cloudy"
            };

            mockService1
                .Setup(s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response1);
            mockService2
                .Setup(s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response2);

            weatherManager.AddService(mockService1.Object);
            weatherManager.AddService(mockService2.Object);

            var task = weatherManager.GetWeather(55.0, 37.0, 10f, CancellationToken.None);

            yield return new WaitUntil(() => task.IsCompleted);

            // Assert
            var weather = task.Result;
            Assert.IsNotNull(weather);
            Assert.AreEqual(2, weather.Results.Count);
            Assert.IsTrue(weather.Results.Exists(r => r.ServiceName == "MockService1" && r.Temperature == 22.5f));
            Assert.IsTrue(weather.Results.Exists(r => r.ServiceName == "MockService2" && r.Temperature == 21.0f));
        }

        [UnityTest]
        public IEnumerator GetWeather_CancellationOrTimeout_ReturnsUnsuccessfulResults()
        {
            var mockService = new Mock<IWeatherService>();
            mockService
                .Setup(s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .Returns(async (double lat, double lon, float timeout, CancellationToken token) =>
                {
                    var msg = "";
                    try
                    {
                        await Task.Delay(1000, token);
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message;
                    }
                    return new WeatherAPIResponse
                    {
                        IsSuccess = false,
                        ServiceName = "MockService",
                        ErrorMessage = msg
                    };
                });

            weatherManager.AddService(mockService.Object);


            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(100);


            var task = weatherManager.GetWeather(50.0, 30.0, 5f, cts.Token);

            yield return new WaitUntil(() => task.IsCompleted);

            var weather = task.Result;
            Assert.IsNotNull(weather);
            Assert.AreEqual(1, weather.Results.Count);
            Assert.IsTrue(weather.Results.Exists(r => !r.IsSuccess && r.ErrorMessage == new TaskCanceledException().Message));
        }


        [UnityTest]
        public IEnumerator GetWeather_OneRequestCanceledByToken_ReturnsAggregatedResults()
        {
            var mockService1 = new Mock<IWeatherService>();
            var mockService2 = new Mock<IWeatherService>();

            var response1 = new WeatherAPIResponse
            {
                IsSuccess = true,
                ServiceName = "MockService1",
                DateTime = DateTime.UtcNow,
                Temperature = 22.5f,
                Pressure = 1012,
                Humidity = 60,
                Visibility = 10000,
                WeatherDescription = "Clear sky"
            };

            var response2 = new WeatherAPIResponse
            {
                IsSuccess = true,
                ServiceName = "MockService2",
                DateTime = DateTime.UtcNow,
                Temperature = 21.0f,
                Pressure = 1010,
                Humidity = 55,
                Visibility = 9000,
                WeatherDescription = "Partly cloudy"
            };

            mockService1
                .Setup(s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response1);
            mockService2
                .Setup(s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .Returns(async (double lat, double lon, float timeout, CancellationToken token) =>
                {
                    var msg = "";
                    try
                    {
                        await Task.Delay(1000, token);
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message;
                    }
                    return new WeatherAPIResponse
                    {
                        IsSuccess = false,
                        ServiceName = "MockService2",
                        ErrorMessage = msg
                    };
                });

            weatherManager.AddService(mockService1.Object);
            weatherManager.AddService(mockService2.Object);

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(100);
            var task = weatherManager.GetWeather(55.0, 37.0, 10f, cts.Token);

            yield return new WaitUntil(() => task.IsCompleted);

            // Assert

            var weather = task.Result;
            Assert.IsNotNull(weather);
            Assert.AreEqual(2, weather.Results.Count);
            Assert.IsTrue(weather.Results.Exists(r => r.ServiceName == "MockService1" && r.Temperature == 22.5f));
            Assert.IsTrue(weather.Results.Exists(r => r.ServiceName == "MockService2" && !r.IsSuccess && r.ErrorMessage == new TaskCanceledException().Message));
        }


        [UnityTest]
        public IEnumerator GetWeather_NoServicesAdded_ReturnsEmptyResults()
        {
            var task = weatherManager.GetWeather(55.0, 37.0, 10f, CancellationToken.None);

            yield return new WaitUntil(() => task.IsCompleted);

            var weather = task.Result;
            Assert.IsNotNull(weather);
            Assert.IsEmpty(weather.Results);
        }


        [UnityTest]
        public IEnumerator GetWeather_PartialFailure_ReturnsMixedResults()
        {
            var mockServiceSuccess = new Mock<IWeatherService>();
            var mockServiceFailure = new Mock<IWeatherService>();

            var successResponse = new WeatherAPIResponse
            {
                IsSuccess = true,
                ServiceName = "MockSuccess",
                DateTime = DateTime.UtcNow,
                Temperature = 23f,
                Pressure = 1013,
                Humidity = 50,
                Visibility = 10000,
                WeatherDescription = "Sunny"
            };

            var failureResponse = new WeatherAPIResponse
            {
                IsSuccess = false,
                ServiceName = "MockFailure",
                ErrorMessage = "Some error occurred"
            };

            mockServiceSuccess
                .Setup(s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successResponse);
            mockServiceFailure
                .Setup(s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failureResponse);

            weatherManager.AddService(mockServiceSuccess.Object);
            weatherManager.AddService(mockServiceFailure.Object);

            var task = weatherManager.GetWeather(55.0, 37.0, 10f, CancellationToken.None);
            yield return new WaitUntil(() => task.IsCompleted);

            var weather = task.Result;
            Assert.IsNotNull(weather);
            Assert.AreEqual(2, weather.Results.Count);

            var successResult = weather.Results.Find(r => r.ServiceName == "MockSuccess");
            var failureResult = weather.Results.Find(r => r.ServiceName == "MockFailure");

            Assert.IsNotNull(successResult);
            Assert.IsTrue(successResult.IsSuccess);
            Assert.AreEqual(23f, successResult.Temperature);

            Assert.IsNotNull(failureResult);
            Assert.IsFalse(failureResult.IsSuccess);
            Assert.AreEqual("Some error occurred", failureResult.ErrorMessage);
        }
    }
}