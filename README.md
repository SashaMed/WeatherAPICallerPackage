# Weather API Caller Package

## Overview
**Weather API Caller Package** is a Unity package designed to integrate multiple weather APIs (such as OpenWeatherMap and OpenMeteo) into your Unity projects. It provides a unified API for fetching weather data, handling errors, and aggregating responses from various services.

**Developed on Unity 2022.3.25f1**

## Features
- **Unified Weather API:** Retrieve weather data from different sources via a single interface.
- **Extensible Architecture:** Easily add new weather services by implementing the `IWeatherService` interface.
- **Built-in Error Handling:** Automatically handles HTTP errors, timeouts, and cancellation.
- **Samples:** Includes a demo scene (in the Samples~ folder) to test and demonstrate package functionality.
- **Tests:** Unit tests are provided for the public API and integrated weather services.

## Installation

### Via Git URL
Add the following entry to your project's `Packages/manifest.json`:

```json
"dependencies": {
  "com.example.weatherapicaller": "https://github.com/your-username/WeatherAPICallerPackage.git#main"
}
```
Alternatively, open the Unity Package Manager, click the "+" button, select "Add package from git URL..." and paste the URL above.

## Configuration

### API Key Security
For using the OpenWeatherMap API, an API key is required. Do not hardcode API keys in code.

* Specify your API key in the openWeatherMapServiceApiKey field of the WeatherRequester component on the SampleScene, or
* Pass the API key via the constructor of WeatherManager when initializing it directly.

## Samples

The package includes a Samples~ folder containing a test scene (SampleScene) and scripts for verifying package functionality.
To import these samples into your project:

1. Open the Unity Package Manager.
2. Select the Weather API Caller Package.
3. Expand the Samples section and click Import.
4. The samples will be copied into your project's Assets folder.

#### Note:
In the SampleScene, ensure you specify your OpenWeatherMap API key in the openWeatherMapServiceApiKey field of the WeatherRequester component to use the OpenWeatherMap API.

## Tests
Unit tests for the public API and integrated weather services have been added. You can run these tests via Unity Test Runner to verify functionality and stability.
