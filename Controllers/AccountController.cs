using FlightOps.Entities;
using FlightOps.Models.Forms.Account;
using FlightOps.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics.CodeAnalysis;

namespace FlightOps.Controllers;

public class AccountController(
    SignInManager<ApplicationUser> signInManager,
    ILogger<AccountController> logger,
    IStringLocalizer<SharedResources> localizer) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await signInManager.PasswordSignInAsync(
            model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, localizer["Login.InvalidCredentials"]);
            return View(model);
        }

        logger.LogInformation("User logged in: {Email}", model.Email);

        if (TryGetPostLoginReturnUrl(model.ReturnUrl, out string? safeReturnUrl))
            return LocalRedirect(safeReturnUrl);

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    private bool TryGetPostLoginReturnUrl(string? returnUrl, [NotNullWhen(true)] out string? safeReturnUrl)
    {
        safeReturnUrl = null;

        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
            return false;

        string path = returnUrl.Split('?', 2)[0].TrimEnd('/').ToLowerInvariant();

        if (path is "" or "/home" or "/home/index"
            or "/account/login"
            or "/account/accessdenied"
            or "/home/privacy")
            return false;

        safeReturnUrl = returnUrl;
        return true;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
