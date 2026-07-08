// 1. TOUTES les directives 'using' en haut
using Microsoft.EntityFrameworkCore;
using PROJLRR.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authentication;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

var builder = WebApplication.CreateBuilder(args);
// --- CONFIGURATION DE QUESTPDF ---
// À placer ici pour être initialisé au démarrage de l'app
QuestPDF.Settings.License = LicenseType.Community;
// --- 1. SERVICES ---

// MODIFICATION : Ajout du Verrouillage Global des Contrôleurs
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Déterminer l'environnement
bool isDevelopment = builder.Environment.IsDevelopment();
string dbPath = DetermineDbPath(builder.Environment, isDevelopment);

// Créer le dossier s'il n'existe pas
string dbDirectory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

// Afficher le chemin pour le diagnostic
Console.WriteLine($"[INFO] Environnement: {(isDevelopment ? "DÉVELOPPEMENT" : "PRODUCTION")}");
Console.WriteLine($"[INFO] Chemin de la base de données : {dbPath}");
Console.WriteLine($"[INFO] Fichier existe : {File.Exists(dbPath)}");
Console.WriteLine($"[INFO] ContentRootPath: {builder.Environment.ContentRootPath}");
Console.WriteLine($"[INFO] WebRootPath: {builder.Environment.WebRootPath}");

// Configurer le DbContext avec le chemin déterminé
builder.Services.AddDbContext<PerslrrsanscodeContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// MODIFICATION : Sécurisation de l'authentification par Cookies (Anti-Restart et Anti-Cache)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Aligné sur la session pour la cohérence
        options.SlidingExpiration = true;

        options.Events = new CookieAuthenticationEvents
        {
            // CETTE FONCTION BLOQUE L'ACCÈS APRÈS UN REDÉMARRAGE DU SERVEUR
            OnValidatePrincipal = async context =>
            {
                // Éviter une boucle infinie si on est déjà sur la page de login
                var path = context.Request.Path.Value ?? "";
                if (path.StartsWith("/Account/Login", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Si la session "ServerReady" est vide alors que le cookie est présent -> Le serveur a redémarré !
                if (string.IsNullOrEmpty(context.HttpContext.Session.GetString("ServerReady")))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            },
            // EMPECHE LE NAVIGATEUR D'AFFICHER LA PAGE EN CACHE (F5 / Bouton Retour)
            OnRedirectToLogin = context =>
            {
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

// Gestion des sessions (Configuration robuste héritée)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Aligné à 2h comme le cookie
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// --- 2. MIDDLEWARE (Pipeline - L'ordre est vital) ---

// Configuration conditionnelle selon l'environnement
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Les fichiers statiques (images, CSS, JS) restent cachables pour préserver les performances
app.UseStaticFiles();

// 1. Activer la session en premier (obligatoire pour le OnValidatePrincipal des cookies)
app.UseSession(); 

// 2. Middleware Anti-Cache Global pour sécuriser les pages HTML dynamiques
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseRouting();

// 3. Authentification et Autorisation
app.UseAuthentication(); 
app.UseAuthorization();

// Route par défaut
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

/// <summary>
/// Détermine le chemin de la base de données en fonction de l'environnement
/// - LOCAL (Développement) : C:\Users\HP\PROJLRR\PERSLRRSANSCODE.db
/// - HÉBERGEMENT (Production) : wwwroot/PERSLRRSANSCODE.db
/// </summary>
string DetermineDbPath(IWebHostEnvironment env, bool isDevelopment)
{
    if (isDevelopment)
    {
        // ===== ENVIRONNEMENT LOCAL (DÉVELOPPEMENT) =====
        Console.WriteLine("[DEBUG] Mode DÉVELOPPEMENT activé");
        
        // Chemin local de développement
        var localDevDb = @"C:\Users\HP\PROJLRR\PERSLRRSANSCODE.db";
        
        if (File.Exists(localDevDb))
        {
            Console.WriteLine($"[SUCCESS] Chemin local trouvé : {localDevDb}");
            return localDevDb;
        }

        // Sinon, essayer à la racine du projet
        var rootDb = Path.Combine(env.ContentRootPath, "PERSLRRSANSCODE.db");
        if (File.Exists(rootDb))
        {
            Console.WriteLine($"[SUCCESS] Base de données trouvée à la racine : {rootDb}");
            return rootDb;
        }

        // Par défaut en développement, utiliser la racine du projet
        Console.WriteLine($"[WARNING] Utilisation du chemin par défaut (racine du projet) : {rootDb}");
        return rootDb;
    }
    else
    {
        // ===== ENVIRONNEMENT HÉBERGEMENT (PRODUCTION) =====
        Console.WriteLine("[DEBUG] Mode PRODUCTION activé");
        
        // En production, utiliser wwwroot directement
        var wwwrootPath = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var productionDb = Path.Combine(wwwrootPath, "PERSLRRSANSCODE.db");

        Console.WriteLine($"[INFO] Chemin de production : {productionDb}");
        Console.WriteLine($"[SUCCESS] Base de données en production sera stockée dans : wwwroot/");
        
        return productionDb;
    }
}