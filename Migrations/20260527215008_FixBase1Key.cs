using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROJLRR.Migrations
{
    /// <inheritdoc />
    public partial class FixBase1Key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ARTICLE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: true),
                    Nom = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    Quantite = table.Column<int>(type: "INTEGER", nullable: true),
                    Unite = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    StockSec = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "BASE",
                columns: table => new
                {
                    Num = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MATRICULE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    NOM_ET_PRENOMS = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    CIN = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DEC = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    CORPS = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    MATIERE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DATENAISS = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    LIEUDENAISS = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    SEXE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    STATUT = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DATEDENTRE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DATEDEPRISE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DIPLOMEAC = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DIPLOMEPED = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    CONTACT = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    PERAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    TEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    QEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    CEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    SEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    SEPMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    HEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    NEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DXEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    ONEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DOU = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    TREI = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    QUAT = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    QUIN = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    SEIZ = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    FONCTION = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    Photo = table.Column<string>(type: "VARCHAR(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BASE", x => x.Num);
                });

            migrationBuilder.CreateTable(
                name: "BASE1",
                columns: table => new
                {
                    Num = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    MATRICULE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    NOM_ET_PRENOMS = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    CIN = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DEC = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    CORPS = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    MATIERE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DATENAISS = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    LIEUDENAISS = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    SEXE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    STATUT = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DATEDENTRE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DATEDEPRISE = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DIPLOMEAC = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DIPLOMEPED = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    CONTACT = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    PERAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    TEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    QEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    CEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    SEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    SEPMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    HEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    NEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DXEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    ONEMAV = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DOU = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    TREI = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    QUAT = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    QUIN = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    SEIZ = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    FONCTION = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    Photo = table.Column<string>(type: "VARCHAR(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BASE1", x => x.Num);
                });

            migrationBuilder.CreateTable(
                name: "DECHARGE",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonnelNom = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    ArticleNom = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    QUANTITE = table.Column<int>(type: "INTEGER", nullable: true),
                    Unite = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    DateDecharge = table.Column<long>(type: "TIMESTAMP(26)", nullable: true),
                    SignaturePath = table.Column<string>(type: "VARCHAR(255)", nullable: true)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ARTICLE");

            migrationBuilder.DropTable(
                name: "BASE");

            migrationBuilder.DropTable(
                name: "BASE1");

            migrationBuilder.DropTable(
                name: "DECHARGE");
        }
    }
}
