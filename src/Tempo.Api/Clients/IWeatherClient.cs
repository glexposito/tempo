namespace Tempo.Api.Clients;

public interface IWeatherClient
{
    Task<double?> GetForecastAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}
