using System;
using System.Collections.Generic;

namespace PROJLRR.Models
{
    public class MultiDechargeViewModel
    {
        // 🟢 AJOUTÉ : Indispensable pour retrouver la ligne en base de données lors de la modification
        public int Id { get; set; }

        public string PersonnelNom { get; set; }
        public DateTime DateDecharge { get; set; } = DateTime.Now;

        public string? Matricule { get; set; }

        public string? SignatureData { get; set; }  // en base64
        public string? SignaturePath { get; set; }  // chemin enregistré sur disque
        public long? GroupeId { get; set; }

        // Utilise directement ton type d'objet DechargeArticle
        public List<DechargeArticle> Articles { get; set; } = new List<DechargeArticle>();
    }
}