using System;
using System.Collections.Generic;

namespace PROJLRR.Models
{
    public class DechargeFusionnee
    {
        public int Id { get; set; }
        public string PersonnelNom { get; set; } = string.Empty;
        public DateTime DateDecharge { get; set; }
        public string ArticlesFusionnes { get; set; } = string.Empty;
        public string MATIERE { get; set; } = string.Empty;
        public string MATRICULE { get; set; } = string.Empty;
        public string? SignaturePath { get; set; }

        // 🟢 AJOUTÉ : Permet de pousser/afficher la dernière date de modification du groupe
        public DateTime DateModif { get; set; }
    }
}