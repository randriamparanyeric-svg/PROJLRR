using System;
using System.Collections.Generic;

namespace PROJLRR.Models;

public partial class Decharge
{
    public int Id { get; set; }

    public string? PersonnelNom { get; set; }

    public string? ArticleNom { get; set; }

    public int? Quantite { get; set; }

    public string? Unite { get; set; }

    public long? DateDecharge { get; set; }

    public string? SignaturePath { get; set; }
}
