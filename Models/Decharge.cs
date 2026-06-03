using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 👈 Assurez-vous d'avoir ceci

namespace PROJLRR.Models;

public partial class Decharge
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] 
    public int Id { get; set; }

    public string? PersonnelNom { get; set; }
    public string? ArticleNom { get; set; }
    public int? Quantite { get; set; }
    public string? Unite { get; set; }
    
    // Le champ brut en base de données (qui contient un mélange de Ticks et de Millisecondes)
    public long? DateDecharge { get; set; }

    public string? SignaturePath { get; set; }

    // 🔥 Propriété magique pour l'affichage (Non stockée en BDD)
    [NotMapped]
    public DateTime? DateAffichage
    {
        get
        {
            if (!DateDecharge.HasValue || DateDecharge == 0) return null;

            // Si le nombre est gigantesque (18 chiffres), c'est le nouveau format Ticks
            if (DateDecharge > 600000000000000000)
            {
                return new DateTime(DateDecharge.Value);
            }
            
            // Sinon, c'est l'ancien format en Millisecondes (Unix Timestamp)
            return DateTimeOffset.FromUnixTimeMilliseconds(DateDecharge.Value).DateTime.ToLocalTime();
        }
    }
}