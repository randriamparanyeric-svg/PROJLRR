using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJLRR.Models; 

namespace PROJLRR.Controllers
{
    public class PersonnelsController : Controller
    {
        private readonly PerslrrsanscodeContext _context;

        public PersonnelsController(PerslrrsanscodeContext context) => _context = context;

        // 1. Liste Globale
        public async Task<IActionResult> Index() => 
            View(await _context.Personnels.OrderBy(p => p.Matiere).ToListAsync());

        // 2. Création et Édition
public IActionResult Create() 
{
    // On passe une nouvelle instance vide à la vue pour éviter le null
    return View(new Personnel()); 
}
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Personnel personnel)
{
    // 1. Vérification du Matricule (IM)
    var existingMatricule = await _context.Personnels
        .FirstOrDefaultAsync(p => p.Matricule == personnel.Matricule);

    if (existingMatricule != null)
    {
        TempData["DuplicateError"] = $"Le matricule <b>{personnel.Matricule}</b> est déjà utilisé par <b>{existingMatricule.NomEtPrenoms}</b>.";
        return View(personnel);
    }

    // 2. Vérification du CIN
    var existingCin = await _context.Personnels
        .FirstOrDefaultAsync(p => p.Cin == personnel.Cin);

    if (existingCin != null)
    {
        TempData["DuplicateError"] = $"Le numéro de CIN <b>{personnel.Cin}</b> est déjà utilisé par <b>{existingCin.NomEtPrenoms}</b>.";
        return View(personnel);
    }

    // 3. Calcul du prochain ID si tout est conforme
    int nextId = _context.Personnels.Any() ? _context.Personnels.Max(p => p.Num) + 1 : 1;
    personnel.Num = nextId;

    if (ModelState.IsValid)
    {
        _context.Add(personnel);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Personnel ajouté avec succès !";
        return RedirectToAction(nameof(Index));
    }
    
    return View(personnel);
}
        public async Task<IActionResult> Delete(int? id) => View(await _context.Personnels.FindAsync(id));
        
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id) 
        { 
            var p = await _context.Personnels.FindAsync(id); 
            if (p != null) { _context.Personnels.Remove(p); await _context.SaveChangesAsync(); } 
            return RedirectToAction(nameof(Index)); 
        }

        // 3. VUES SPÉCIFIQUES
        
        public async Task<IActionResult> IndexEFA() 
        {
            var list = await _context.Personnels
                .Where(p => (p.Statut != null && p.Statut.ToUpper() == "EFA") || (p.Fonction != null && p.Fonction.ToUpper() == "PA"))
                .OrderBy(p => p.Matiere)
                .ToListAsync();
            return View(list); 
        }

        public async Task<IActionResult> IndexPE() => 
            View(await _context.Personnels.Where(p => p.Fonction == "PE").OrderBy(p => p.Matiere).ToListAsync());

        public async Task<IActionResult> RETRAITE() 
        { 
            int anneeActuelle = DateTime.Now.Year;
            var list = await _context.Personnels.ToListAsync();
            var retraitables = list.Where(p => !string.IsNullOrEmpty(p.Datenaiss) && 
                               DateTime.TryParse(p.Datenaiss, out var d) && 
                               (anneeActuelle - d.Year) >= 60).ToList();
            return View(retraitables); 
        }

        public async Task<IActionResult> STAT() => View(await _context.Personnels.ToListAsync());

        public async Task<IActionResult> STAT2() => View(await _context.Personnels.ToListAsync());

        public async Task<IActionResult> TableauPersonnels() => 
            View(await _context.Personnels.OrderBy(p => p.NomEtPrenoms).ToListAsync());
    }
}