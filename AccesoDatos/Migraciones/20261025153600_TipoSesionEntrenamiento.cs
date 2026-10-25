using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    [DbContext(typeof(CoachDbContext))]
    [Migration("20261025153600_TipoSesionEntrenamiento")]
    public partial class TipoSesionEntrenamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Entrenamientos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Agua");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Entrenamientos");
        }
    }
}
