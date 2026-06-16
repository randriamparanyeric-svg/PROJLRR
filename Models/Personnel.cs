using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Ajouté pour l'attribut Table

namespace PROJLRR.Models;

[Table("BASE")] // Indique à EF Core que ce modèle correspond à la table BASE de SQLite
public partial class Personnel
{
    [Key] 
    public int Num { get; set; }

    public string? Matricule { get; set; }

    public string? NomEtPrenoms { get; set; }

    public string? Cin { get; set; }

    public string? Dec { get; set; }

    public string? Corps { get; set; }

    public string? Matiere { get; set; }

    public string? Datenaiss { get; set; }

    public string? Lieudenaiss { get; set; }

    public string? Sexe { get; set; }

    public string? Statut { get; set; }

    public string? Datedentre { get; set; }

    public string? Datedeprise { get; set; }

    public string? Diplomeac { get; set; }

    public string? Diplomeped { get; set; }

    public string? Contact { get; set; }

    public string? Perav { get; set; }

    public string? Demav { get; set; }

    public string? Temav { get; set; }

    public string? Qemav { get; set; }

    public string? Cemav { get; set; }

    public string? Semav { get; set; }

    public string? Sepmav { get; set; }

    public string? Hemav { get; set; }

    public string? Nemav { get; set; }

    public string? Dxemav { get; set; }

    public string? Onemav { get; set; }

    public string? Dou { get; set; }

    public string? Trei { get; set; }

    public string? Quat { get; set; }

    public string? Quin { get; set; }

    public string? Seiz { get; set; }

    public string? Fonction { get; set; }

    public string? Photo { get; set; }

    // ==========================================================================
    // COLONNE DE SYNCHRONISATION : Capture la date et l'heure de modification
    // ==========================================================================
    public DateTime? DateModif { get; set; }
}