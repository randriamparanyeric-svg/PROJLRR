using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJLRR.Models;

namespace PROJLRR.Controllers
{
    public class DechargeController : Controller
    {
        private readonly PerslrrsanscodeContext _dbContext;

        public DechargeController(PerslrrsanscodeContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("Decharge/Index")]
public IActionResult Index()
{
    // 1. Récupération des données brutes triées par Ticks (long?) -> Traduisible en SQL
    var dechargesRaw = _dbContext.Decharges
        .Join(_dbContext.Personnels,
            d => d.PersonnelNom,
            p => p.NomEtPrenoms,
            (d, p) => new
            {
                d.Id,
                d.PersonnelNom,
                d.ArticleNom,
                d.Quantite,
                d.SignaturePath,
                d.Unite,
                d.DateDecharge, // On garde le long? brut ici
                p.Matricule,
                p.Matiere
            })
        .OrderBy(x => x.DateDecharge) // Tri sur un type numérique basique (Parfait pour SQL)
        .ToList(); // On bascule les données en mémoire vive (évaluation côté client)

    // 2. Conversion SÉCURISÉE et INTELLIGENTE en DateTime côté C#
    var decharges = dechargesRaw
        .Select(x => new
        {
            x.Id,
            x.PersonnelNom,
            x.ArticleNom,
            x.Quantite,
            x.SignaturePath,
            x.Unite,
            
            // 🔥 LE CORRECTIF : On détecte le format du nombre avant de convertir
            DateDechargeReal = x.DateDecharge.HasValue && x.DateDecharge.Value > 0
                ? (x.DateDecharge.Value > 600000000000000000 
                    ? new DateTime(x.DateDecharge.Value) // Format Ticks (Nouvelles dates)
                    : DateTimeOffset.FromUnixTimeMilliseconds(x.DateDecharge.Value).DateTime.ToLocalTime()) // Format Millisecondes (Anciennes dates)
                : DateTime.MinValue,
                
            x.Matricule,
            x.Matiere
        })
        .ToList();

   // 3. Groupement et fusion (Version blindée contre les espaces et la casse)
var dechargesFusionnees = decharges
    .GroupBy(d => new { d.PersonnelNom, d.Matricule, d.Matiere, Date = d.DateDechargeReal.Date })
    .Select(g => new DechargeFusionnee
    {
        SignaturePath = g.First().SignaturePath,
        PersonnelNom = g.Key.PersonnelNom,
        MATRICULE = g.Key.Matricule,
        MATIERE = g.Key.Matiere,
        DateDecharge = g.Key.Date, 
        
        // Fonctionnalité de fusion et cumul des quantités
        ArticlesFusionnes = string.Join("<br/>", g
            .GroupBy(a => new { 
                NomNettoye = a.ArticleNom?.Trim().ToLower() ?? "", 
                UniteNettoye = a.Unite?.Trim().ToLower() ?? "" 
            })
            .Select((groupArticle, index) => {
                var premierArticle = groupArticle.First();
                var totalQuantite = groupArticle.Sum(d => d.Quantite); // Somme automatique des lignes identiques
                
                return $"{index + 1}- {premierArticle.ArticleNom} ({totalQuantite} {premierArticle.Unite})";
            }))
    })
    .OrderBy(df => df.DateDecharge)
    .ToList();

    var lastDecharge = _dbContext.Decharges
        .OrderByDescending(d => d.DateDecharge)
        .ThenByDescending(d => d.Id)
        .FirstOrDefault();

    if (lastDecharge != null)
    {
        ViewBag.LastPersonnelNom = lastDecharge.PersonnelNom;
    }

    ViewBag.Personnel = GetPersonnel();
    ViewBag.Articles = GetArticles();

    return View(dechargesFusionnees);
}
[HttpGet("Decharge/IndexBis")]
public IActionResult IndexBis()
{
    // 1. Même logique : extraction brute et tri par Ticks d'abord
    var dechargesRaw = _dbContext.Decharges
        .Join(_dbContext.Personnels,
            d => d.PersonnelNom,
            p => p.NomEtPrenoms,
            (d, p) => new
            {
                d.Id,
                d.PersonnelNom,
                d.ArticleNom,
                d.Quantite,
                d.SignaturePath,
                d.Unite,
                d.DateDecharge,
                p.Matricule,
                p.Matiere
            })
        .OrderBy(x => x.DateDecharge)
        .ToList();

    // 2. Conversion en DateTime côté client
    var decharges = dechargesRaw
        .Select(x => new
        {
            x.Id,
            x.PersonnelNom,
            x.ArticleNom,
            x.Quantite,
            x.SignaturePath,
            x.Unite,
            DateDechargeReal = x.DateDecharge.HasValue ? new DateTime(x.DateDecharge.Value) : DateTime.MinValue,
            x.Matricule,
            x.Matiere
        })
        .ToList();
// 3. Groupement et fusion (Version blindée contre les espaces et la casse)
var dechargesFusionnees = decharges
    .GroupBy(d => new { d.PersonnelNom, d.Matricule, d.Matiere, Date = d.DateDechargeReal.Date })
    .Select(g => new DechargeFusionnee
    {
        SignaturePath = g.First().SignaturePath,
        PersonnelNom = g.Key.PersonnelNom,
        MATRICULE = g.Key.Matricule,
        MATIERE = g.Key.Matiere,
        DateDecharge = g.Key.Date, 
        
        // Fonctionnalité de fusion et cumul des quantités
        ArticlesFusionnes = string.Join("<br/>", g
            .GroupBy(a => new { 
                NomNettoye = a.ArticleNom?.Trim().ToLower() ?? "", 
                UniteNettoye = a.Unite?.Trim().ToLower() ?? "" 
            })
            .Select((groupArticle, index) => {
                var premierArticle = groupArticle.First();
                var totalQuantite = groupArticle.Sum(d => d.Quantite); // Somme automatique des lignes identiques
                
                return $"{index + 1}- {premierArticle.ArticleNom} ({totalQuantite} {premierArticle.Unite})";
            }))
    })
    .OrderBy(df => df.DateDecharge)
    .ToList();

    var lastDecharge = _dbContext.Decharges
        .OrderByDescending(d => d.DateDecharge)
        .ThenByDescending(d => d.Id)
        .FirstOrDefault();

    if (lastDecharge != null)
    {
        ViewBag.LastPersonnelNom = lastDecharge.PersonnelNom;
    }

    var dechargesDuJour = dechargesFusionnees
        .Where(df => df.DateDecharge.Date == DateTime.Today)
        .OrderByDescending(df => df.DateDecharge)
        .ToList();

    ViewBag.Personnel = GetPersonnel();
    ViewBag.Articles = GetArticles();

    return View(dechargesDuJour);
}
        [HttpGet("Decharge/Add")]
        public IActionResult Add()
        {
            var model = new MultiDechargeViewModel
            {
                DateDecharge = DateTime.Now,
                Articles = new List<DechargeArticle> { new DechargeArticle() }
            };

            if (TempData["LastPersonnelNom"] != null)
            {
                model.PersonnelNom = TempData["LastPersonnelNom"].ToString();
                ViewBag.LastPersonnelNom = model.PersonnelNom;
            }

            ViewBag.Personnel = GetPersonnel();
            ViewBag.Articles = GetArticles();
            return View(model);
        }

        [HttpPost("Decharge/Add")]
public IActionResult Add(MultiDechargeViewModel model)
{
    // 1. Gestion et enregistrement de la signature
    if (!string.IsNullOrEmpty(model.SignatureData))
    {
        try
        {
            var base64Data = model.SignatureData.Replace("data:image/png;base64,", "");
            byte[] imageBytes = Convert.FromBase64String(base64Data);

            string fileName = Guid.NewGuid().ToString() + ".png";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signatures", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            System.IO.File.WriteAllBytes(filePath, imageBytes);

            model.SignaturePath = "/signatures/" + fileName;
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Erreur signature : " + ex.Message);
        }
    }
    else
    {
        ModelState.AddModelError("SignatureData", "Veuillez fournir une signature.");
    }

    // Arrêt immédiat si la signature ou le modèle de base est invalide
    if (!ModelState.IsValid)
    {
        ViewBag.Personnel = GetPersonnel();
        ViewBag.Articles = GetArticles();
        return View(model);
    }

    // 2. PREMIÈRE BOUCLE : Validation stricte de TOUS les articles (sécurité anti-doublon)
    var articlesAValider = new List<(PROJLRR.Models.Article ArticleData, PROJLRR.Models.DechargeArticle Item, int NewQty)>();

    foreach (var article in model.Articles)
    {
        if (string.IsNullOrWhiteSpace(article.ArticleNom) || article.Quantite <= 0)
            continue;

        var articleData = _dbContext.Articles.FirstOrDefault(a => a.Nom == article.ArticleNom);
        if (articleData == null)
        {
            ModelState.AddModelError("", $"Article {article.ArticleNom} introuvable.");
            continue;
        }

        int currentQuantity = articleData.Quantite ?? 0;
        int stockSec = articleData.StockSec ?? 0;
        int newQuantity = currentQuantity - article.Quantite;

        if (article.Quantite > currentQuantity)
        {
            ModelState.AddModelError("", $"Quantité insuffisante pour {article.ArticleNom}.");
            continue;
        }

        if (newQuantity < stockSec)
        {
            ModelState.AddModelError("", $"Alerte stock sécurité pour {article.ArticleNom}.");
            continue;
        }

        // Si l'article est valide, on le garde en mémoire pour l'étape suivante
        articlesAValider.Add((articleData, article, newQuantity));
    }

    // Si un seul article a échoué aux validations, on réaffiche la vue sans rien toucher en BDD
    if (!ModelState.IsValid)
    {
        ViewBag.Personnel = GetPersonnel();
        ViewBag.Articles = GetArticles();
        return View(model);
    }

    // 3. DEUXIÈME BOUCLE : Application et Sauvegarde (Tout est OK)
    try
    {
        // 🔥 SOLUTION ANTY-CRASH SQLITE : On calcule le prochain ID disponible manuellement
        int prochainId = _dbContext.Decharges.Any() ? _dbContext.Decharges.Max(d => d.Id) : 0;

        foreach (var specs in articlesAValider)
        {
            // Mise à jour du stock de l'article
            specs.ArticleData.Quantite = specs.NewQty;
            _dbContext.Articles.Update(specs.ArticleData);

            // Incrémentation manuelle de l'ID pour contourner le manque d'autoincrement de SQLite
            prochainId++;

            // Création de la décharge
            var decharge = new Decharge
            {
                Id = prochainId, // 🔥 On force l'ID ici
                PersonnelNom = model.PersonnelNom,
                ArticleNom = specs.Item.ArticleNom,
                Quantite = specs.Item.Quantite,
                Unite = specs.Item.Unite,
                DateDecharge = model.DateDecharge.Ticks,
                SignaturePath = model.SignaturePath
            };

            _dbContext.Decharges.Add(decharge);
        }

        // Sauvegarde finale de toutes les modifications d'un coup
        _dbContext.SaveChanges();

        TempData["SuccessMessage"] = "Décharge enregistrée avec succès !";
        TempData["LastPersonnelNom"] = model.PersonnelNom;
        return RedirectToAction("Add");
    }
    catch (Exception ex)
    {
        var vraieErreur = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        ModelState.AddModelError("", $"Erreur Base de données : {vraieErreur}");
        
        ViewBag.Personnel = GetPersonnel();
        ViewBag.Articles = GetArticles();
        return View(model);
    }
}
 [HttpGet]
public JsonResult GetDerniereDechargePersonnel(string nom)
{
    if (string.IsNullOrEmpty(nom))
        return Json(null);

    // 1. On récupère toutes les décharges de cette personne en mémoire C#
    var dechargesDuPersonnel = _dbContext.Decharges
        .Where(d => d.PersonnelNom == nom)
        .ToList();

    if (!dechargesDuPersonnel.Any())
        return Json(new List<object>());

    // Fonction de conversion pour corriger définitivement le bug du 03/01/0001
    Func<object, DateTime> interpreterVraieDate = (dateObj) =>
    {
        if (dateObj == null) return DateTime.MinValue;

        if (dateObj is long valLong)
        {
            // Timestamp Unix Millisecondes (13 chiffres) -> Anciennes dates
            if (valLong >= 1000000000000L && valLong < 99999999999999L)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(valLong).LocalDateTime;
            }
            // Ticks .NET (18 chiffres) -> Nouvelles dates
            return new DateTime(valLong);
        }

        try { return Convert.ToDateTime(dateObj); }
        catch { return DateTime.MinValue; }
    };

    // 2. On GROUPE par JOURNÉE pure (sans l'heure) et par ARTICLE pour fusionner les doublons
    var resultats = dechargesDuPersonnel
        .GroupBy(d => new 
        {
            Journee = interpreterVraieDate(d.DateDecharge).Date,
            NomNettoye = d.ArticleNom?.Trim().ToLower() ?? "",
            UniteNettoye = d.Unite?.Trim().ToLower() ?? ""
        })
        // 3. On trie pour avoir les journées les plus récentes en premier
        .OrderByDescending(g => g.Key.Journee)
        // 4. On prend exactement les 5 dernières lignes de l'historique fusionné
        .Take(5) 
        .Select(group => {
            var premierArticle = group.First();
            var totalQuantite = group.Sum(x => x.Quantite); // Somme des quantités du même jour

            return new
            {
                article = premierArticle.ArticleNom,
                quantite = totalQuantite.ToString(),
                unite = premierArticle.Unite,
                // Format propre sans l'heure (ex: 02/06/2026)
                date = group.Key.Journee.ToString("dd/MM/yyyy")
            };
        })
        .ToList();

    return Json(resultats);
}
        [HttpGet]
public JsonResult SearchPersonnel(string search)
{
    if (string.IsNullOrEmpty(search)) return Json(new List<object>());

    // 1. On passe la recherche en minuscules une seule fois
    string searchLower = search.ToLower();

    // 2. On compare en appliquant .ToLower() sur les champs de la base de données
    var personnels = _dbContext.Personnels
        .Where(p => p.Matricule.ToLower().Contains(searchLower) || 
                    p.NomEtPrenoms.ToLower().Contains(searchLower))
        .Select(p => new { matricule = p.Matricule, nom = p.NomEtPrenoms })
        .ToList();

    return Json(personnels);
}

        [HttpGet]
        public JsonResult SearchPersonnel5(string search)
        {
            if (string.IsNullOrEmpty(search)) return Json(new List<object>());

            var personnels = _dbContext.Personnels
                .Where(p => p.NomEtPrenoms.Contains(search))
                .Select(p => new { nom = p.NomEtPrenoms })
                .ToList();

            return Json(personnels);
        }

        [HttpGet]
        public IActionResult GetArt()
        {
            var articles = _dbContext.Articles
                .Select(a => new { nom = a.Nom, unite = a.Unite, quantite = a.Quantite })
                .OrderBy(a => a.nom)
                .ToList();

            return Json(articles);
        }

        private List<Personnel> GetPersonnel()
        {
            return _dbContext.Personnels
                .Select(p => new Personnel { Matricule = p.Matricule, NomEtPrenoms = p.NomEtPrenoms })
                .OrderBy(p => p.NomEtPrenoms)
                .ToList();
        }

private List<Article> GetArticles()
{
    return _dbContext.Articles
        .FromSqlRaw("SELECT * FROM ARTICLE WHERE Id IS NOT NULL") // 👈 SQLite nettoie avant qu'EF Core ne touche aux données
        .Select(a => new Article { Id = a.Id, Nom = a.Nom })
        .OrderBy(a => a.Nom)
        .ToList();
}
    }
}