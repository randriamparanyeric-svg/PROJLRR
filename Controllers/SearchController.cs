using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJLRR.Models;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System;

namespace PROJLRR.Controllers
{
    public class SearchController : Controller
    {
        private readonly PerslrrsanscodeContext _context;

        public SearchController(PerslrrsanscodeContext context)
        {
            _context = context;
        }

        // 1. Recherche
       [HttpGet]
        public IActionResult SearchResults(string searchTerm = "")
        {

            // Correction : Utilisation du singulier 'Personnel' pour la liste
            List<Personnel> results = new List<Personnel>();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                
                // Correction : Ici on utilise le nom de la propriété définie dans le DbContext
                // (Si vous avez une erreur ici, vérifiez le nom dans PerslrrsanscodeContext.cs)
                results = _context.Personnels
                    .Where(p => (p.NomEtPrenoms != null && p.NomEtPrenoms.ToLower().Contains(term)) || 
                                (p.Matricule != null && p.Matricule.ToLower().Contains(term)))
                    .ToList(); 
            }

            ViewBag.SearchTerm = searchTerm;
            return View(results);
        }
        // 2. Détail (Json)
        [HttpGet]
        public JsonResult GetPersonnelDetails(int id)
        {
            var personnel = _context.Personnels.FirstOrDefault(p => p.Num == id);
            return Json(personnel);
        }

        // 3. Mise à jour
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
        if (parts.Length == 3)
        {
            updatedPersonnel.Datenaiss = $"{parts[2]}/{parts[1]}/{parts[0]}";
        }
    }

    if (!string.IsNullOrEmpty(updatedPersonnel.Datedentre) && updatedPersonnel.Datedentre.Contains("-"))
    {
        var parts = updatedPersonnel.Datedentre.Split('-');
        if (parts.Length == 3)
        {
            updatedPersonnel.Datedentre = $"{parts[2]}/{parts[1]}/{parts[0]}";
        }
    }

    if (!string.IsNullOrEmpty(updatedPersonnel.Datedeprise) && updatedPersonnel.Datedeprise.Contains("-"))
    {
        var parts = updatedPersonnel.Datedeprise.Split('-');
        if (parts.Length == 3)
        {
            updatedPersonnel.Datedeprise = $"{parts[2]}/{parts[1]}/{parts[0]}";
        }
    }

    // Mise à jour automatique de toutes les propriétés converties
    _context.Entry(existing).CurrentValues.SetValues(updatedPersonnel);

    try
    {
        await _context.SaveChangesAsync();
        
        // CORRECTION ICI : On renvoie "photoUrl" au JavaScript
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
       [HttpPost]
public async Task<IActionResult> DeletePersonnel(int id) // Pas besoin de [FromBody] ici
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
    
    // Ajout de ceci pour voir l'erreur réelle dans la console
    var message = ex.Message;
    if (ex.InnerException != null)
    {
        message = ex.InnerException.Message;
    }
    
    return Json(new { success = false, message = "Erreur SQL : " + message });
}
}
        // 5. Upload Photo
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

        // --- HELPER D'UPLOAD ---
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
        // Vérifiez bien que le nom est SearchPersonnel et qu'il est public
[HttpGet]
public IActionResult SearchPersonnel(string search)
{
    // Sécurité : si search est null, on retourne une liste vide
    if (string.IsNullOrWhiteSpace(search)) return Json(new List<object>());

    // On convertit le terme cherché en minuscules
    string term = search.ToLower();

    var resultats = _context.Personnels
        // On convertit chaque donnée de la base en minuscules avant de comparer
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
[HttpGet]
public IActionResult SearchResultsPartial(string searchTerm)
{
    var model = _context.Personnels
        .Where(p => p.NomEtPrenoms.Contains(searchTerm) || p.Matricule.Contains(searchTerm))
        .ToList();

    // On retourne une "PartialView" (vous devez créer le fichier _PersonnelList.cshtml)
    return PartialView("_PersonnelList", model);
}
public IActionResult Archive()
{
    // Récupère tous les éléments de la table d'archives
    var archives = _context.Base1s.ToList();
    return View(archives);
}
    }
}