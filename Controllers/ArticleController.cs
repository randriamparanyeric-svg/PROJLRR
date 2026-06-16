using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // 💡 AJOUTÉ : Indispensable pour utiliser "Task" et l'asynchrone
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJLRR.Models;

namespace PROJLRR.Controllers
{
    public class ArticleController : Controller
    {
        // 💡 Ton champ privé s'appelle _dbContext
        private readonly PerslrrsanscodeContext _dbContext;

        public ArticleController(PerslrrsanscodeContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("Article/Index")]
        public IActionResult Index()
        {
            var articles = _dbContext.Articles
                .Select(a => new Article
                {
                    Id = ((int?)a.Id) ?? 0,
                    Nom = a.Nom ?? "Sans nom",
                    Quantite = ((int?)a.Quantite) ?? 0,
                    Unite = a.Unite ?? "PCE",
                    StockSec = ((int?)a.StockSec) ?? 0 
                })
                .ToList();

            return View(articles);
        }

        // 🔹 Formulaire d'ajout d'un article
        [HttpGet("Article/AddArticle")]
        public IActionResult AddArticle()
        {
            return View(new Article());
        }

        // 🔹 Ajouter un article (Traitement du formulaire)
        [HttpPost("Article/AddArticle")]
        public IActionResult AddArticle(Article article)
        {
            if (!ModelState.IsValid)
            {
                return View(article);
            }

            try
            {
                int prochainId = (_dbContext.Articles.Select(a => (int?)a.Id).Max() ?? 0) + 1;

                var newArticle = new Article
                {
                    Id = prochainId,
                    Nom = article.Nom,
                    Quantite = article.Quantite,
                    Unite = article.Unite,
                    StockSec = article.StockSec 
                };

                _dbContext.Articles.Add(newArticle);
                _dbContext.SaveChanges();

                TempData["Message"] = "Article ajouté avec succès !";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur d'ajout : {ex.Message}");
                ModelState.AddModelError("", "Une erreur est survenue lors de l'ajout de l'article.");
                return View(article);
            }
        }

        // 🏢 1. GET : Affiche le formulaire pré-rempli (Asynchrone)
        // 💡 Ajout de la route explicite pour correspondre à ton URL : /Article/ModifArticle/65
        [HttpGet("Article/ModifArticle/{id}")]
        public async Task<IActionResult> ModifArticle(int id)
        {
            // CORRECTION : _context est devenu _dbContext
            var article = await _dbContext.Articles.FirstOrDefaultAsync(a => a.Id == id);
            
            if (article == null)
            {
                return NotFound(); 
            }
            
            return View(article);
        }

        // 💾 2. POST : Réceptionne et enregistre les modifications (Asynchrone)
        [HttpPost("Article/ModifArticle/{id?}")]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> ModifArticle(Article article)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // CORRECTION : _context est devenu _dbContext
                    _dbContext.Articles.Update(article);
                    
                    await _dbContext.SaveChangesAsync();
                    TempData["Message"] = "Article modifié avec succès !";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    // CORRECTION : _context est devenu _dbContext
                    if (!await _dbContext.Articles.AnyAsync(a => a.Id == article.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            return View(article);
        }
    }
}