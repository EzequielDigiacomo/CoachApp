using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class TrabajosPorAtleta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trabajos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntrenamientoId = table.Column<int>(type: "integer", nullable: false),
                    AtletaId = table.Column<int>(type: "integer", nullable: false),
                    DistanciaMetros = table.Column<int>(type: "integer", nullable: false),
                    TiempoTotal = table.Column<TimeSpan>(type: "interval", nullable: true),
                    HoraInicio = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trabajos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trabajos_EntrenamientoAtletas_EntrenamientoId_AtletaId",
                        columns: x => new { x.EntrenamientoId, x.AtletaId },
                        principalTable: "EntrenamientoAtletas",
                        principalColumns: new[] { "EntrenamientoId", "AtletaId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrabajoParciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrabajoId = table.Column<int>(type: "integer", nullable: false),
                    DistanciaMetros = table.Column<int>(type: "integer", nullable: false),
                    Tiempo = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrabajoParciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrabajoParciales_Trabajos_TrabajoId",
                        column: x => x.TrabajoId,
                        principalTable: "Trabajos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrabajoParciales_TrabajoId_Orden",
                table: "TrabajoParciales",
                columns: new[] { "TrabajoId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_Trabajos_EntrenamientoId_AtletaId",
                table: "Trabajos",
                columns: new[] { "EntrenamientoId", "AtletaId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrabajoParciales");

            migrationBuilder.DropTable(
                name: "Trabajos");
        }
    }
}
