using FlightOps.E2E.Support;
using Microsoft.Playwright;
using System.Text.Json;

namespace FlightOps.E2E;

[Trait("Category", "E2E")]
public class FlightApiTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _context = await _browser.NewContextAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task AuthenticatedViewer_CanCallFlightsApi()
    {
        IPage page = await _context.NewPageAsync();
        await E2ETestContext.LoginAsViewerAsync(page);

        IAPIResponse response = await _context.APIRequest.GetAsync($"{E2ETestContext.BaseUrl}/api/flights");

        Assert.True(response.Ok);
        JsonDocument json = JsonDocument.Parse(await response.TextAsync());
        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
    }

    [Fact]
    public async Task ActiveFlightsEndpoint_ReturnsJsonArray()
    {
        IPage page = await _context.NewPageAsync();
        await E2ETestContext.LoginAsViewerAsync(page);

        IAPIResponse response =
            await _context.APIRequest.GetAsync($"{E2ETestContext.BaseUrl}/api/flights/active");

        Assert.True(response.Ok);
        JsonDocument json = JsonDocument.Parse(await response.TextAsync());
        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
    }
}
