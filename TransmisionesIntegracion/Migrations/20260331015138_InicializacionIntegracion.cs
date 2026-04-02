using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransmisionesIntegracion.Migrations
{
    /// <inheritdoc />
    public partial class InicializacionIntegracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductosCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", nullable: false),
                    StockActual = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransaccionesPendientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TipoTransaccion = table.Column<string>(type: "TEXT", nullable: false),
                    DatosJson = table.Column<string>(type: "TEXT", nullable: false),
                    FechaIntento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionesPendientes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductosCache");

            migrationBuilder.DropTable(
                name: "TransaccionesPendientes");
        }
    }
}
