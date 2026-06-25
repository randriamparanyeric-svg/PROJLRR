using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading; 
using System.Threading.Tasks; 
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

    // --- Les Tables de la Base de Données ---
    public virtual DbSet<Article> Articles { get; set; }
    public virtual DbSet<Personnel> Personnels { get; set; }
    public virtual DbSet<Base1> Base1s { get; set; }
    public virtual DbSet<Decharge> Decharges { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Ne pas configurer ici - laisser Program.cs gérer le chemin
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configuration de la table ARTICLE
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("ARTICLE");

            entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nom).HasColumnType("TEXT").HasColumnName("Nom");
            entity.Property(e => e.Quantite).HasColumnName("Quantite");
            entity.Property(e => e.Unite).HasColumnType("TEXT").HasColumnName("Unite");
            entity.Property(e => e.StockSec).HasColumnName("StockSec");

            // 📅 ─── NOUVELLE COLONNE DE SYNCHRONISATION (LAST WRITE ARTICLES) ───
            entity.Property(e => e.DateModif).HasColumnType("TEXT").HasColumnName("DateModif");
        });

        // 2. Configuration de la table PERSONNEL (BASE)
        modelBuilder.Entity<Personnel>(entity =>
        {
            entity.HasKey(e => e.Num);
            entity.Property(e => e.Num).ValueGeneratedOnAdd();
            entity.ToTable("BASE");

            entity.Property(e => e.Num).HasColumnName("Num");
            entity.Property(e => e.Cemav).HasColumnType("TEXT").HasColumnName("CEMAV");
            entity.Property(e => e.Cin).HasColumnType("TEXT").HasColumnName("CIN");
            entity.Property(e => e.Contact).HasColumnType("TEXT").HasColumnName("CONTACT");
            entity.Property(e => e.Corps).HasColumnType("TEXT").HasColumnName("CORPS");
            entity.Property(e => e.Datedentre).HasColumnType("TEXT").HasColumnName("DATEDENTRE");
            entity.Property(e => e.Datedeprise).HasColumnType("TEXT").HasColumnName("DATEDEPRISE");
            entity.Property(e => e.Datenaiss).HasColumnType("TEXT").HasColumnName("DATENAISS");
            entity.Property(e => e.Dec).HasColumnType("TEXT").HasColumnName("DEC");
            entity.Property(e => e.Demav).HasColumnType("TEXT").HasColumnName("DEMAV");
            entity.Property(e => e.Diplomeac).HasColumnType("TEXT").HasColumnName("DIPLOMEAC");
            entity.Property(e => e.Diplomeped).HasColumnType("TEXT").HasColumnName("DIPLOMEPED");
            entity.Property(e => e.Dou).HasColumnType("TEXT").HasColumnName("DOU");
            entity.Property(e => e.Dxemav).HasColumnType("TEXT").HasColumnName("DXEMAV");
            entity.Property(e => e.Fonction).HasColumnType("TEXT").HasColumnName("FONCTION");
            entity.Property(e => e.Hemav).HasColumnType("TEXT").HasColumnName("HEMAV");
            entity.Property(e => e.Lieudenaiss).HasColumnType("TEXT").HasColumnName("LIEUDENAISS");
            entity.Property(e => e.Matiere).HasColumnType("TEXT").HasColumnName("MATIERE");
            entity.Property(e => e.Matricule).HasColumnType("TEXT").HasColumnName("MATRICULE");
            entity.Property(e => e.Nemav).HasColumnType("TEXT").HasColumnName("NEMAV");
            entity.Property(e => e.NomEtPrenoms).HasColumnType("TEXT").HasColumnName("NOM_ET_PRENOMS");
            entity.Property(e => e.Onemav).HasColumnType("TEXT").HasColumnName("ONEMAV");
            entity.Property(e => e.Perav).HasColumnType("TEXT").HasColumnName("PERAV");
            entity.Property(e => e.Photo).HasColumnType("TEXT");
            entity.Property(e => e.Qemav).HasColumnType("TEXT").HasColumnName("QEMAV");
            entity.Property(e => e.Quat).HasColumnType("TEXT").HasColumnName("QUAT");
            entity.Property(e => e.Quin).HasColumnType("TEXT").HasColumnName("QUIN");
            entity.Property(e => e.Seiz).HasColumnType("TEXT").HasColumnName("SEIZ");
            entity.Property(e => e.Semav).HasColumnType("TEXT").HasColumnName("SEMAV");
            entity.Property(e => e.Sepmav).HasColumnType("TEXT").HasColumnName("SEPMAV");
            entity.Property(e => e.Sexe).HasColumnType("TEXT").HasColumnName("SEXE");
            entity.Property(e => e.Statut).HasColumnType("TEXT").HasColumnName("STATUT");
            entity.Property(e => e.Temav).HasColumnType("TEXT").HasColumnName("TEMAV");
            entity.Property(e => e.Trei).HasColumnType("TEXT").HasColumnName("TREI");

            // ─── MAPPAGE DES NOUVELLES COLONNES AJOUTÉES ───
            entity.Property(e => e.Grade).HasColumnType("TEXT").HasColumnName("GRADE");
            entity.Property(e => e.SerieBacc).HasColumnType("TEXT").HasColumnName("SERIE_BACC");
            entity.Property(e => e.ClasseTenue1).HasColumnType("TEXT").HasColumnName("CLASSE_TENUE_1");
            entity.Property(e => e.ClasseTenue2).HasColumnType("TEXT").HasColumnName("CLASSE_TENUE_2");
            entity.Property(e => e.ClasseTenue3).HasColumnType("TEXT").HasColumnName("CLASSE_TENUE_3");
            entity.Property(e => e.ClasseTenue4).HasColumnType("TEXT").HasColumnName("CLASSE_TENUE_4");
            entity.Property(e => e.ClasseTenue5).HasColumnType("TEXT").HasColumnName("CLASSE_TENUE_5");
            entity.Property(e => e.ClasseTenue6).HasColumnType("TEXT").HasColumnName("CLASSE_TENUE_6");
            entity.Property(e => e.ClasseTenue7).HasColumnType("TEXT").HasColumnName("CLASSE_TENUE_7");
            entity.Property(e => e.ClasseTenue8).HasColumnType("TEXT").HasColumnName("CLASSE_TENUE_8");

            // ─── NOUVELLE COLONNE DE SYNCHRONISATION (LAST WRITE PERSONNEL) ───
            entity.Property(e => e.DateModif).HasColumnType("TEXT").HasColumnName("DateModif");
        });

        // 3. Configuration de la table BASE1
        modelBuilder.Entity<Base1>(entity =>
        {
            entity.ToTable("BASE1");
            entity.HasKey(e => e.Num);
            entity.Property(e => e.Num).ValueGeneratedOnAdd();
            
            entity.Property(e => e.Cemav).HasColumnType("TEXT").HasColumnName("CEMAV");
            entity.Property(e => e.Cin).HasColumnType("TEXT").HasColumnName("CIN");
            entity.Property(e => e.Contact).HasColumnType("TEXT").HasColumnName("CONTACT");
            entity.Property(e => e.Corps).HasColumnType("TEXT").HasColumnName("CORPS");
            entity.Property(e => e.Datedentre).HasColumnType("TEXT").HasColumnName("DATEDENTRE");
            entity.Property(e => e.Datedeprise).HasColumnType("TEXT").HasColumnName("DATEDEPRISE");
            entity.Property(e => e.Datenaiss).HasColumnType("TEXT").HasColumnName("DATENAISS");
            entity.Property(e => e.Dec).HasColumnType("TEXT").HasColumnName("DEC");
            entity.Property(e => e.Demav).HasColumnType("TEXT").HasColumnName("DEMAV");
            entity.Property(e => e.Diplomeac).HasColumnType("TEXT").HasColumnName("DIPLOMEAC");
            entity.Property(e => e.Diplomeped).HasColumnType("TEXT").HasColumnName("DIPLOMEPED");
            entity.Property(e => e.Dou).HasColumnType("TEXT").HasColumnName("DOU");
            entity.Property(e => e.Dxemav).HasColumnType("TEXT").HasColumnName("DXEMAV");
            entity.Property(e => e.Fonction).HasColumnType("TEXT").HasColumnName("FONCTION");
            entity.Property(e => e.Hemav).HasColumnType("TEXT").HasColumnName("HEMAV");
            entity.Property(e => e.Lieudenaiss).HasColumnType("TEXT").HasColumnName("LIEUDENAISS");
            entity.Property(e => e.Matiere).HasColumnType("TEXT").HasColumnName("MATIERE");
            entity.Property(e => e.Matricule).HasColumnType("TEXT").HasColumnName("MATRICULE");
            entity.Property(e => e.Nemav).HasColumnType("TEXT").HasColumnName("NEMAV");
            entity.Property(e => e.NomEtPrenoms).HasColumnType("TEXT").HasColumnName("NOM_ET_PRENOMS");
            entity.Property(e => e.Onemav).HasColumnType("TEXT").HasColumnName("ONEMAV");
            entity.Property(e => e.Perav).HasColumnType("TEXT").HasColumnName("PERAV");
            entity.Property(e => e.Photo).HasColumnType("TEXT");
            entity.Property(e => e.Qemav).HasColumnType("TEXT").HasColumnName("QEMAV");
            entity.Property(e => e.Quat).HasColumnType("TEXT").HasColumnName("QUAT");
            entity.Property(e => e.Quin).HasColumnType("TEXT").HasColumnName("QUIN");
            entity.Property(e => e.Seiz).HasColumnType("TEXT").HasColumnName("SEIZ");
            entity.Property(e => e.Semav).HasColumnType("TEXT").HasColumnName("SEMAV");
            entity.Property(e => e.Sepmav).HasColumnType("TEXT").HasColumnName("SEPMAV");
            entity.Property(e => e.Sexe).HasColumnType("TEXT").HasColumnName("SEXE");
            entity.Property(e => e.Statut).HasColumnType("TEXT").HasColumnName("STATUT");
            entity.Property(e => e.Temav).HasColumnType("TEXT").HasColumnName("TEMAV");
            entity.Property(e => e.Trei).HasColumnType("TEXT").HasColumnName("TREI");
        });

        // 4. Configuration de la table DECHARGE
        modelBuilder.Entity<Decharge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("DECHARGE");

            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.ArticleNom).HasColumnType("TEXT").HasColumnName("ArticleNom");
            entity.Property(e => e.DateDecharge).HasColumnName("DateDecharge");
            entity.Property(e => e.PersonnelNom).HasColumnType("TEXT").HasColumnName("PersonnelNom");
            entity.Property(e => e.Quantite).HasColumnName("Quantite");
            entity.Property(e => e.SignaturePath).HasColumnType("TEXT").HasColumnName("SignaturePath");
            entity.Property(e => e.Unite).HasColumnType("TEXT").HasColumnName("Unite");
        });

        // 5. Configuration de la table NOTIFICATION
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Notifications");
            entity.Property(e => e.Message).IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    // ─── INTERCEPTION AUTOMATIQUE DE LA DATE DE MODIFICATION ──────────────────

    public override int SaveChanges()
    {
        MettreAJourDatesModification();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        MettreAJourDatesModification();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void MettreAJourDatesModification()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added 
                     || e.State == Microsoft.EntityFrameworkCore.EntityState.Modified);

        var maintenant = DateTime.UtcNow;
        DateTime dateUtcSansMillisecondes = new DateTime(
            maintenant.Year, maintenant.Month, maintenant.Day, 
            maintenant.Hour, maintenant.Minute, maintenant.Second, 
            DateTimeKind.Utc
        );

        foreach (var entry in entries)
        {
            var dateModifProp = entry.Entity.GetType().GetProperty("DateModif");
            if (dateModifProp != null)
            {
                dateModifProp.SetValue(entry.Entity, dateUtcSansMillisecondes);
            }
        }
    }
}