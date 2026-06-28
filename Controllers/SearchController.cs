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

    // --- GESTION DE LA PHOTO ---
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

    // --- ENREGISTREMENT DES DATES AU FORMAT FR (JJ/MM/AAAA) ---
    if (!string.IsNullOrEmpty(updatedPersonnel.Datenaiss) && updatedPersonnel.Datenaiss.Contains("-"))
    {
        var parts = updatedPersonnel.Datenaiss.Split('-');
        if (parts.Length == 3) updatedPersonnel.Datenaiss = $"{parts[2]}/{parts[1]}/{parts[0]}";
    }

    if (!string.IsNullOrEmpty(updatedPersonnel.Datedentre) && updatedPersonnel.Datedentre.Contains("-"))
    {
        var parts = updatedPersonnel.Datedentre.Split('-');
        if (parts.Length == 3) updatedPersonnel.Datedentre = $"{parts[2]}/{parts[1]}/{parts[0]}";
    }

    if (!string.IsNullOrEmpty(updatedPersonnel.Datedeprise) && updatedPersonnel.Datedeprise.Contains("-"))
    {
        var parts = updatedPersonnel.Datedeprise.Split('-');
        if (parts.Length == 3) updatedPersonnel.Datedeprise = $"{parts[2]}/{parts[1]}/{parts[0]}";
    }

   // --- 🔔 DÉTECTION DE TOUS LES ÉCARTS (POUR ENSEIGNANTS ET ADMINS) ---
bool isAdmin = User.IsInRole("Admin");
List<string> changements = new List<string>();

// ⚡ FONCTION LOCALE : Compare proprement deux valeurs sans faux positifs (gère null, vide et espaces)
bool EstModifie(object avant, object apres)
{
    string strAvant = (avant?.ToString() ?? "").Trim();
    string strApres = (apres?.ToString() ?? "").Trim();
    return strAvant != strApres;
}

// 1. Informations Générales
if (EstModifie(existing.Matricule, updatedPersonnel.Matricule)) changements.Add($"Matricule : '{existing.Matricule}' ➡️ '{updatedPersonnel.Matricule}'");
if (EstModifie(existing.NomEtPrenoms, updatedPersonnel.NomEtPrenoms)) changements.Add($"Nom & Prénoms : '{existing.NomEtPrenoms}' ➡️ '{updatedPersonnel.NomEtPrenoms}'");
if (EstModifie(existing.Cin, updatedPersonnel.Cin)) changements.Add($"CIN : '{existing.Cin}' ➡️ '{updatedPersonnel.Cin}'");
if (EstModifie(existing.Dec, updatedPersonnel.Dec)) changements.Add($"Dec : '{existing.Dec}' ➡️ '{updatedPersonnel.Dec}'");
if (EstModifie(existing.Corps, updatedPersonnel.Corps)) changements.Add($"Corps : '{existing.Corps}' ➡️ '{updatedPersonnel.Corps}'");
if (EstModifie(existing.Matiere, updatedPersonnel.Matiere)) changements.Add($"Matière : '{existing.Matiere}' ➡️ '{updatedPersonnel.Matiere}'");
if (EstModifie(existing.Datenaiss, updatedPersonnel.Datenaiss)) changements.Add($"Date Naiss : '{existing.Datenaiss}' ➡️ '{updatedPersonnel.Datenaiss}'");
if (EstModifie(existing.Lieudenaiss, updatedPersonnel.Lieudenaiss)) changements.Add($"Lieu Naiss : '{existing.Lieudenaiss}' ➡️ '{updatedPersonnel.Lieudenaiss}'");
if (EstModifie(existing.Sexe, updatedPersonnel.Sexe)) changements.Add($"Sexe : '{existing.Sexe}' ➡️ '{updatedPersonnel.Sexe}'");
if (EstModifie(existing.Statut, updatedPersonnel.Statut)) changements.Add($"Statut : '{existing.Statut}' ➡️ '{updatedPersonnel.Statut}'");
if (EstModifie(existing.Datedentre, updatedPersonnel.Datedentre)) changements.Add($"Date Entrée : '{existing.Datedentre}' ➡️ '{updatedPersonnel.Datedentre}'");
if (EstModifie(existing.Datedeprise, updatedPersonnel.Datedeprise)) changements.Add($"Date Prise : '{existing.Datedeprise}' ➡️ '{updatedPersonnel.Datedeprise}'");
if (EstModifie(existing.Diplomeac, updatedPersonnel.Diplomeac)) changements.Add($"Diplôme Ac. : '{existing.Diplomeac}' ➡️ '{updatedPersonnel.Diplomeac}'");
if (EstModifie(existing.Diplomeped, updatedPersonnel.Diplomeped)) changements.Add($"Diplôme Péd. : '{existing.Diplomeped}' ➡️ '{updatedPersonnel.Diplomeped}'");
if (EstModifie(existing.Contact, updatedPersonnel.Contact)) changements.Add($"Contact : '{existing.Contact}' ➡️ '{updatedPersonnel.Contact}'");
if (EstModifie(existing.Fonction, updatedPersonnel.Fonction)) changements.Add($"Fonction : '{existing.Fonction}' ➡️ '{updatedPersonnel.Fonction}'");

// 🌟 Suivi Grade et Série Bacc
if (EstModifie(existing.Grade, updatedPersonnel.Grade)) changements.Add($"Grade : '{existing.Grade}' ➡️ '{updatedPersonnel.Grade}'");
if (EstModifie(existing.SerieBacc, updatedPersonnel.SerieBacc)) changements.Add($"Série Bacc : '{existing.SerieBacc}' ➡️ '{updatedPersonnel.SerieBacc}'");

// --- Historique des Avancements ---
if (EstModifie(existing.Perav, updatedPersonnel.Perav)) changements.Add($"Perav : '{existing.Perav}' ➡️ '{updatedPersonnel.Perav}'");
if (EstModifie(existing.Demav, updatedPersonnel.Demav)) changements.Add($"Demav : '{existing.Demav}' ➡️ '{updatedPersonnel.Demav}'");
if (EstModifie(existing.Temav, updatedPersonnel.Temav)) changements.Add($"Temav : '{existing.Temav}' ➡️ '{updatedPersonnel.Temav}'");
if (EstModifie(existing.Qemav, updatedPersonnel.Qemav)) changements.Add($"Qemav : '{existing.Qemav}' ➡️ '{updatedPersonnel.Qemav}'");
if (EstModifie(existing.Cemav, updatedPersonnel.Cemav)) changements.Add($"Cemav : '{existing.Cemav}' ➡️ '{updatedPersonnel.Cemav}'");
if (EstModifie(existing.Semav, updatedPersonnel.Semav)) changements.Add($"Semav : '{existing.Semav}' ➡️ '{updatedPersonnel.Semav}'");
if (EstModifie(existing.Sepmav, updatedPersonnel.Sepmav)) changements.Add($"Sepmav : '{existing.Sepmav}' ➡️ '{updatedPersonnel.Sepmav}'");
if (EstModifie(existing.Hemav, updatedPersonnel.Hemav)) changements.Add($"Hemav : '{existing.Hemav}' ➡️ '{updatedPersonnel.Hemav}'");
if (EstModifie(existing.Nemav, updatedPersonnel.Nemav)) changements.Add($"Nemav : '{existing.Nemav}' ➡️ '{updatedPersonnel.Nemav}'");
if (EstModifie(existing.Dxemav, updatedPersonnel.Dxemav)) changements.Add($"Dxemav : '{existing.Dxemav}' ➡️ '{updatedPersonnel.Dxemav}'");
if (EstModifie(existing.Onemav, updatedPersonnel.Onemav)) changements.Add($"Onemav : '{existing.Onemav}' ➡️ '{updatedPersonnel.Onemav}'");
if (EstModifie(existing.Dou, updatedPersonnel.Dou)) changements.Add($"Dou : '{existing.Dou}' ➡️ '{updatedPersonnel.Dou}'");
if (EstModifie(existing.Trei, updatedPersonnel.Trei)) changements.Add($"Trei : '{existing.Trei}' ➡️ '{updatedPersonnel.Trei}'");
if (EstModifie(existing.Quat, updatedPersonnel.Quat)) changements.Add($"Quat : '{existing.Quat}' ➡️ '{updatedPersonnel.Quat}'");
if (EstModifie(existing.Quin, updatedPersonnel.Quin)) changements.Add($"Quin : '{existing.Quin}' ➡️ '{updatedPersonnel.Quin}'");
if (EstModifie(existing.Seiz, updatedPersonnel.Seiz)) changements.Add($"Seiz : '{existing.Seiz}' ➡️ '{updatedPersonnel.Seiz}'");

// 🌟 Suivi des 8 Classes tenues
if (EstModifie(existing.ClasseTenue1, updatedPersonnel.ClasseTenue1)) changements.Add($"Classe 1 : '{existing.ClasseTenue1}' ➡️ '{updatedPersonnel.ClasseTenue1}'");
if (EstModifie(existing.ClasseTenue2, updatedPersonnel.ClasseTenue2)) changements.Add($"Classe 2 : '{existing.ClasseTenue2}' ➡️ '{updatedPersonnel.ClasseTenue2}'");
if (EstModifie(existing.ClasseTenue3, updatedPersonnel.ClasseTenue3)) changements.Add($"Classe 3 : '{existing.ClasseTenue3}' ➡️ '{updatedPersonnel.ClasseTenue3}'");
if (EstModifie(existing.ClasseTenue4, updatedPersonnel.ClasseTenue4)) changements.Add($"Classe 4 : '{existing.ClasseTenue4}' ➡️ '{updatedPersonnel.ClasseTenue4}'");
if (EstModifie(existing.ClasseTenue5, updatedPersonnel.ClasseTenue5)) changements.Add($"Classe 5 : '{existing.ClasseTenue5}' ➡️ '{updatedPersonnel.ClasseTenue5}'");
if (EstModifie(existing.ClasseTenue6, updatedPersonnel.ClasseTenue6)) changements.Add($"Classe 6 : '{existing.ClasseTenue6}' ➡️ '{updatedPersonnel.ClasseTenue6}'");
if (EstModifie(existing.ClasseTenue7, updatedPersonnel.ClasseTenue7)) changements.Add($"Classe 7 : '{existing.ClasseTenue7}' ➡️ '{updatedPersonnel.ClasseTenue7}'");
if (EstModifie(existing.ClasseTenue8, updatedPersonnel.ClasseTenue8)) changements.Add($"Classe 8 : '{existing.ClasseTenue8}' ➡️ '{updatedPersonnel.ClasseTenue8}'");

if (PhotoFile != null && PhotoFile.Length > 0)
    changements.Add("Nouvelle photo de profil téléversée");

// 🔔 Création de la notification uniquement s'il y a de réels changements alternatifs
if (changements.Count > 0)
{
    string nomUtilisateur = User.Identity?.Name ?? (isAdmin ? "Un administrateur" : "Un enseignant");
    string titreUser = isAdmin ? "L'administrateur" : "L'enseignant";
    string detailsTexte = string.Join(", ", changements);

    var notification = new Notification
    {
        Message = $"{titreUser} **{nomUtilisateur}** a modifié la fiche de **{existing.NomEtPrenoms ?? existing.Matricule}** [{detailsTexte}].",
        DateCreation = DateTime.Now,
        IsRead = false,
        ModifiePar = nomUtilisateur
    };

    _context.Notifications.Add(notification);
}

// Mise à jour automatique de toutes les propriétés converties
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