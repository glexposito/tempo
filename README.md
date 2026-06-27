# Tempo

A proof-of-concept weather API built to demonstrate **service mocking** (also known as service virtualisation) in .NET.

## Why mock services?

When you build services that depend on other services, you need those dependencies running so you can develop and test. But spinning up real services is expensive, and most of the time it's just not possible. Think about working at a place like AWS or Azure — there are hundreds of teams. You can't deploy real instances of everything for everyone. The only way to make it work is to mock those dependencies.

With Docker Compose this is pretty straightforward. You run containers for things like SQL Server, Cosmos DB, or Service Bus, and for internal services each team can provide a small Docker image that behaves like their API. If no image exists, you mock it yourself.

For cloud services, tools like [Floci](https://floci.io/) (AWS, Azure, GCP) and [LocalStack](https://localstack.cloud/) (AWS) do the same thing — they emulate cloud APIs on your machine so you can work with S3, DynamoDB, or Service Bus without a cloud account. Same idea: replace what you don't control with something you do.

If you don't mock your dependencies, you're tying yourself to the hope that every other system is up and healthy. When something goes down — and it will — your service breaks too, even though your code is fine. That's not a testing problem, it's a design problem.

With mocked dependencies, your system is still fully testable end to end. You can explore, test edge cases, and keep developing without worrying about what's up or down.

Real end-to-end testing against real services belongs in staging and production. Depending on how complex the system is, full end-to-end might only be realistic in production itself.

## What is this?

A single endpoint — `GET /weather/v1/{city}` — that returns the current temperature for a city. Internally it chains two external API calls (geocoding → forecast), which makes it a good teaching example for:

- **Why** mock — the external APIs may not be available and are expensive to spin up and maintain
- **How** to mock — the handler depends on two services through interfaces, so you mock the contracts and focus on testing inputs and outputs

## Architecture

```
GET /weather/v1/{city}
        │
   WeatherController
        │
   1. IGeocodingClient  ──►  Geocoding API  (city → lat/lon)
   2. IWeatherClient    ──►  Forecast API   (lat/lon → temperature)
   3. Return { city, temperatureC }
```

## Running locally (real services)

When running locally, the API calls the real services:

- Geocoding: https://geocoding-api.open-meteo.com/v1/search
- Forecast: https://api.open-meteo.com/v1/forecast

```bash
dotnet run --project src/Tempo.Api
```

- API: http://localhost:5297/weather/v1/Auckland
- Docs: http://localhost:5297/scalar/v1

## Running with Docker (service mocking/virtualisation)

A Node.js mock replaces both external APIs, so the entire system runs without internet access. The API is still fully testable end to end — you can explore it, test edge cases, and develop new features without worrying about external services being available or breaking.

```bash
docker compose up --build
```

- API: http://localhost:8080/weather/v1/Auckland
- Docs: http://localhost:8080/scalar/v1
- Mock: http://localhost:3000

### Test scenarios

| City           | Behaviour                                  |
|----------------|--------------------------------------------|
| Any string     | Happy path — random coordinates and temp   |
| `nocity`       | Geocoding returns no results → 404         |
| `noforecast`   | Geocoding succeeds, forecast fails → 404   |

Both failure cases return [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) Problem Details responses.

## Tests

```bash
dotnet test
```

## Tech stack

- .NET 10 / ASP.NET Core
- Typed `HttpClient`s with interfaces (`IGeocodingClient`, `IWeatherClient`)
- [Scalar](https://github.com/ScalarHQ/scalar) for API docs
- xUnit v3 + WireMock.Net + Shouldly for integration tests
- Docker Compose + Express mock for service virtualisation
