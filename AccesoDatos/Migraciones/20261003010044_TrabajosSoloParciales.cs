using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class TrabajosSoloParciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanciaMetros",
                table: "Trabajos");

            migrationBuilder.DropColumn(
                name: "TiempoTotal",
                table: "Trabajos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistanciaMetros",
                table: "Trabajos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TiempoTotal",
                table: "Trabajos",
                type: "interval",
                nullable: true);
        }
    }
}
