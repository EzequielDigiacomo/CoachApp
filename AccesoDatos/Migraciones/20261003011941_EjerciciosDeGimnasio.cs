using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class EjerciciosDeGimnasio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrabajoEjercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrabajoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrabajoEjercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrabajoEjercicios_Trabajos_TrabajoId",
                        column: x => x.TrabajoId,
                        principalTable: "Trabajos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrabajoSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrabajoEjercicioId = table.Column<int>(type: "integer", nullable: false),
                    Repeticiones = table.Column<int>(type: "integer", nullable: false),
                    Porcentaje = table.Column<int>(type: "integer", nullable: true),
                    PesoKg = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrabajoSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrabajoSeries_TrabajoEjercicios_TrabajoEjercicioId",
                        column: x => x.TrabajoEjercicioId,
                        principalTable: "TrabajoEjercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrabajoEjercicios_TrabajoId_Orden",
                table: "TrabajoEjercicios",
                columns: new[] { "TrabajoId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_TrabajoSeries_TrabajoEjercicioId_Orden",
                table: "TrabajoSeries",
                columns: new[] { "TrabajoEjercicioId", "Orden" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrabajoSeries");

            migrationBuilder.DropTable(
                name: "TrabajoEjercicios");
        }
    }
}
