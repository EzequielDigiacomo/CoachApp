using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class TipoDeTrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las filas que ya existian no tienen tipo. Se les pone Tierra (pista),
            // que es lo mas comun en un control con parciales cada 250 m; despues
            // se puede cambiar desde el modal. No puede quedar en "" porque el
            // valor no se podria convertir al enum al leerlo.
            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Trabajos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Tierra");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Trabajos");
        }
    }
}
