namespace Tempo.Api.Clients;

public interface IGeocodingClient
{
    Task<(double Latitude, double Longitude)?> GeocodeAsync(string city, CancellationToken cancellationToken = default);
}
