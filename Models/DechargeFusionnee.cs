using System;
using System.Collections.Generic;
namespace PROJLRR.Models
{
public class DechargeFusionnee
  {
    public string PersonnelNom { get; set; }
    public DateTime DateDecharge { get; set; }
    public string ArticlesFusionnes { get; set; } // Ex: "Stylo(10 pièce) + RAM de papier(1 RAM)"
    public string MATIERE { get; set; }
    public string MATRICULE { get; set; }
     public string? SignaturePath { get; set; }

}
}