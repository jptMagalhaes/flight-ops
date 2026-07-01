using FlightOps.E2E.Support;
using Microsoft.Playwright;

namespace FlightOps.E2E;

[Trait("Category", "E2E")]
public class FlightScheduleConflictTests : IAsyncLifetime
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
    public async Task OverlappingFlights_ShowsScheduleConflictError()
    {
        IPage page = await _browser.NewPageAsync();
        await E2ETestContext.LoginAsOperatorAsync(page);
        string aircraftName = await E2ETestContext.CreateUniqueAircraftAsync(page);

        string departure = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-ddTHH:mm");

        await page.GotoAsync($"{E2ETestContext.BaseUrl}/Flight/Create");
        await page.SelectOptionAsync("#OriginId", new SelectOptionValue { Label = "LIS - Lisbon" });
        await page.SelectOptionAsync("#DestinationId", new SelectOptionValue { Label = "OPO - Porto" });
        await page.SelectOptionAsync("#AircraftId", new SelectOptionValue { Label = aircraftName });
        await page.FillAsync("#DepartureTime", departure);
        await page.SelectOptionAsync("#Status", new SelectOptionValue { Label = "Scheduled" });
        await page.ClickAsync("form[action='/Flight/Create'] button[type=submit]");
        await page.WaitForURLAsync($"{E2ETestContext.BaseUrl}/Flight");

        await page.GotoAsync($"{E2ETestContext.BaseUrl}/Flight/Create");
        await page.SelectOptionAsync("#OriginId", new SelectOptionValue { Label = "LIS - Lisbon" });
        await page.SelectOptionAsync("#DestinationId", new SelectOptionValue { Label = "OPO - Porto" });
        await page.SelectOptionAsync("#AircraftId", new SelectOptionValue { Label = aircraftName });
        await page.FillAsync("#DepartureTime", departure);
        await page.SelectOptionAsync("#Status", new SelectOptionValue { Label = "Scheduled" });
        await page.ClickAsync("form[action='/Flight/Create'] button[type=submit]");

        await Microsoft.Playwright.Assertions.Expect(
                page.Locator("form[action='/Flight/Create'] .validation-summary-errors"))
            .ToContainTextAsync("already has a flight in that time window", new() { Timeout = 10_000 });
    }
}
