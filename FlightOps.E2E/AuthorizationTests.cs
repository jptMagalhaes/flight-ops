using FlightOps.E2E.Support;
using Microsoft.Playwright;

namespace FlightOps.E2E;

[Trait("Category", "E2E")]
public class AuthorizationTests : IAsyncLifetime
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
    public async Task UnauthenticatedUser_RedirectedToLogin()
    {
        IPage page = await _browser.NewPageAsync();

        await page.GotoAsync($"{E2ETestContext.BaseUrl}/Flight");

        await page.WaitForURLAsync(url =>
            url.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase));
        await Microsoft.Playwright.Assertions.Expect(page.Locator("#Email")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Viewer_CannotAccessFlightCreate_ShowsAccessDenied()
    {
        IPage page = await _browser.NewPageAsync();
        await E2ETestContext.LoginAsViewerAsync(page);

        await page.GotoAsync($"{E2ETestContext.BaseUrl}/Flight/Create");

        await page.WaitForURLAsync(url =>
            url.Contains("/Account/AccessDenied", StringComparison.OrdinalIgnoreCase));
        await Microsoft.Playwright.Assertions.Expect(page.Locator("h1"))
            .ToContainTextAsync("Access denied");
    }

    [Fact]
    public async Task Viewer_CanBrowseFlightList()
    {
        IPage page = await _browser.NewPageAsync();
        await E2ETestContext.LoginAsViewerAsync(page);

        await page.GotoAsync($"{E2ETestContext.BaseUrl}/Flight");

        await Microsoft.Playwright.Assertions.Expect(page.Locator("table")).ToBeVisibleAsync();
    }
}
