using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class BotesEquipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AtletaId",
                table: "Trabajos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "EntrenamientoEquipoId",
                table: "Trabajos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Equipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntrenamientoEquipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntrenamientoId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EquipoId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrenamientoEquipos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntrenamientoEquipos_Entrenamientos_EntrenamientoId",
                        column: x => x.EntrenamientoId,
                        principalTable: "Entrenamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntrenamientoEquipos_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EquipoMiembros",
                columns: table => new
                {
                    EquipoId = table.Column<int>(type: "integer", nullable: false),
                    AtletaId = table.Column<int>(type: "integer", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipoMiembros", x => new { x.EquipoId, x.AtletaId });
                    table.ForeignKey(
                        name: "FK_EquipoMiembros_Atletas_AtletaId",
                        column: x => x.AtletaId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipoMiembros_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntrenamientoEquipoMiembros",
                columns: table => new
                {
                    EntrenamientoEquipoId = table.Column<int>(type: "integer", nullable: false),
                    AtletaId = table.Column<int>(type: "integer", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrenamientoEquipoMiembros", x => new { x.EntrenamientoEquipoId, x.AtletaId });
                    table.ForeignKey(
                        name: "FK_EntrenamientoEquipoMiembros_Atletas_AtletaId",
                        column: x => x.AtletaId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntrenamientoEquipoMiembros_EntrenamientoEquipos_Entrenamie~",
                        column: x => x.EntrenamientoEquipoId,
                        principalTable: "EntrenamientoEquipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trabajos_EntrenamientoEquipoId",
                table: "Trabajos",
                column: "EntrenamientoEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_EntrenamientoEquipoMiembros_AtletaId",
                table: "EntrenamientoEquipoMiembros",
                column: "AtletaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntrenamientoEquipoMiembros_EntrenamientoEquipoId_Orden",
                table: "EntrenamientoEquipoMiembros",
                columns: new[] { "EntrenamientoEquipoId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntrenamientoEquipos_EntrenamientoId",
                table: "EntrenamientoEquipos",
                column: "EntrenamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_EntrenamientoEquipos_EquipoId",
                table: "EntrenamientoEquipos",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipoMiembros_AtletaId",
                table: "EquipoMiembros",
                column: "AtletaId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipoMiembros_EquipoId_Orden",
                table: "EquipoMiembros",
                columns: new[] { "EquipoId", "Orden" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Trabajos_EntrenamientoEquipos_EntrenamientoEquipoId",
                table: "Trabajos",
                column: "EntrenamientoEquipoId",
                principalTable: "EntrenamientoEquipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trabajos_EntrenamientoEquipos_EntrenamientoEquipoId",
                table: "Trabajos");

            migrationBuilder.DropTable(
                name: "EntrenamientoEquipoMiembros");

            migrationBuilder.DropTable(
                name: "EquipoMiembros");

            migrationBuilder.DropTable(
                name: "EntrenamientoEquipos");

            migrationBuilder.DropTable(
                name: "Equipos");

            migrationBuilder.DropIndex(
                name: "IX_Trabajos_EntrenamientoEquipoId",
                table: "Trabajos");

            migrationBuilder.DropColumn(
                name: "EntrenamientoEquipoId",
                table: "Trabajos");

            migrationBuilder.AlterColumn<int>(
                name: "AtletaId",
                table: "Trabajos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
