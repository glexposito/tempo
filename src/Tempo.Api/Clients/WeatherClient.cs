using System.Globalization;
using System.Net.Http.Json;
using Tempo.Api.Models;

namespace Tempo.Api.Clients;

public sealed class WeatherClient(HttpClient httpClient) : IWeatherClient
{
    public async Task<double?> GetForecastAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);

        var response = await httpClient.GetAsync(
            $"v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var forecast = await response.Content.ReadFromJsonAsync<ForecastResponse>(cancellationToken);

        return forecast?.Current?.Temperature2m;
    }
}
