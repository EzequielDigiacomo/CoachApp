using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class AnotacionTrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrabajoId",
                table: "Anotaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Anotaciones_TrabajoId",
                table: "Anotaciones",
                column: "TrabajoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Anotaciones_Trabajos_TrabajoId",
                table: "Anotaciones",
                column: "TrabajoId",
                principalTable: "Trabajos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anotaciones_Trabajos_TrabajoId",
                table: "Anotaciones");

            migrationBuilder.DropIndex(
                name: "IX_Anotaciones_TrabajoId",
                table: "Anotaciones");

            migrationBuilder.DropColumn(
                name: "TrabajoId",
                table: "Anotaciones");
        }
    }
}
