using Microsoft.AspNetCore.Mvc;
using Tempo.Api.Clients;
using Tempo.Api.Models;

namespace Tempo.Api.Controllers;

[ApiController]
[Route("weather/v1")]
public class WeatherController(IGeocodingClient geocodingClient, IWeatherClient weatherClient) : ControllerBase
{
    /// <summary>
    /// Gets the current weather for a given city.
    /// </summary>
    /// <param name="city">City name (e.g. "Auckland", "London").</param>
    /// <param name="cancellationToken"/>
    /// <returns>The current temperature in Celsius for the requested city.</returns>
    /// <response code="200">Returns the current weather.</response>
    /// <response code="404">City not found or forecast unavailable.</response>
    [HttpGet("{city}")]
    [ProducesResponseType<WeatherResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherResult>> Get(string city, CancellationToken cancellationToken)
    {
        var coordinates = await geocodingClient.GeocodeAsync(city, cancellationToken);
        if (coordinates is null)
            return Problem(
                detail: $"Could not find a location matching '{city}'.",
                statusCode: StatusCodes.Status404NotFound,
                title: "City not found");

        var (lat, lon) = coordinates.Value;

        var temperatureC = await weatherClient.GetForecastAsync(lat, lon, cancellationToken);
        if (temperatureC is null)
            return Problem(
                detail: $"Forecast data is currently unavailable for '{city}'.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Forecast unavailable");

        return new WeatherResult
        {
            City = city,
            TemperatureC = temperatureC.Value
        };
    }
}
