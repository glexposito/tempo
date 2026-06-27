using System.Net.Http.Json;
using Tempo.Api.Models;

namespace Tempo.Api.Clients;

public sealed class GeocodingClient(HttpClient httpClient) : IGeocodingClient
{
    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string city, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<GeocodingResponse>(
            $"v1/search?name={Uri.EscapeDataString(city)}&count=1",
            cancellationToken);

        var result = response?.Results?.FirstOrDefault();
        if (result is null)
            return null;

        return (result.Latitude, result.Longitude);
    }
}
