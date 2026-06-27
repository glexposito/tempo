using Scalar.AspNetCore;
using Tempo.Api.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IGeocodingClient, GeocodingClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("GeocodingApi:BaseUrl")
                                 ?? "https://geocoding-api.open-meteo.com/");
});

builder.Services.AddHttpClient<IWeatherClient, WeatherClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("WeatherApi:BaseUrl")
                                 ?? "https://api.open-meteo.com/");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;

