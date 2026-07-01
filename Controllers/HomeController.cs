using System.Diagnostics;
using System.Globalization;
using FlightOps.Features.Home.Queries;
using FlightOps.Models.Pages;
using FlightOps.Models.Pages.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace FlightOps.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    IOperationsDashboardQuery operationsDashboardQuery) : Controller
{
    public async Task<IActionResult> Index()
    {
        OperationsDashboardModel dashboard = await operationsDashboardQuery.BuildAsync();
        return View(dashboard);
    }

    [AllowAnonymous]
    public IActionResult Privacy() => View();

    private static readonly HashSet<string> SupportedCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        "en",
        "pt-PT",
        "de-DE"
    };

    [HttpGet]
    [AllowAnonymous]
    public IActionResult SetCulture(string culture, string returnUrl)
    {
        if (!SupportedCultures.Contains(culture))
            return BadRequest();

        if (IsCurrentCulture(culture))
        {
            if (Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Lax
            });

        if (!Url.IsLocalUrl(returnUrl))
        {
            logger.LogWarning("Rejected non-local returnUrl in SetCulture: {ReturnUrl}", returnUrl);
            returnUrl = Url.Action(nameof(Index)) ?? "/";
        }

        return LocalRedirect(returnUrl);
    }

    private static bool IsCurrentCulture(string culture)
    {
        string current = CultureInfo.CurrentUICulture.Name;
        return current.Equals(culture, StringComparison.OrdinalIgnoreCase)
            || (culture.Equals("en", StringComparison.OrdinalIgnoreCase)
                && current.StartsWith("en", StringComparison.OrdinalIgnoreCase));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View("Layout/Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
