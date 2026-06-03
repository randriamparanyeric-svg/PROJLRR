using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PROJLRR.ViewModels;
using System.Diagnostics;

public class AccountController : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // On passe le returnUrl à la vue pour ne pas le perdre lors du POST
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        // Affichage des erreurs dans la console de debug de Visual Studio
        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                Debug.WriteLine($"Erreur sur {state.Key}: {error.ErrorMessage}");
            }
        }

        if (!ModelState.IsValid) 
        {
            return View(model); 
        }

        // ===== LOGIQUE DE VÉRIFICATION ET CONNEXION =====
        if (model.Username == "admin" && model.Password == "password123")
        {
            // 1. Création des revendications (Claims) de l'utilisateur
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Role, "Administrator") // Optionnel : si vous gérez des rôles plus tard
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Permet de conserver le cookie selon l'ExpireTimeSpan du Program.cs
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            // 2. Génération du Cookie d'authentification
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), 
                authProperties
            );

            // 3. CLÉ DE SÉCURITÉ ANTI-RESTART : On initialise la session immédiatement après le SignIn
            HttpContext.Session.SetString("ServerReady", "True");

            // 4. Redirection intelligente (soit vers la page demandée initialement, soit vers la recherche)
            return RedirectToLocal(returnUrl);
        }

        // Si l'authentification échoue
        ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe incorrect.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        // Suppression du cookie d'authentification
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Nettoyage complet de la session (supprime "ServerReady")
        HttpContext.Session.Clear();
        
        return RedirectToAction("Login", "Account");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        // Sécurité : Vérifie que l'URL de retour est bien locale au site pour éviter les failles "Open Redirect"
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
            
        // Si pas d'URL locale, redirection par défaut vers les résultats de recherche
        return RedirectToAction("SearchResults", "Search", new { searchTerm = "" });
    }
}