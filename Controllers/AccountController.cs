using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PROJLRR.ViewModels;
using System.Diagnostics; // Cette ligne permet d'utiliser 'Debug'

public class AccountController : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    // Affichez les erreurs dans la console de Visual Studio (Fenêtre "Sortie" / Output)
    foreach (var state in ModelState)
    {
        foreach (var error in state.Value.Errors)
        {
            Debug.WriteLine($"Erreur sur {state.Key}: {error.ErrorMessage}");
        }
    }

    if (!ModelState.IsValid) 
    {
        // Retourne la vue, mais maintenant vous pourrez voir l'erreur dans la console
        return View(model); 
    }

    // Votre logique de vérification
    if (model.Username == "admin" && model.Password == "password123")
    {
        // ... (votre code SignInAsync)
        return RedirectToAction("SearchResults", "Search");
    }

    ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe incorrect.");
    return View(model);
}

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
            
        return RedirectToAction("SearchResults", "Search", new { searchTerm = "" });
    }
    [HttpGet]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    HttpContext.Session.Clear();
    return RedirectToAction("Login", "Account");
}
}