using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PROJLRR.Models;

public partial class PerslrrsanscodeContext : DbContext
{
    public PerslrrsanscodeContext()
    {
    }

    public PerslrrsanscodeContext(DbContextOptions<PerslrrsanscodeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Article> Articles { get; set; }
    public virtual DbSet<Personnel> Personnels { get; set; }
    public virtual DbSet<Base1> Base1s { get; set; }
    public virtual DbSet<Decharge> Decharges { get; set; }

 protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // On vérifie si le Program.cs n'a pas déjà configuré la base de données
    if (!optionsBuilder.IsConfigured)
    {
        // Ce chemin ne sera utilisé QU'EN LOCAL sur votre PC
        string dbPath = @"C:\Users\HP\PROJLRR\PERSLRRSANSCODE.db";
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ARTICLE");

            entity.Property(e => e.Nom).HasColumnType("VARCHAR(255)");
            entity.Property(e => e.Unite).HasColumnType("VARCHAR(255)");
        });

        // --- CORRECTION EFFECTUÉE ICI ---
        modelBuilder.Entity<Personnel>(entity =>
        {
            entity.HasKey(e => e.Num); // Définition de la Clé Primaire
            entity.Property(e => e.Num).ValueGeneratedOnAdd();
            entity.ToTable("BASE");

            entity.Property(e => e.Num).HasColumnName("Num"); // Assurez-vous que Num est mappé ici
            entity.Property(e => e.Cemav).HasColumnType("VARCHAR(255)").HasColumnName("CEMAV");
            entity.Property(e => e.Cin).HasColumnType("VARCHAR(255)").HasColumnName("CIN");
            entity.Property(e => e.Contact).HasColumnType("VARCHAR(255)").HasColumnName("CONTACT");
            entity.Property(e => e.Corps).HasColumnType("VARCHAR(255)").HasColumnName("CORPS");
            entity.Property(e => e.Datedentre).HasColumnType("VARCHAR(255)").HasColumnName("DATEDENTRE");
            entity.Property(e => e.Datedeprise).HasColumnType("VARCHAR(255)").HasColumnName("DATEDEPRISE");
            entity.Property(e => e.Datenaiss).HasColumnType("VARCHAR(255)").HasColumnName("DATENAISS");
            entity.Property(e => e.Dec).HasColumnType("VARCHAR(255)").HasColumnName("DEC");
            entity.Property(e => e.Demav).HasColumnType("VARCHAR(255)").HasColumnName("DEMAV");
            entity.Property(e => e.Diplomeac).HasColumnType("VARCHAR(255)").HasColumnName("DIPLOMEAC");
            entity.Property(e => e.Diplomeped).HasColumnType("VARCHAR(255)").HasColumnName("DIPLOMEPED");
            entity.Property(e => e.Dou).HasColumnType("VARCHAR(255)").HasColumnName("DOU");
            entity.Property(e => e.Dxemav).HasColumnType("VARCHAR(255)").HasColumnName("DXEMAV");
            entity.Property(e => e.Fonction).HasColumnType("VARCHAR(255)").HasColumnName("FONCTION");
            entity.Property(e => e.Hemav).HasColumnType("VARCHAR(255)").HasColumnName("HEMAV");
            entity.Property(e => e.Lieudenaiss).HasColumnType("VARCHAR(255)").HasColumnName("LIEUDENAISS");
            entity.Property(e => e.Matiere).HasColumnType("VARCHAR(255)").HasColumnName("MATIERE");
            entity.Property(e => e.Matricule).HasColumnType("VARCHAR(255)").HasColumnName("MATRICULE");
            entity.Property(e => e.Nemav).HasColumnType("VARCHAR(255)").HasColumnName("NEMAV");
            entity.Property(e => e.NomEtPrenoms).HasColumnType("VARCHAR(255)").HasColumnName("NOM_ET_PRENOMS");
            entity.Property(e => e.Onemav).HasColumnType("VARCHAR(255)").HasColumnName("ONEMAV");
            entity.Property(e => e.Perav).HasColumnType("VARCHAR(255)").HasColumnName("PERAV");
            entity.Property(e => e.Photo).HasColumnType("VARCHAR(255)");
            entity.Property(e => e.Qemav).HasColumnType("VARCHAR(255)").HasColumnName("QEMAV");
            entity.Property(e => e.Quat).HasColumnType("VARCHAR(255)").HasColumnName("QUAT");
            entity.Property(e => e.Quin).HasColumnType("VARCHAR(255)").HasColumnName("QUIN");
            entity.Property(e => e.Seiz).HasColumnType("VARCHAR(255)").HasColumnName("SEIZ");
            entity.Property(e => e.Semav).HasColumnType("VARCHAR(255)").HasColumnName("SEMAV");
            entity.Property(e => e.Sepmav).HasColumnType("VARCHAR(255)").HasColumnName("SEPMAV");
            entity.Property(e => e.Sexe).HasColumnType("VARCHAR(255)").HasColumnName("SEXE");
            entity.Property(e => e.Statut).HasColumnType("VARCHAR(255)").HasColumnName("STATUT");
            entity.Property(e => e.Temav).HasColumnType("VARCHAR(255)").HasColumnName("TEMAV");
            entity.Property(e => e.Trei).HasColumnType("VARCHAR(255)").HasColumnName("TREI");
        });

       modelBuilder.Entity<Base1>(entity =>
{
    entity.ToTable("BASE1");
    entity.HasKey(e => e.Num); // Déclarez la clé primaire ici
            entity.Property(e => e.Cemav).HasColumnType("VARCHAR(255)").HasColumnName("CEMAV");
            entity.Property(e => e.Cin).HasColumnType("VARCHAR(255)").HasColumnName("CIN");
            entity.Property(e => e.Contact).HasColumnType("VARCHAR(255)").HasColumnName("CONTACT");
            entity.Property(e => e.Corps).HasColumnType("VARCHAR(255)").HasColumnName("CORPS");
            entity.Property(e => e.Datedentre).HasColumnType("VARCHAR(255)").HasColumnName("DATEDENTRE");
            entity.Property(e => e.Datedeprise).HasColumnType("VARCHAR(255)").HasColumnName("DATEDEPRISE");
            entity.Property(e => e.Datenaiss).HasColumnType("VARCHAR(255)").HasColumnName("DATENAISS");
            entity.Property(e => e.Dec).HasColumnType("VARCHAR(255)").HasColumnName("DEC");
            entity.Property(e => e.Demav).HasColumnType("VARCHAR(255)").HasColumnName("DEMAV");
            entity.Property(e => e.Diplomeac).HasColumnType("VARCHAR(255)").HasColumnName("DIPLOMEAC");
            entity.Property(e => e.Diplomeped).HasColumnType("VARCHAR(255)").HasColumnName("DIPLOMEPED");
            entity.Property(e => e.Dou).HasColumnType("VARCHAR(255)").HasColumnName("DOU");
            entity.Property(e => e.Dxemav).HasColumnType("VARCHAR(255)").HasColumnName("DXEMAV");
            entity.Property(e => e.Fonction).HasColumnType("VARCHAR(255)").HasColumnName("FONCTION");
            entity.Property(e => e.Hemav).HasColumnType("VARCHAR(255)").HasColumnName("HEMAV");
            entity.Property(e => e.Lieudenaiss).HasColumnType("VARCHAR(255)").HasColumnName("LIEUDENAISS");
            entity.Property(e => e.Matiere).HasColumnType("VARCHAR(255)").HasColumnName("MATIERE");
            entity.Property(e => e.Matricule).HasColumnType("VARCHAR(255)").HasColumnName("MATRICULE");
            entity.Property(e => e.Nemav).HasColumnType("VARCHAR(255)").HasColumnName("NEMAV");
            entity.Property(e => e.NomEtPrenoms).HasColumnType("VARCHAR(255)").HasColumnName("NOM_ET_PRENOMS");
            entity.Property(e => e.Onemav).HasColumnType("VARCHAR(255)").HasColumnName("ONEMAV");
            entity.Property(e => e.Perav).HasColumnType("VARCHAR(255)").HasColumnName("PERAV");
            entity.Property(e => e.Photo).HasColumnType("VARCHAR(255)");
            entity.Property(e => e.Qemav).HasColumnType("VARCHAR(255)").HasColumnName("QEMAV");
            entity.Property(e => e.Quat).HasColumnType("VARCHAR(255)").HasColumnName("QUAT");
            entity.Property(e => e.Quin).HasColumnType("VARCHAR(255)").HasColumnName("QUIN");
            entity.Property(e => e.Seiz).HasColumnType("VARCHAR(255)").HasColumnName("SEIZ");
            entity.Property(e => e.Semav).HasColumnType("VARCHAR(255)").HasColumnName("SEMAV");
            entity.Property(e => e.Sepmav).HasColumnType("VARCHAR(255)").HasColumnName("SEPMAV");
            entity.Property(e => e.Sexe).HasColumnType("VARCHAR(255)").HasColumnName("SEXE");
            entity.Property(e => e.Statut).HasColumnType("VARCHAR(255)").HasColumnName("STATUT");
            entity.Property(e => e.Temav).HasColumnType("VARCHAR(255)").HasColumnName("TEMAV");
            entity.Property(e => e.Trei).HasColumnType("VARCHAR(255)").HasColumnName("TREI");
        });


        modelBuilder.Entity<Decharge>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("DECHARGE");

            entity.Property(e => e.ArticleNom).HasColumnType("VARCHAR(255)");
            entity.Property(e => e.DateDecharge).HasColumnType("TIMESTAMP(26)");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PersonnelNom).HasColumnType("VARCHAR(255)");
            entity.Property(e => e.Quantite).HasColumnName("QUANTITE");
            entity.Property(e => e.SignaturePath).HasColumnType("VARCHAR(255)");
            entity.Property(e => e.Unite).HasColumnType("VARCHAR(255)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}