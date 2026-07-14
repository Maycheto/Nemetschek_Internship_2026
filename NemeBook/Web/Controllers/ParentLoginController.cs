using Entities.Enums;
using Entities.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Interfaces.Registration;
using Services.Services.Registration;
using System.Security.Claims;
using Web.ViewModels;

namespace Web.Controllers;

public class ParentLoginController : Controller
{
    private readonly IRegistrationService registrationService;
    private readonly IAuthService authService;
    private readonly ILogger<ParentLoginController> logger;

    public ParentLoginController(
        IRegistrationService registrationService,
        IAuthService authService,
        ILogger<ParentLoginController> logger)
    {
        this.registrationService = registrationService;
        this.authService = authService;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Content("Missing token.");

        try
        {
            await registrationService.ValidateInvitationAsync(
                token,
                RegistrationInvitationType.ParentSignUp);
        }
        catch
        {
            return Content("Invalid or expired parent login code.");
        }

        return View(new ParentLoginViewModel { Token = token });
    }

    [HttpPost]
    public async Task<IActionResult> Index(ParentLoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // 1) Валидираме токена
        try
        {
            await registrationService.ValidateInvitationAsync(
                model.Token,
                RegistrationInvitationType.ParentSignUp);
        }
        catch
        {
            ModelState.AddModelError("", "Invalid or expired login code.");
            return View(model);
        }

        // 2) Взимаме поканата чрез email
        // Поканата съдържа Email → взимаме я чрез accountsRepository
        var invitation = await registrationService.GetInvitationByTokenAsync(model.Token);
        if (invitation == null)
        {
            ModelState.AddModelError("", "Invitation not found.");
            return View(model);
        }

        if (invitation.UserId == null)
        {
            ModelState.AddModelError("", "Parent account is not created yet.");
            return View(model);
        }

        // 3) Взимаме потребителя
        var user = await authService.GetUserByIdAsync(invitation.UserId.Value);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            return View(model);
        }

        // 4) Логваме потребителя
        var loginResult = await authService.LoginAsync(new LoginRequest
        {
            Email = user.Email,
            Password = model.Password,
            RememberMe = true
        });

        if (loginResult == null)
        {
            ModelState.AddModelError("", "Incorrect password.");
            return View(model);
        }

        // 5) Claims
        var claims = new List<Claim>
    {
        new Claim("UserId", user.Id.ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return RedirectToAction("Index", "ParentHome");
    }

}

