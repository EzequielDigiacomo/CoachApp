using AccesoDatos;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    [DbContext(typeof(CoachDbContext))]
    [Migration("20261006153000_TrabajoDesdeGarmin")]
    public partial class TrabajoDesdeGarmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GarminActividadId",
                table: "Trabajos",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GarminActividadId",
                table: "Trabajos");
        }
    }
}
