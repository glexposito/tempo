namespace Tempo.Api.Models;

public sealed class WeatherResult
{
    public required string City { get; set; }
    public required double TemperatureC { get; set; }
}
