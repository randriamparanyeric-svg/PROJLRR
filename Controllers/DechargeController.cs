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
    // 1. Récupération des données avec une LEFT JOIN (inclut tout le monde)
    var dechargesRaw = _dbContext.Decharges
        .GroupJoin(_dbContext.Personnels,
            d => d.PersonnelNom.ToLower().Trim(),
            p => p.NomEtPrenoms.ToLower().Trim(),
            (d, personnels) => new { d, personnels })
        .SelectMany(x => x.personnels.DefaultIfEmpty(), // LEFT JOIN : garde les décharges même sans personnel trouvé
            (x, p) => new
            {
                x.d.Id,
                x.d.PersonnelNom,
                x.d.ArticleNom,
                x.d.Quantite,
                x.d.SignaturePath,
                x.d.Unite,
                x.d.DateDecharge,
                // Si le personnel n'existe pas (p est null), on met des valeurs par défaut
                Matricule = p != null ? p.Matricule : "N/A",
                Matiere = p != null ? p.Matiere : "" 
            })
        .OrderBy(x => x.DateDecharge)
        .ToList();

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
            
            DateDechargeReal = x.DateDecharge.HasValue && x.DateDecharge.Value > 0
                ? (x.DateDecharge.Value > 600000000000000000 
                    ? new DateTime(x.DateDecharge.Value) 
                    : DateTimeOffset.FromUnixTimeMilliseconds(x.DateDecharge.Value).DateTime.ToLocalTime()) 
                : DateTime.MinValue,
                
            x.Matricule,
            x.Matiere
        })
        .ToList();

    // 3. Groupement et fusion
    var dechargesFusionnees = decharges
        .GroupBy(d => new { d.PersonnelNom, d.Matricule, d.Matiere, Date = d.DateDechargeReal.Date })
        .Select(g => new DechargeFusionnee
        {
            // 🟢 AJOUTÉ : Permet de donner un Id de référence au bouton de modification (Crayon)
            Id = g.First().Id,

            SignaturePath = g.First().SignaturePath,
            PersonnelNom = g.Key.PersonnelNom,
            MATRICULE = g.Key.Matricule,
            MATIERE = g.Key.Matiere,
            DateDecharge = g.Key.Date, 
            
            ArticlesFusionnes = string.Join("<br/>", g
                .GroupBy(a => new { 
                    NomNettoye = a.ArticleNom?.Trim().ToLower() ?? "", 
                    UniteNettoye = a.Unite?.Trim().ToLower() ?? "" 
                })
                .Select((groupArticle, index) => {
                    var premierArticle = groupArticle.First();
                    var totalQuantite = groupArticle.Sum(d => d.Quantite);
                    
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
    // 1. Récupération des données avec LEFT JOIN pour ne perdre aucune décharge
    var dechargesRaw = _dbContext.Decharges
        .GroupJoin(_dbContext.Personnels,
            d => (d.PersonnelNom ?? "").ToLower().Trim(), // Clé décharge
            p => (p.NomEtPrenoms ?? "").ToLower().Trim(), // Clé personnel
            (d, personnels) => new { d, personnels })
        .SelectMany(x => x.personnels.DefaultIfEmpty(), // Le "Left" : garde tout
            (x, p) => new
            {
                x.d.Id,
                x.d.PersonnelNom,
                x.d.ArticleNom,
                x.d.Quantite,
                x.d.SignaturePath,
                x.d.Unite,
                x.d.DateDecharge,
                // Si le personnel n'est pas trouvé, on met des valeurs par défaut
                Matricule = p != null ? p.Matricule : "N/A",
                Matiere = p != null ? p.Matiere : ""
            })
        .OrderBy(x => x.DateDecharge)
        .ToList();

    // 2. Conversion sécurisée
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

    // 3. Groupement et fusion
    var dechargesFusionnees = decharges
        .GroupBy(d => new { d.PersonnelNom, d.Matricule, d.Matiere, Date = d.DateDechargeReal.Date })
        .Select(g => new DechargeFusionnee
        {
            SignaturePath = g.First().SignaturePath,
            PersonnelNom = g.Key.PersonnelNom,
            MATRICULE = g.Key.Matricule,
            MATIERE = g.Key.Matiere,
            DateDecharge = g.Key.Date,
            
            ArticlesFusionnes = string.Join("<br/>", g
                .GroupBy(a => new { 
                    NomNettoye = a.ArticleNom?.Trim().ToLower() ?? "", 
                    UniteNettoye = a.Unite?.Trim().ToLower() ?? "" 
                })
                .Select((groupArticle, index) => {
                    var premierArticle = groupArticle.First();
                    var totalQuantite = groupArticle.Sum(d => d.Quantite);
                    
                    return $"{index + 1}- {premierArticle.ArticleNom} ({totalQuantite} {premierArticle.Unite})";
                }))
        })
        .OrderBy(df => df.DateDecharge)
        .ToList();

    // 4. Filtrage par jour (IndexBis = Décharges du jour uniquement)
    var dechargesDuJour = dechargesFusionnees
        .Where(df => df.DateDecharge.Date == DateTime.Today)
        .OrderByDescending(df => df.DateDecharge)
        .ToList();

    // ViewBag pour le formulaire (toujours basé sur le tout)
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
            string signatureFinalPath = null;

            // 1. STRATÉGIE DE SIGNATURE DOUBLE CANAL
            if (!string.IsNullOrEmpty(model.SignatureData))
            {
                // Cas A : L'utilisateur a dessiné sur le canvas (Prioritaire)
                try
                {
                    var base64Data = model.SignatureData.Replace("data:image/png;base64,", "");
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    string fileName = Guid.NewGuid().ToString() + ".png";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signatures", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    System.IO.File.WriteAllBytes(filePath, imageBytes);

                    signatureFinalPath = "/signatures/" + fileName;
                }
                // Si l'injection échoue, on lève une alerte sur le dictionnaire d'états
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Erreur de décodage de la signature dessinée : " + ex.Message);
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.Matricule))
            {
                // Cas B : L'utilisateur a saisi son matricule pour appeler son historique
                var personnelValide = _dbContext.Personnels
                    .Any(p => p.NomEtPrenoms == model.PersonnelNom && p.Matricule == model.Matricule.Trim());

                if (!personnelValide)
                {
                    ModelState.AddModelError("Matricule", "Le matricule saisi ne correspond pas au personnel sélectionné.");
                }
                else
                {
                    var derniereDecharge = _dbContext.Decharges
                        .Where(d => d.PersonnelNom == model.PersonnelNom && !string.IsNullOrEmpty(d.SignaturePath))
                        .OrderByDescending(d => d.DateDecharge)
                        .ThenByDescending(d => d.Id)
                        .FirstOrDefault();

                    if (derniereDecharge == null)
                    {
                        ModelState.AddModelError("Matricule", "Aucune signature précédente enregistrée pour cet agent. Veuillez dessiner manuellement.");
                    }
                    else
                    {
                        signatureFinalPath = derniereDecharge.SignaturePath;
                    }
                }
            }
            else
            {
                ModelState.AddModelError("SignatureData", "La validation exige soit une signature manuscrite, soit votre matricule.");
            }

            // Retour direct si le processus de signature a échoué
            if (!ModelState.IsValid)
            {
                ViewBag.Personnel = GetPersonnel();
                ViewBag.Articles = GetArticles();
                return View(model);
            }

            // 2. PREMIÈRE BOUCLE : Validation des stocks de sécurité (Code original préservé)
            var articlesAValider = new List<(PROJLRR.Models.Article ArticleData, PROJLRR.Models.DechargeArticle Item)>();

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

                articlesAValider.Add((articleData, article));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Personnel = GetPersonnel();
                ViewBag.Articles = GetArticles();
                return View(model);
            }

            // 3. DEUXIÈME BOUCLE : Enregistrement (Le Trigger SQLite se charge de soustraire le stock)
            try
            {
                int prochainId = _dbContext.Decharges.Any() ? _dbContext.Decharges.Max(d => d.Id) : 0;

                foreach (var specs in articlesAValider)
                {
                    prochainId++;

                    var decharge = new Decharge
                    {
                        Id = prochainId,
                        PersonnelNom = model.PersonnelNom,
                        ArticleNom = specs.Item.ArticleNom,
                        Quantite = specs.Item.Quantite,
                        Unite = specs.Item.Unite,
                        DateDecharge = model.DateDecharge.Ticks,
                        SignaturePath = signatureFinalPath // Utilisation du chemin final calculé
                    };

                    _dbContext.Decharges.Add(decharge);
                }

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

        // GET: Decharge/GetSignaturePrecedente (Interrogé par AJAX au floutage du Matricule)
        [HttpGet]
        public JsonResult GetSignaturePrecedente(string nom, string matricule)
        {
            if (string.IsNullOrEmpty(nom) || string.IsNullOrEmpty(matricule))
                return Json(new { success = false, message = "Données d'identité incomplètes." });

            var personnelValide = _dbContext.Personnels
                .Any(p => p.NomEtPrenoms == nom.Trim() && p.Matricule == matricule.Trim());

            if (!personnelValide)
                return Json(new { success = false, message = "Le matricule ne correspond pas au personnel sélectionné." });

            var derniereDecharge = _dbContext.Decharges
                .Where(d => d.PersonnelNom == nom.Trim() && !string.IsNullOrEmpty(d.SignaturePath))
                .OrderByDescending(d => d.DateDecharge)
                .ThenByDescending(d => d.Id)
                .FirstOrDefault();

            if (derniereDecharge != null)
            {
                return Json(new { success = true, path = derniereDecharge.SignaturePath });
            }

            return Json(new { success = false, message = "Aucun historique de signature trouvé pour cet agent." });
        }

        [HttpGet]
        public JsonResult GetDerniereDechargePersonnel(string nom)
        {
            if (string.IsNullOrEmpty(nom))
                return Json(null);

            var dechargesDuPersonnel = _dbContext.Decharges
                .Where(d => d.PersonnelNom == nom)
                .ToList();

            if (!dechargesDuPersonnel.Any())
                return Json(new List<object>());

            Func<object, DateTime> interpreterVraieDate = (dateObj) =>
            {
                if (dateObj == null) return DateTime.MinValue;

                if (dateObj is long valLong)
                {
                    if (valLong >= 1000000000000L && valLong < 99999999999999L)
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(valLong).LocalDateTime;
                    }
                    return new DateTime(valLong);
                }

                try { return Convert.ToDateTime(dateObj); }
                catch { return DateTime.MinValue; }
            };

            var resultats = dechargesDuPersonnel
                .GroupBy(d => new 
                {
                    Journee = interpreterVraieDate(d.DateDecharge).Date,
                    NomNettoye = d.ArticleNom?.Trim().ToLower() ?? "",
                    UniteNettoye = d.Unite?.Trim().ToLower() ?? ""
                })
                .OrderByDescending(g => g.Key.Journee)
                .Take(5) 
                .Select(group => {
                    var premierArticle = group.First();
                    var totalQuantite = group.Sum(x => x.Quantite);

                    return new
                    {
                        article = premierArticle.ArticleNom,
                        quantite = totalQuantite.ToString(),
                        unite = premierArticle.Unite,
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
    string searchLower = search.ToLower();

    // 1. Recherche dans la table Personnels (on garde Matricule ici car il existe)
    var queryPersonnels = _dbContext.Personnels
        .Where(p => (p.Matricule ?? "").ToLower().Contains(searchLower) || 
                    (p.NomEtPrenoms ?? "").ToLower().Contains(searchLower))
        .Select(p => new { matricule = p.Matricule, nom = p.NomEtPrenoms });

    // 2. Recherche dans la table Decharges (on retire Matricule car il n'existe pas)
    // On se base uniquement sur le nom du personnel
    var queryDecharges = _dbContext.Decharges
        .Where(d => (d.PersonnelNom ?? "").ToLower().Contains(searchLower))
        .Select(d => new { matricule = "N/A", nom = d.PersonnelNom }); // Matricule est inconnu ici

    // 3. Union et nettoyage
    var resultats = queryPersonnels.Union(queryDecharges)
        .GroupBy(x => x.nom.ToLower().Trim()) 
        .Select(g => g.FirstOrDefault())      
        .Take(10)                             
        .ToList();

    return Json(resultats);
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
    // 1. On récupère la liste du personnel depuis la table de référence
    var sourceReference = _dbContext.Personnels
        .Select(p => new { Matricule = p.Matricule, NomEtPrenoms = p.NomEtPrenoms });

    // 2. On récupère la liste depuis la table des décharges
    // Comme 'Decharge' n'a pas de matricule, on met 'null' pour le champ Matricule
    // afin que la structure de l'objet anonyme corresponde à celle de sourceReference.
    var sourceDecharge = _dbContext.Decharges
        .Where(d => d.PersonnelNom != null) // On ignore les noms nuls
        .Select(d => new { Matricule = (string?)null, NomEtPrenoms = d.PersonnelNom });

    // 3. On fusionne les deux listes, on groupe par nom
    return sourceReference.Union(sourceDecharge)
        .GroupBy(x => x.NomEtPrenoms!.ToLower().Trim()) // On groupe par nom normalisé
        .Select(g => new Personnel 
        { 
            // On prend les infos du premier élément trouvé dans le groupe
            Matricule = g.FirstOrDefault()!.Matricule, 
            NomEtPrenoms = g.FirstOrDefault()!.NomEtPrenoms 
        })
        .OrderBy(p => p.NomEtPrenoms)
        .ToList();
}
        private List<Article> GetArticles()
        {
            return _dbContext.Articles
                .FromSqlRaw("SELECT * FROM ARTICLE WHERE Id IS NOT NULL")
                .Select(a => new Article { Id = a.Id, Nom = a.Nom })
                .OrderBy(a => a.Nom)
                .ToList();
        }
  [HttpGet("Decharge/Edit/{id}")]
public IActionResult Edit(int id)
{
    // 1. Récupération de la ligne source pour avoir la référence (Nom et Date)
    var dechargeSource = _dbContext.Decharges.FirstOrDefault(d => d.Id == id);
    if (dechargeSource == null) return NotFound();

    // 2. Normalisation de la date pour la recherche
    // On convertit les Ticks en date, puis en chaîne "AnnéeMoisJourHeureMinute"
    // Cela permet d'ignorer les différences de secondes/millisecondes lors de la recherche.
    var dateRef = new DateTime(dechargeSource.DateDecharge ?? DateTime.Now.Ticks);
    string dateStr = dateRef.ToString("yyyyMMddHHmm");

    // 3. Récupération de TOUT le groupe
    // On utilise AsEnumerable() pour pouvoir traiter la date en mémoire 
    // et ignorer les variations de Ticks.
    var articlesDuGroupe = _dbContext.Decharges
        .AsEnumerable() 
        .Where(d => d.PersonnelNom == dechargeSource.PersonnelNom 
                 && new DateTime(d.DateDecharge ?? 0).ToString("yyyyMMddHHmm") == dateStr)
        .ToList();

    // 4. Construction du modèle
    var model = new MultiDechargeViewModel
    {
        PersonnelNom = dechargeSource.PersonnelNom,
        DateDecharge = dateRef, // On garde la date originale pour l'affichage
        SignaturePath = dechargeSource.SignaturePath,
        Articles = articlesDuGroupe.Select(d => new DechargeArticle
        {
            ArticleNom = d.ArticleNom,
            Quantite = d.Quantite ?? 0,
            Unite = d.Unite
        }).ToList()
    };

    ViewBag.Personnel = GetPersonnel();
    ViewBag.Articles = GetArticles();
    
    return View(model);
}
[HttpPost("Decharge/Edit/{id}")]
[ValidateAntiForgeryToken]
public IActionResult Edit(int id, MultiDechargeViewModel model)
{
    // 1. Identification de l'ancien groupe (pour suppression et calcul stock)
    var anciennesDecharges = _dbContext.Decharges
        .Where(d => d.PersonnelNom == model.PersonnelNom 
                 && d.DateDecharge == model.DateDecharge.Ticks)
        .ToList();

    string signatureFinalPath = model.SignaturePath; // On conserve l'existant par défaut

    // --- LOGIQUE SIGNATURE (Identique à votre Add) ---
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
            signatureFinalPath = "/signatures/" + fileName;
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Erreur de signature : " + ex.Message);
        }
    }
    // (Ajoutez ici votre logique else if pour le matricule si vous souhaitez la garder)

    if (!ModelState.IsValid)
    {
        ViewBag.Personnel = GetPersonnel();
        ViewBag.Articles = GetArticles();
        return View(model);
    }

    // 2. Validation Stock
    var articlesAValider = new List<(PROJLRR.Models.Article ArticleData, PROJLRR.Models.DechargeArticle Item)>();

    foreach (var article in model.Articles)
    {
        if (string.IsNullOrWhiteSpace(article.ArticleNom) || article.Quantite <= 0) continue;

        var articleData = _dbContext.Articles.FirstOrDefault(a => a.Nom == article.ArticleNom);
        if (articleData == null) { ModelState.AddModelError("", $"Article {article.ArticleNom} introuvable."); continue; }

        // CRUCIAL : On rend les quantités de l'ancienne décharge au stock avant de valider
        int ancienneQuantite = anciennesDecharges.FirstOrDefault(d => d.ArticleNom == article.ArticleNom)?.Quantite ?? 0;
        int stockReelDispo = (articleData.Quantite ?? 0) + ancienneQuantite;
        int stockSec = articleData.StockSec ?? 0;

        if (article.Quantite > stockReelDispo)
        {
            ModelState.AddModelError("", $"Quantité insuffisante pour {article.ArticleNom}.");
            continue;
        }
        
        // Vérification du seuil de sécurité
        if ((stockReelDispo - article.Quantite) < stockSec)
        {
            ModelState.AddModelError("", $"Alerte stock sécurité pour {article.ArticleNom}.");
            continue;
        }

        articlesAValider.Add((articleData, article));
    }

    if (!ModelState.IsValid)
    {
        ViewBag.Personnel = GetPersonnel();
        ViewBag.Articles = GetArticles();
        return View(model);
    }

    // 3. MISE À JOUR TRANSACTIONNELLE (Suppression puis Insertion)
    using var transaction = _dbContext.Database.BeginTransaction();
    try
{
    // 1. Suppression des anciennes lignes
    _dbContext.Decharges.RemoveRange(anciennesDecharges);
    _dbContext.SaveChanges(); // Applique la suppression dans la base

    // --- RECTIFICATION : On vide la mémoire du contexte ---
    // Cette ligne force EF Core à "oublier" les objets supprimés 
    // pour éviter les conflits lors de l'insertion suivante.
    _dbContext.ChangeTracker.Clear();

    // 2. Ajout des nouvelles lignes
    foreach (var specs in articlesAValider)
    {
        var decharge = new Decharge
        {
            // IMPORTANT : Ne définissez PAS l'ID ici si votre colonne est en auto-incrément.
            // Laissez la base de données gérer l'ID automatiquement.
            PersonnelNom = model.PersonnelNom,
            ArticleNom = specs.Item.ArticleNom,
            Quantite = specs.Item.Quantite,
            Unite = specs.Item.Unite,
            DateDecharge = model.DateDecharge.Ticks,
            SignaturePath = signatureFinalPath,
            DateModif = DateTime.Now 
        };
        _dbContext.Decharges.Add(decharge);
    }

    // 3. Enregistrement final
    _dbContext.SaveChanges();
    transaction.Commit();


        TempData["SuccessMessage"] = "Modification enregistrée avec succès !";
        return RedirectToAction("Index"); // Redirigez vers votre liste
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        ModelState.AddModelError("", "Erreur base de données : " + ex.Message);
        ViewBag.Personnel = GetPersonnel();
        ViewBag.Articles = GetArticles();
        return View(model);
    }
}
   }
}