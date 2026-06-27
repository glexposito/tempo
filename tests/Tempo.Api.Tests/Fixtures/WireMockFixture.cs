using WireMock.Server;
using Xunit;

namespace Tempo.Api.Tests.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class WireMockFixture : IAsyncLifetime
{
    public WireMockServer Server { get; private set; } = null!;
    public string Url => Server.Url!;

    public ValueTask InitializeAsync()
    {
        Server = WireMockServer.Start();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Server.Stop();
        Server.Dispose();
        return ValueTask.CompletedTask;
    }
}
