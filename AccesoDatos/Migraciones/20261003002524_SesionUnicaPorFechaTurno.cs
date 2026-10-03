using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class SesionUnicaPorFechaTurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entrenamientos_Fecha_Turno_Sesion",
                table: "Entrenamientos");

            migrationBuilder.CreateIndex(
                name: "IX_Entrenamientos_Fecha_Turno_Sesion",
                table: "Entrenamientos",
                columns: new[] { "Fecha", "Turno", "Sesion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entrenamientos_Fecha_Turno_Sesion",
                table: "Entrenamientos");

            migrationBuilder.CreateIndex(
                name: "IX_Entrenamientos_Fecha_Turno_Sesion",
                table: "Entrenamientos",
                columns: new[] { "Fecha", "Turno", "Sesion" });
        }
    }
}
