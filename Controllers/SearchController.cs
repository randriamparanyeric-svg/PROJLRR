using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJLRR.Models;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace PROJLRR.Controllers
{
    public class SearchController : Controller
    {
        private readonly PerslrrsanscodeContext _context;

        // 🧠 Mémoire vive globale pour suivre la présence en ligne des enseignants en temps réel
        private static readonly ConcurrentDictionary<string, DateTime> UtilisateursEnLigne = new ConcurrentDictionary<string, DateTime>();

        public SearchController(PerslrrsanscodeContext context)
        {
            _context = context;
        }

        // =================================================================
        // 1. RECHERCHE PRINCIPALE (Vue complète) - Optimisé AsNoTracking
        // =================================================================
        [HttpGet]
        [Authorize] 
        public IActionResult SearchResults(string searchTerm = "")
        {
            List<Personnel> results = new List<Personnel>();
            bool isAdmin = User.IsInRole("Admin");
            string userNom = "";

            if (!isAdmin)
            {
                var userCin = User.FindFirst("UserCin")?.Value;

                if (!string.IsNullOrEmpty(userCin))
                {
                    // Utilisation de AsNoTracking() : économise la mémoire vive sur MonsterASP
                    var monProfil = _context.Personnels.AsNoTracking().FirstOrDefault(p => p.Cin == userCin);
                    if (monProfil != null)
                    {
                        userNom = monProfil.NomEtPrenoms;
                        searchTerm = userNom; 
                        results.Add(monProfil);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.ToLower();
                    results = _context.Personnels
                        .AsNoTracking() // Gain de performance immédiat pour la recherche administrative
                        .Where(p => (p.NomEtPrenoms != null && p.NomEtPrenoms.ToLower().Contains(term)) || 
                                    (p.Matricule != null && p.Matricule.ToLower().Contains(term)))
                        .ToList(); 
                }
            }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.UserNom = userNom;
            ViewBag.SearchTerm = searchTerm;

            return View(results);
        }

        // =================================================================
        // 2. DÉTAIL (JSON) - Optimisé AsNoTracking
        // =================================================================
        [HttpGet]
        public JsonResult GetPersonnelDetails(int id)
        {
            var personnel = _context.Personnels.AsNoTracking().FirstOrDefault(p => p.Num == id);
            return Json(personnel);
        }

        // =================================================================
        // 3. MISE À JOUR + ANALYSE EXHAUSTIVE DE TOUS LES ÉCARTS
        // =================================================================
       [HttpPost]
public async Task<IActionResult> UpdatePersonnel([FromForm] Personnel updatedPersonnel, IFormFile? PhotoFile)
{
    if (updatedPersonnel == null) 
        return BadRequest(new { success = false, message = "Données invalides" });

    var existing = await _context.Personnels.FindAsync(updatedPersonnel.Num);
    if (existing == null) 
        return NotFound(new { success = false, message = "Personnel non trouvé" });

    // --- 1. GESTION DE LA PHOTO ---
    if (PhotoFile != null && PhotoFile.Length > 0)
    {
        try
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = updatedPersonnel.Matricule + Path.GetExtension(PhotoFile.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await PhotoFile.CopyToAsync(stream);
            }

            updatedPersonnel.Photo = "/photos/" + fileName;
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Erreur lors de l'enregistrement de l'image : " + ex.Message });
        }
    }
    else
    {
        updatedPersonnel.Photo = existing.Photo;
    }

    // --- 2. ENREGISTREMENT DES DATES AU FORMAT FR (JJ/MM/AAAA) ---
    string FormaterDateFr(string date)
    {
        if (!string.IsNullOrEmpty(date) && date.Contains("-"))
        {
            var parts = date.Split('-');
            return parts.Length == 3 ? $"{parts[2]}/{parts[1]}/{parts[0]}" : date;
        }
        return date;
    }

    updatedPersonnel.Datenaiss = FormaterDateFr(updatedPersonnel.Datenaiss);
    updatedPersonnel.Datedentre = FormaterDateFr(updatedPersonnel.Datedentre);
    updatedPersonnel.Datedeprise = FormaterDateFr(updatedPersonnel.Datedeprise);

    // --- 3. 🔔 DÉTECTION AUTOMATIQUE DES ÉCARTS (RÉFLEXION) ---
    var changements = new List<object>();

    // Dictionnaire complet et personnalisé (Interface humaine)
    var libellesChamps = new Dictionary<string, string>
    {
        // Informations Générales
        { "Matricule", "Matricule" }, 
        { "NomEtPrenoms", "Nom & Prénoms" }, 
        { "Cin", "CIN" },
        { "Dec", "Dec" }, 
        { "Corps", "Corps" }, 
        { "Matiere", "Matière" },
        { "Datenaiss", "Date de Naissance" }, 
        { "Lieudenaiss", "Lieu de Naissance" }, 
        { "Sexe", "Sexe" },
        { "Statut", "Statut" }, 
        { "Datedentre", "Date Entrée" }, 
        { "Datedeprise", "Date Prise" },
        { "Diplomeac", "Diplôme Académique" }, 
        { "Diplomeped", "Diplôme Pédagogique" },
        { "Contact", "Contact" }, 
        { "Fonction", "Fonction" }, 
        { "Grade", "Grade" }, 
        { "SerieBacc", "Série du Bacc" },

        // 🌟 Suivi des 8 Classes tenues
        { "ClasseTenue1", "1ère Classe tenue" },
        { "ClasseTenue2", "2ème Classe tenue" },
        { "ClasseTenue3", "3ème Classe tenue" },
        { "ClasseTenue4", "4ème Classe tenue" },
        { "ClasseTenue5", "5ème Classe tenue" },
        { "ClasseTenue6", "6ème Classe tenue" },
        { "ClasseTenue7", "7ème Classe tenue" },
        { "ClasseTenue8", "8ème Classe tenue" },

        // 📈 Historique des Avancements (Du 1er au 16ème)
        { "Perav", "1er avancement" },
        { "Demav", "2ème avancement" },
        { "Temav", "3ème avancement" },
        { "Qemav", "4ème avancement" },
        { "Cemav", "5ème avancement" },
        { "Semav", "6ème avancement" },
        { "Sepmav", "7ème avancement" },
        { "Hemav", "8ème avancement" },
        { "Nemav", "9ème avancement" },
        { "Dxemav", "10ème avancement" },
        { "Onemav", "11ème avancement" },
        { "Dou", "12ème avancement" },
        { "Trei", "13ème avancement" },
        { "Quat", "14ème avancement" },
        { "Quin", "15ème avancement" },
        { "Seiz", "16ème avancement" }
    };

    // Parcours dynamique de toutes les propriétés de la classe Personnel
    var proprietes = typeof(Personnel).GetProperties();
    foreach (var prop in proprietes)
    {
        // 🛑 EXCLUSIONS : On ignore l'ID, la photo et la colonne de synchronisation DateModif
        if (prop.Name == "Num" || 
            prop.Name == "Photo" || 
            prop.Name.Equals("DateModif", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        // Récupération et nettoyage des valeurs (gère le null, le vide et les espaces superflus)
        var avant = (prop.GetValue(existing)?.ToString() ?? "").Trim();
        var apres = (prop.GetValue(updatedPersonnel)?.ToString() ?? "").Trim();

        if (avant != apres)
        {
            // Traduction via le dictionnaire ou conservation du nom technique par défaut
            string nomClair = libellesChamps.TryGetValue(prop.Name, out var libelle) ? libelle : prop.Name;

            changements.Add(new { 
                champ = nomClair, 
                avant = avant, 
                apres = apres 
            });
        }
    }

    // Prise en compte manuelle de l'état de la photo de profil
    if (PhotoFile != null && PhotoFile.Length > 0)
    {
        changements.Add(new { champ = "Photo de profil", avant = "Ancienne", apres = "Nouvelle" });
    }

    // --- 4. ENREGISTREMENT ET FORMATAGE DE LA NOTIFICATION ---
    if (changements.Count > 0)
    {
        bool isAdmin = User.IsInRole("Admin");
        string nomUtilisateur = User.Identity?.Name ?? (isAdmin ? "Un administrateur" : "Un enseignant");
        string titreUser = isAdmin ? "L'administrateur" : "L'enseignant";

        // Construction du détail textuel des modifications
        string detailsTexte = string.Join(", ", changements.Select(c => {
            var dyn = (dynamic)c;
            if (dyn.champ == "Photo de profil")
            {
                return "Nouvelle photo de profil téléversée";
            }
            return $"{dyn.champ} : '{dyn.avant}' ➡️ '{dyn.apres}'";
        }));

        // 🔥 Anti-répétition : On vérifie si l'auteur connecté modifie sa propre fiche
        bool estSoiMeme = string.Equals(nomUtilisateur?.Trim(), existing.NomEtPrenoms?.Trim(), StringComparison.OrdinalIgnoreCase);
        string texteCible = estSoiMeme 
            ? "**sa propre fiche**" 
            : $"la fiche de **{existing.NomEtPrenoms ?? existing.Matricule}**";

        var notification = new Notification
        {
            Message = $"{titreUser} **{nomUtilisateur}** a mis à jour {texteCible} [{detailsTexte}].",
            DateCreation = DateTime.Now,
            IsRead = false,
            ModifiePar = nomUtilisateur
        };

        _context.Notifications.Add(notification);
    }

    // --- 5. ENREGISTREMENT FINAL EN BASE DE DONNÉES ---
    _context.Entry(existing).CurrentValues.SetValues(updatedPersonnel);

    try
    {
        await _context.SaveChangesAsync();
        return Ok(new { 
            success = true, 
            message = "Modification réussie", 
            photoUrl = updatedPersonnel.Photo 
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, message = ex.Message });
    }
}
        // =================================================================
        // 4. SUPPRESSION ET ARCHIVAGE AUTOMATIQUE
        // =================================================================
        [HttpPost]
        public async Task<IActionResult> DeletePersonnel(int id)
        {
            var personnel = await _context.Personnels.FindAsync(id);
            if (personnel == null) 
                return Json(new { success = false, message = "Personnel non trouvé." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var archive = new Base1();
                _context.Entry(archive).CurrentValues.SetValues(personnel);
                _context.Base1s.Add(archive);

                _context.Personnels.Remove(personnel);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Archivé avec succès." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message = ex.InnerException.Message;
                }
                return Json(new { success = false, message = "Erreur SQL : " + message });
            }
        }

        // =================================================================
        // 5. UPLOAD DE PHOTO ISOLÉ
        // =================================================================
        [HttpPost]
        public async Task<IActionResult> UploadPhoto(int num, IFormFile photoFile)
        {
            if (photoFile == null || photoFile.Length == 0) 
                return BadRequest(new { success = false, message = "Aucun fichier." });

            string filePath = await UploadPersonnelPhoto(num, photoFile);
            
            if (string.IsNullOrEmpty(filePath))
                return StatusCode(500, new { success = false, message = "Erreur lors de l'enregistrement." });

            var p = await _context.Personnels.FindAsync(num);
            if (p != null)
            {
                p.Photo = filePath;
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Photo mise à jour." });
            }

            return NotFound(new { success = false, message = "Personnel introuvable." });
        }

        private async Task<string> UploadPersonnelPhoto(int num, IFormFile photoFile)
        {
            try
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = $"{num}_{Guid.NewGuid()}{Path.GetExtension(photoFile.FileName)}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }

                return $"/uploads/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur upload fichier : {ex.Message}");
                return null;
            }
        }

        // =================================================================
        // 6. AUTO-COMPLÉTION BARRE DE RECHERCHE (JSON)
        // =================================================================
        [HttpGet]
        public IActionResult SearchPersonnel(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return Json(new List<object>());

            string term = search.ToLower();

            var resultats = _context.Personnels
                .AsNoTracking() // Gain de performance pour l'auto-complétion
                .Where(p => p.NomEtPrenoms.ToLower().Contains(term) || 
                            p.Matricule.ToLower().Contains(term))
                .Take(10)
                .Select(p => new {
                    nom = p.NomEtPrenoms,
                    matricule = p.Matricule
                })
                .ToList();

            return Json(resultats);
        }

        // =================================================================
        // 7. RECHERCHE ASYNCHRONE DYNAMIQUE (RENVOIE DU JSON DIRECT)
        // =================================================================
        [HttpGet]
        [Authorize] 
        public IActionResult SearchResultsPartial(string searchTerm)
        {
            if (!User.IsInRole("Admin"))
            {
                var userCin = User.FindFirst("UserCin")?.Value;

                if (string.IsNullOrEmpty(userCin))
                {
                    return Challenge(); 
                }

                var monRenseignement = _context.Personnels
                    .AsNoTracking()
                    .Where(p => p.Cin == userCin)
                    .ToList();

                return Json(monRenseignement);
            }

            var queryable = _context.Personnels.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                queryable = queryable.Where(p => p.NomEtPrenoms.Contains(searchTerm) || p.Matricule.Contains(searchTerm));
            }

            var model = queryable.ToList();
            return Json(model);
        }

        // =================================================================
        // 8. VUE DE L'ARCHIVE (Base1) - Optimisé AsNoTracking
        // =================================================================
        public IActionResult Archive()
        {
            var archives = _context.Base1s.AsNoTracking().ToList();
            return View(archives);
        }

        // =================================================================
        // 9. APIS : TEMPS RÉEL OPTIMISÉES (DASHBOARD & LIVE ATTENDANCE) 🚀
        // =================================================================

        /// <summary>
        /// 👑 NOUVELLE MÉTHODE UNIQUE ET ULTRA-LÉGÈRE POUR LE SERVEUR
        /// Regroupe les notifications de la DB et les profs connectés en mémoire vive en une seule requête HTTP.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLiveDashboardData()
        {
            var limiteInactivite = DateTime.Now.AddSeconds(-90);
            var currentUserName = User.Identity?.Name;

            // 1. Un seul accès ciblé à la base de données (optimisé AsNoTracking)
            var notifications = await _context.Notifications
                                              .AsNoTracking()
                                              .Where(n => !n.IsRead)
                                              .OrderByDescending(n => n.DateCreation)
                                              .ToListAsync();

            // 2. Lecture ultra-rapide depuis la mémoire RAM (sans toucher à SQL)
            var connectes = UtilisateursEnLigne
                .Where(u => u.Value >= limiteInactivite)
                .Select(u => u.Key)
                .Where(name => name != currentUserName) 
                .ToList();

            // Renvoie le pack de données groupé
            return Json(new {
                notifications = notifications,
                teachers = connectes
            });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Heartbeat()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                UtilisateursEnLigne[User.Identity.Name] = DateTime.Now;
                return Ok();
            }
            return BadRequest();
        }

        // Conservé pour la rétrocompatibilité (au cas où d'autres composants l'utilisent en dehors du layout)
        [HttpGet]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var notifications = await _context.Notifications
                                              .AsNoTracking()
                                              .Where(n => !n.IsRead)
                                              .OrderByDescending(n => n.DateCreation)
                                              .ToListAsync();
            return Json(notifications);
        }

        // Conservé pour la rétrocompatibilité
        [HttpGet]
        public IActionResult GetConnectedTeachers()
        {
            var limiteInactivite = DateTime.Now.AddSeconds(-90);
            var currentUserName = User.Identity?.Name;

            var connectes = UtilisateursEnLigne
                .Where(u => u.Value >= limiteInactivite)
                .Select(u => u.Key)
                .Where(name => name != currentUserName) 
                .ToList();

            return Json(connectes);
        }
    }
}