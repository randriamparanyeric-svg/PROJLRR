using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
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
        // On prépare l'ajout du personnel
        _context.Add(personnel);

        // =================================================================
        // 🔔 NOUVEAU : CRÉATION DE LA NOTIFICATION ENTRE ADMINS
        // =================================================================
        string nomAdmin = User.Identity?.Name ?? "Un administrateur";
        
        var notification = new Notification
        {
            // Message formaté (le Layout transformera les ** en gras <b>)
            Message = $"L'administrateur **{nomAdmin}** a créé la fiche du nouveau personnel : **{personnel.NomEtPrenoms ?? personnel.Matricule}**.",
            DateCreation = DateTime.Now,
            IsRead = false, // Lu = non, pour qu'il apparaisse dans la cloche
            ModifiePar = nomAdmin
        };
        
        // On prépare l'ajout de la notification
        _context.Notifications.Add(notification);
        // =================================================================

        // On valide le tout en une seule transaction SQL 🚀
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Personnel ajouté avec succès !";
        return RedirectToAction(nameof(Index));
    }

    return View(personnel);
}
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

            // Récupération de la liste depuis SQLite
            var list = await _context.Personnels.ToListAsync();

            // Formats de date autorisés
            string[] formatsAutorises = { "dd/MM/yyyy", "yyyy-MM-dd", "d/M/yyyy", "yyyy-M-d" };

            var retraitables = list.Where(p => !string.IsNullOrEmpty(p.Datenaiss) &&
                                               DateTime.TryParseExact(p.Datenaiss,
                                                                      formatsAutorises,
                                                                      System.Globalization.CultureInfo.InvariantCulture, // 🎯 Chemin complet
                                                                      System.Globalization.DateTimeStyles.None,          // 🎯 Chemin complet
                                                                      out var d) &&
                                               (anneeActuelle - d.Year) >= 60)
                                   .ToList();

            return View(retraitables);
        }

        public async Task<IActionResult> STAT() => View(await _context.Personnels.ToListAsync());

        public async Task<IActionResult> STAT2() => View(await _context.Personnels.ToListAsync());

        public async Task<IActionResult> TableauPersonnels() =>
            View(await _context.Personnels.OrderBy(p => p.NomEtPrenoms).ToListAsync());
            
            [HttpGet]
public async Task<IActionResult> ExporterExcel()
{
    // 1. Récupération des données depuis la base de données
  var personnelsDb = await _context.Personnels
        .AsNoTracking()
        .OrderBy(p => p.Matricule) // <--- ICI : Trie du plus petit au plus grand matricule
        .ToListAsync();

    // 2. Construction de la liste avec l'indexation automatique
    var donneesExport = new List<object>();
    int compteur = 1;

    foreach (var p in personnelsDb)
    {
        donneesExport.Add(new
        {
            Num = compteur++, // Assure la suite séquentielle 1, 2, 3...
            Matricule = p.Matricule,
            NomEtPrenoms = p.NomEtPrenoms,
            Cin = p.Cin,
            Dec = p.Dec,
            Corps = p.Corps,
            Matiere = p.Matiere,
            Datenaiss = p.Datenaiss,
            Lieudenaiss = p.Lieudenaiss,
            Sexe = p.Sexe,
            Statut = p.Statut,
            Datedentre = p.Datedentre,
            Datedeprise = p.Datedeprise,
            Diplomeac = p.Diplomeac,
            Diplomeped = p.Diplomeped,
            Contact = p.Contact,
            Perav = p.Perav, // Évaluation Trimestre 1
            Demav = p.Demav, // Évaluation Trimestre 2
            Temav = p.Temav, // Évaluation Trimestre 3
            Qemav = p.Qemav, // Évaluation Trimestre 4
            Cemav = p.Cemav,
            Semav = p.Semav,
            Sepmav = p.Sepmav,
            Hemav = p.Hemav,
            Nemav = p.Nemav,
            Dxemav = p.Dxemav,
            Onemav = p.Onemav,
            Dou = p.Dou,
            Trei = p.Trei,
            Quat = p.Quat,
            Quin = p.Quin,
            Seiz = p.Seiz
        });
    }

    // 3. Écriture du flux de données en mémoire
    var memoryStream = new MemoryStream();
    await memoryStream.SaveAsAsync(donneesExport);
    memoryStream.Position = 0;

    // 4. Configuration du téléchargement de fichier
    string nomFichier = $"Fiche_Personnels_{System.DateTime.Now.ToString("yyyyMMdd")}.xlsx";
    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    return File(memoryStream, contentType, nomFichier);
}
    }
}