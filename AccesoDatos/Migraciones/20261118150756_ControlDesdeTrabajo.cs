using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class ControlDesdeTrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrabajoId",
                table: "Controles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrabajoParcialId",
                table: "Controles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Controles_TrabajoId",
                table: "Controles",
                column: "TrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_Controles_TrabajoParcialId",
                table: "Controles",
                column: "TrabajoParcialId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Controles_TrabajoParciales_TrabajoParcialId",
                table: "Controles",
                column: "TrabajoParcialId",
                principalTable: "TrabajoParciales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Controles_Trabajos_TrabajoId",
                table: "Controles",
                column: "TrabajoId",
                principalTable: "Trabajos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Controles_TrabajoParciales_TrabajoParcialId",
                table: "Controles");

            migrationBuilder.DropForeignKey(
                name: "FK_Controles_Trabajos_TrabajoId",
                table: "Controles");

            migrationBuilder.DropIndex(
                name: "IX_Controles_TrabajoId",
                table: "Controles");

            migrationBuilder.DropIndex(
                name: "IX_Controles_TrabajoParcialId",
                table: "Controles");

            migrationBuilder.DropColumn(
                name: "TrabajoId",
                table: "Controles");

            migrationBuilder.DropColumn(
                name: "TrabajoParcialId",
                table: "Controles");
        }
    }
}
