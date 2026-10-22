using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class ControlAgua : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnioTestigo",
                table: "Controles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistanciaMetros",
                table: "Controles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Tiempo",
                table: "Controles",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TiempoTestigo",
                table: "Controles",
                type: "interval",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnioTestigo",
                table: "Controles");

            migrationBuilder.DropColumn(
                name: "DistanciaMetros",
                table: "Controles");

            migrationBuilder.DropColumn(
                name: "Tiempo",
                table: "Controles");

            migrationBuilder.DropColumn(
                name: "TiempoTestigo",
                table: "Controles");
        }
    }
}
