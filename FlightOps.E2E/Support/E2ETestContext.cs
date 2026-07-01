using Microsoft.Playwright;

namespace FlightOps.E2E.Support;

public static class E2ETestContext
{
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5198";

    public static async Task LoginAsync(IPage page, string email, string password)
    {
        await page.GotoAsync($"{BaseUrl}/Account/Login");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", password);
        await page.ClickAsync("form[action='/Account/Login'] button[type=submit]");
        await page.WaitForURLAsync(url => !url.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase));
    }

    public static Task LoginAsOperatorAsync(IPage page) =>
        LoginAsync(page, "operator@flightops.demo", "Operator123!");

    public static Task LoginAsViewerAsync(IPage page) =>
        LoginAsync(page, "viewer@flightops.demo", "Viewer123!");

    public static async Task<string> CreateUniqueAircraftAsync(IPage page)
    {
        string suffix = DateTime.UtcNow.ToString("HHmmssfff");
        string registration = $"E2E-{suffix}";
        string aircraftName = $"E2E Aircraft {suffix}";

        await page.GotoAsync($"{BaseUrl}/Aircraft/Create");
        await page.FillAsync("#Registration", registration);
        await page.FillAsync("#Name", aircraftName);
        await page.FillAsync("#Model", "Test Model");
        await page.SelectOptionAsync("#CurrentAirportId", new SelectOptionValue { Label = "LIS - Lisbon" });
        await page.FillAsync("#TakeOffEffort", "200");
        await page.FillAsync("#FuelConsumptionPerKm", "3");
        await page.FillAsync("#CruiseSpeedKmh", "800");
        await page.ClickAsync("form[action='/Aircraft/Create'] button[type=submit]");
        await page.WaitForURLAsync($"{BaseUrl}/Aircraft");

        return aircraftName;
    }
}
