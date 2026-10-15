using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class CuentasDeAtleta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtletaId",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnotacionesAtleta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AtletaId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnotacionesAtleta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnotacionesAtleta_Atletas_AtletaId",
                        column: x => x.AtletaId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sensaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AtletaId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensaciones_Atletas_AtletaId",
                        column: x => x.AtletaId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AtletaId",
                table: "Usuarios",
                column: "AtletaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnotacionesAtleta_AtletaId",
                table: "AnotacionesAtleta",
                column: "AtletaId");

            migrationBuilder.CreateIndex(
                name: "IX_AnotacionesAtleta_FechaCreacion",
                table: "AnotacionesAtleta",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Sensaciones_AtletaId",
                table: "Sensaciones",
                column: "AtletaId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensaciones_FechaCreacion",
                table: "Sensaciones",
                column: "FechaCreacion");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Atletas_AtletaId",
                table: "Usuarios",
                column: "AtletaId",
                principalTable: "Atletas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Atletas_AtletaId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "AnotacionesAtleta");

            migrationBuilder.DropTable(
                name: "Sensaciones");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_AtletaId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AtletaId",
                table: "Usuarios");
        }
    }
}
