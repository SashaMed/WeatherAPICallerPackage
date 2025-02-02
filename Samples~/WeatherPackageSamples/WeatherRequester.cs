using System;
using System.Threading;
using TMPro;
using UnityEngine;
using WeatherAPICaller;

public class WeatherRequester : MonoBehaviour
{
    private WeatherManager weatherManager;

    public TMP_InputField latitudeText;
    public TMP_InputField longitudeText;
    public TMP_InputField timeoutText;


    [Space]
    [SerializeField] private string openWeatherMapServiceApiKey;

    //12339c213446dfcc099f07c574e2b96e

    private CancellationTokenSource cancellationTokenSource;
    private float defaultTimeout = 10f;

    private void Awake()
    {
        weatherManager = new WeatherManager(openWeatherMapServiceApiKey);
    }

    public void ActivateToken()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
        cancellationTokenSource = new CancellationTokenSource();
    }

    public async void RequestWeather()
    {
        ActivateToken();

        float timeout;
        double latitude, longitude;
        if (!double.TryParse(latitudeText.text, out latitude))
        {
            Debug.LogError("Invalid latitude value. Please enter a valid number.");
            return;
        }
        if (!double.TryParse(longitudeText.text, out longitude))
        {
            Debug.LogError("Invalid longitude value. Please enter a valid number.");
            return;
        }

        if (!float.TryParse(timeoutText.text, out timeout))
        {
            Debug.LogError("Invalid timeout value. Please enter a valid number.");
            return;
        }

        if (latitude < -90 || latitude > 90)
        {
            Debug.LogError("Latitude must be between -90 and 90 degrees.");
            return;
        }
        if (longitude < -180 || longitude > 180)
        {
            Debug.LogError("Longitude must be between -180 and 180 degrees.");
            return;
        }

        if (timeout < 0)
        {
            Debug.LogError("timeout must be greater then 0, switch to default.");
            timeout = defaultTimeout;
        }


        try
        {
            var weather = await weatherManager.GetWeather(latitude, longitude, timeout, cancellationTokenSource.Token);

            foreach (var result in weather.Results)
            {
                if (result.IsSuccess)
                {
                    Debug.Log($"Service: {result.ServiceName}, Temperature: {result.Temperature}°C, Description: {result.WeatherDescription}, Pressure: {result.Pressure}, Humidity: {result.Humidity}.");
                }
                else
                {
                    Debug.LogError($"Service: {result.ServiceName}, error during call to service, error message: {result.ErrorMessage}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error fetching weather: " + ex.Message);
        }
    }
}
