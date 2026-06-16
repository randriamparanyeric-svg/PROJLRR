using System;
using System.Collections.Generic;

namespace PROJLRR.Models;

public partial class Article
{
    public int? Id { get; set; }

    public string? Nom { get; set; }

    public int? Quantite { get; set; }

    public string? Unite { get; set; }

    public int? StockSec { get; set; }

    // 📅 ─── DERNIÈRE MODIFICATION (Last Write Time) ───
    // Cette propriété sera interceptée automatiquement par SaveChangesAsync()
    public DateTime? DateModif { get; set; }
}