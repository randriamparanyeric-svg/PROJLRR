using System;
using System.Collections.Generic;

namespace PROJLRR.Models
{
    public class DechargeArticle
    {
        // Ajoutez cette ligne pour corriger l'erreur CS0117
    public int Id { get; set; }
        public string ArticleNom { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public string Unite { get; set; } = string.Empty;

        // 🟢 AJOUTÉ : Permet de savoir si la ligne a été modifiée côté JavaScript
        public bool IsModified { get; set; }
    }
}