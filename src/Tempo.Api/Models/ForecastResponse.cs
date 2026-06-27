using System.Text.Json.Serialization;

namespace Tempo.Api.Models;

public sealed class ForecastResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather? Current { get; set; }
}

public sealed class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature2m { get; set; }
}
