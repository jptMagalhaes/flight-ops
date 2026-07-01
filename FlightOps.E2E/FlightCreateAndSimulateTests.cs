using FlightOps.E2E.Support;
using Microsoft.Playwright;

namespace FlightOps.E2E;

[Trait("Category", "E2E")]
public class FlightCreateAndSimulateTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Login_CreateDepartedFlight_AppearsInSimulation()
    {
        IPage page = await _browser.NewPageAsync();
        await E2ETestContext.LoginAsOperatorAsync(page);
        string aircraftName = await E2ETestContext.CreateUniqueAircraftAsync(page);

        await page.GotoAsync($"{E2ETestContext.BaseUrl}/Flight/Create");
        await page.SelectOptionAsync("#OriginId", new SelectOptionValue { Label = "LIS - Lisbon" });
        await page.SelectOptionAsync("#DestinationId", new SelectOptionValue { Label = "OPO - Porto" });
        await page.SelectOptionAsync("#AircraftId", new SelectOptionValue { Label = aircraftName });
        await page.SelectOptionAsync("#Status", new SelectOptionValue { Label = "Departed" });
        await page.ClickAsync("form[action='/Flight/Create'] button[type=submit]");
        await page.WaitForURLAsync($"{E2ETestContext.BaseUrl}/Flight");

        await Microsoft.Playwright.Assertions.Expect(page.Locator("table"))
            .ToContainTextAsync("Departed", new() { Timeout = 10_000 });

        await page.GotoAsync($"{E2ETestContext.BaseUrl}/Simulation");
        await Microsoft.Playwright.Assertions.Expect(page.Locator("#flightList"))
            .ToContainTextAsync("LIS → OPO", new() { Timeout = 15_000 });
    }

    [Fact]
    public async Task FlightCreateForm_CalculatePreview_PopulatesMetrics()
    {
        IPage page = await _browser.NewPageAsync();
        await E2ETestContext.LoginAsOperatorAsync(page);
        string aircraftName = await E2ETestContext.CreateUniqueAircraftAsync(page);

        await page.GotoAsync($"{E2ETestContext.BaseUrl}/Flight/Create");
        await page.SelectOptionAsync("#OriginId", new SelectOptionValue { Label = "LIS - Lisbon" });
        await page.SelectOptionAsync("#DestinationId", new SelectOptionValue { Label = "OPO - Porto" });
        await page.SelectOptionAsync("#AircraftId", new SelectOptionValue { Label = aircraftName });

        await Microsoft.Playwright.Assertions.Expect(page.Locator("[data-fo-calc-distance]"))
            .Not.ToHaveTextAsync("—", new() { Timeout = 10_000 });
        await Microsoft.Playwright.Assertions.Expect(page.Locator("[data-fo-calc-fuel]"))
            .Not.ToHaveTextAsync("—", new() { Timeout = 10_000 });
        await Microsoft.Playwright.Assertions.Expect(page.Locator("[data-fo-calc-arrival]"))
            .Not.ToHaveTextAsync("—", new() { Timeout = 10_000 });
    }
}
