using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tempo.Api.Models;
using Tempo.Api.Tests.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace Tempo.Api.Tests;

public class WeatherEndpointTests : IClassFixture<WireMockFixture>, IAsyncDisposable
{
    private readonly WireMockFixture _wireMock;
    private readonly TempoApiFactory _factory;
    private readonly HttpClient _client;

    public WeatherEndpointTests(WireMockFixture wireMock)
    {
        _wireMock = wireMock;
        _factory = new TempoApiFactory(wireMock);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetWeather_ReturnsTemperature_ForValidCity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange: geocoding returns coordinates for "London"
        _wireMock.Server.Given(
            Request.Create()
                .WithPath("/v1/search")
                .WithParam("name", "London")
                .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"results":[{"name":"London","latitude":51.5074,"longitude":-0.1278}]}"""));

        // Arrange: forecast returns 18.5°C for those coordinates
        _wireMock.Server.Given(
            Request.Create()
                .WithPath("/v1/forecast")
                .WithParam("latitude", "51.5074")
                .WithParam("longitude", "-0.1278")
                .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"current":{"temperature_2m":18.5}}"""));

        // Act
        var response = await _client.GetAsync("/weather/v1/London", cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<WeatherResult>(cancellationToken);
        result.ShouldNotBeNull();
        result.City.ShouldBe("London");
        result.TemperatureC.ShouldBe(18.5);
    }

    [Fact]
    public async Task GetWeather_Returns404_WhenCityNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        // Arrange: geocoding returns no results for "nocity"
        _wireMock.Server.Given(
            Request.Create()
                .WithPath("/v1/search")
                .WithParam("name", "nocity")
                .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"results":[]}"""));

        // Act
        var response = await _client.GetAsync("/weather/v1/nocity", cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("City not found");
        problem.Detail!.ShouldContain("nocity");
    }

    [Fact]
    public async Task GetWeather_Returns404_WhenForecastUnavailable()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        
        // Arrange: geocoding succeeds for "noforecast"
        _wireMock.Server.Given(
            Request.Create()
                .WithPath("/v1/search")
                .WithParam("name", "noforecast")
                .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"results":[{"name":"noforecast","latitude":0.0,"longitude":0.0}]}"""));

        // Arrange: forecast API returns 500 for those coordinates
        _wireMock.Server.Given(
            Request.Create()
                .WithPath("/v1/forecast")
                .WithParam("latitude", "0")
                .WithParam("longitude", "0")
                .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.InternalServerError)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"error":"simulated forecast failure"}"""));

        // Act
        var response = await _client.GetAsync("/weather/v1/noforecast", cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Forecast unavailable");
        problem.Detail!.ShouldContain("noforecast");
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
}
