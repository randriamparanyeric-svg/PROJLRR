using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJLRR.Models;

namespace PROJLRR.Controllers
{
    public class ArticleController : Controller
    {
        private readonly PerslrrsanscodeContext _dbContext;

        public ArticleController(PerslrrsanscodeContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("Article/Index")]
        public IActionResult Index()
        {
            // 🔥 CORRECTION : On utilise (int?) au lieu de (long?) pour correspondre à ton modèle
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

        // 🔹 Formulaire d'ajout d'un article (Route corrigée)
        [HttpGet("Article/AddArticle")]
        public IActionResult AddArticle()
        {
            return View(new Article());
        }

        // 🔹 Ajouter un article (Route corrigée)
        [HttpPost("Article/AddArticle")]
        public IActionResult AddArticle(Article article)
        {
            if (!ModelState.IsValid)
            {
                return View(article);
            }

            try
            {
                // 🔥 CORRECTION : prochainId passe en 'int' au lieu de 'long'
                int prochainId = (_dbContext.Articles.Select(a => (int?)a.Id).Max() ?? 0) + 1;

                var newArticle = new Article
                {
                    Id = prochainId, // Plus d'erreur de conversion ici !
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
    }
}