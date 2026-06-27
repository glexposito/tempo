using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Tempo.Api.Tests.Fixtures;

namespace Tempo.Api.Tests;

public class TempoApiFactory(WireMockFixture wireMock) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GeocodingApi:BaseUrl"] = wireMock.Url,
                ["WeatherApi:BaseUrl"] = wireMock.Url
            });
        });
    }
}
