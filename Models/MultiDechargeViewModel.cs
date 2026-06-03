using System;
using System.Collections.Generic;
namespace PROJLRR.Models
{
       public class MultiDechargeViewModel
    {
        public string PersonnelNom { get; set; }
        public DateTime DateDecharge { get; set; } = DateTime.Now;

        public string? SignatureData { get; set; }  // en base64
        public string? SignaturePath { get; set; }  // chemin enregistré sur disque

        public List<DechargeArticle> Articles { get; set; } = new List<DechargeArticle>();
    }

}