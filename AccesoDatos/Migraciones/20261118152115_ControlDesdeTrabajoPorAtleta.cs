using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class ControlDesdeTrabajoPorAtleta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Controles_TrabajoParcialId",
                table: "Controles");

            migrationBuilder.CreateIndex(
                name: "IX_Controles_AtletaId_TrabajoParcialId",
                table: "Controles",
                columns: new[] { "AtletaId", "TrabajoParcialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Controles_TrabajoParcialId",
                table: "Controles",
                column: "TrabajoParcialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Controles_AtletaId_TrabajoParcialId",
                table: "Controles");

            migrationBuilder.DropIndex(
                name: "IX_Controles_TrabajoParcialId",
                table: "Controles");

            migrationBuilder.CreateIndex(
                name: "IX_Controles_TrabajoParcialId",
                table: "Controles",
                column: "TrabajoParcialId",
                unique: true);
        }
    }
}
