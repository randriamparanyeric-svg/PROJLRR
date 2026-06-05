using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using PROJLRR.Models; 
using PROJLRR.ViewModels; // Indispensable pour trouver votre LoginViewModel

public class AccountController : Controller
{
    private readonly PerslrrsanscodeContext _context;

    public AccountController(PerslrrsanscodeContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    // On réintègre votre LoginViewModel pour s'accorder parfaitement avec votre vue HTML
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        // 1. Vérification de la validité du modèle (champs requis, etc.)
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // On nettoie les espaces inutiles pour éviter les erreurs de frappe
        string username = model.Username?.Trim() ?? string.Empty;
        string password = model.Password ?? string.Empty;

        // ===== 1. LOGIQUE COMPTE ADMINISTRATEUR (admin / password123) =====
        if (username == "admin" && password == "password123")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) }
            );

            // On lève le verrou de session attendu par le Program.cs
            HttpContext.Session.SetString("ServerReady", "True");

            return RedirectToLocal(returnUrl);
        }

        // ===== 2. LOGIQUE ENSEIGNANT / AGENT (Connexion par CIN) =====
        // L'utilisateur tape son CIN dans Username ET dans Password
        if (username == password && !string.IsNullOrEmpty(username))
        {
            // On cherche dans la table Personnels si le CIN existe
            var personnel = await _context.Personnels
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Cin == username);

            if (personnel != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, personnel.NomEtPrenoms ?? personnel.Cin),
                    new Claim("UserCin", personnel.Cin), // Sauvegarde du CIN pour filtrer ses données
                    new Claim(ClaimTypes.Role, "PersonnelRestreint")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) }
                );

                // On lève le verrou de session attendu par le Program.cs
                HttpContext.Session.SetString("ServerReady", "True");

                // Redirection directe de l'enseignant vers sa fiche de recherche
                return RedirectToAction("SearchResults", "Search");
            }
        }

        // Si aucune correspondance n'est trouvée
        ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe incorrect.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("SearchResults", "Search", new { searchTerm = "" });
    }
}