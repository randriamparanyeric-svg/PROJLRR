using Microsoft.EntityFrameworkCore;
using PROJLRR.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

/// --- 1. SERVICES ---
builder.Services.AddControllersWithViews();

// On reconstruit le chemin de manière ultra-sécurisée pour IIS
var webRoot = builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
var dbPath = Path.Combine(webRoot, "PERSLRRSANSCODE.db");

// CRUCIAL : Ce bloc va nous dire si le fichier existe vraiment là où .NET le cherche
if (!File.Exists(dbPath))
{
    throw new FileNotFoundException($"[DIAGNOSTIC PROJLRR] Le fichier est introuvable au chemin absolu suivant : {dbPath}");
}

builder.Services.AddDbContext<PerslrrsanscodeContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
// Configuration de l'authentification par Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Gestion des sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// --- 2. MIDDLEWARE (Pipeline) ---

// FORCE L'AFFICHAGE DE LA VRAIE ERREUR EN PRODUCTION (TEMPORAIRE)
app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseRouting();

app.UseSession(); 
app.UseAuthentication(); 
app.UseAuthorization();

// Route par défaut
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();