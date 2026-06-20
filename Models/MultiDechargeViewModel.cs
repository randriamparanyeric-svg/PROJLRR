using System;
using System.Collections.Generic;

namespace PROJLRR.Models
{
    public class MultiDechargeViewModel
    {
        public string PersonnelNom { get; set; }
        public DateTime DateDecharge { get; set; } = DateTime.Now;

        // 🔥 LIGNE AJOUTÉE : Permet de lier le matricule saisi à ton contrôleur et ta vue
        public string? Matricule { get; set; }

        public string? SignatureData { get; set; }  // en base64
        public string? SignaturePath { get; set; }  // chemin enregistré sur disque

        public List<DechargeArticle> Articles { get; set; } = new List<DechargeArticle>();
    }
}