using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJLRR.Models;

public partial class Decharge
{
   [Key]
[DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Remplacez None par Identity
public int Id { get; set; }

    public string? PersonnelNom { get; set; }
    public string? ArticleNom { get; set; }
    public int? Quantite { get; set; }
    public string? Unite { get; set; }
    
    // Le champ brut en base de données (qui contient un mélange de Ticks et de Millisecondes)
    public long? DateDecharge { get; set; }

    public string? SignaturePath { get; set; }

    // 🟢 AJOUTÉ : Traçabilité pour la synchronisation Last Write Wins
    public DateTime? DateModif { get; set; } = DateTime.Now;
    public long? GroupeId { get; set; }

    // 🟢 AJOUTÉ : Indicateur d'état pour la validation sélective à l'écran (Ignoré par la BDD)
    [NotMapped]
    public bool IsModified { get; set; } = false;

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