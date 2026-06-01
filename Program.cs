using Microsoft.EntityFrameworkCore;
using PROJLRR.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

/// --- 1. SERVICES ---
builder.Services.AddControllersWithViews();

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
