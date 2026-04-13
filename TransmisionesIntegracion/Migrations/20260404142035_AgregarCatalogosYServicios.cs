using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransmisionesIntegracion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCatalogosYServicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CondicionesPagoCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Plazo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CondicionesPagoCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MunicipiosCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    IdProvincia = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipiosCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProvinciasCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvinciasCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiciosCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    PrecioBase = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosCache", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CondicionesPagoCache");

            migrationBuilder.DropTable(
                name: "MunicipiosCache");

            migrationBuilder.DropTable(
                name: "ProvinciasCache");

            migrationBuilder.DropTable(
                name: "ServiciosCache");
        }
    }
}
