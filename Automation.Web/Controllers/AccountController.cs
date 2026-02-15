using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.Interfaces;
using Automation.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Automation.Web.Controllers;

public class AccountController : Controller
{
    private readonly IdentityDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionService _sessionService;

    public AccountController(IdentityDbContext context, IPasswordHasher passwordHasher, ISessionService sessionService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _sessionService = sessionService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = await _context.Set<User>()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserPositions)
                    .ThenInclude(up => up.Position)
                .FirstOrDefaultAsync(u => u.Username == model.Username && !u.IsDeleted);

            if (user != null && _passwordHasher.VerifyHashedPassword(user.PasswordHash, model.Password))
            {
                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim("FullName", $"{user.FirstName} {user.LastName}"),
                    new Claim("UserId", user.Id.ToString())
                };

                // Add role claims
                foreach (var userRole in user.UserRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleCode));
                }

                // Add position claims
                foreach (var userPosition in user.UserPositions)
                {
                    claims.Add(new Claim("Position", userPosition.Position.PositionName));
                    claims.Add(new Claim("PositionId", userPosition.PositionId.ToString()));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8),
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                // Set session for ICurrentContext (OUserId, OPosId)
                _sessionService.SetCurrentUserId(user.Id);
                var firstPosition = user.UserPositions?.FirstOrDefault();
                if (firstPosition != null)
                    _sessionService.SetCurrentPositionId(firstPosition.PositionId);

                // Update last login
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور اشتباه است");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        _sessionService.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}