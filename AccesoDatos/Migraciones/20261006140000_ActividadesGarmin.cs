using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class ActividadesGarmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GarminActividadId",
                table: "EntrenamientoAtletas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GarminCadencia",
                table: "EntrenamientoAtletas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GarminCalorias",
                table: "EntrenamientoAtletas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GarminDistanciaMetros",
                table: "EntrenamientoAtletas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GarminDuracionSegundos",
                table: "EntrenamientoAtletas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GarminFcMaxima",
                table: "EntrenamientoAtletas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GarminFcPromedio",
                table: "EntrenamientoAtletas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GarminInicio",
                table: "EntrenamientoAtletas",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GarminNombre",
                table: "EntrenamientoAtletas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GarminTipo",
                table: "EntrenamientoAtletas",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CuentasGarmin",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiraUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasGarmin", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_CuentasGarmin_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuentasGarmin");

            migrationBuilder.DropColumn(
                name: "GarminActividadId",
                table: "EntrenamientoAtletas");

            migrationBuilder.DropColumn(
                name: "GarminCadencia",
                table: "EntrenamientoAtletas");

            migrationBuilder.DropColumn(
                name: "GarminCalorias",
                table: "EntrenamientoAtletas");

            migrationBuilder.DropColumn(
                name: "GarminDistanciaMetros",
                table: "EntrenamientoAtletas");

            migrationBuilder.DropColumn(
                name: "GarminDuracionSegundos",
                table: "EntrenamientoAtletas");

            migrationBuilder.DropColumn(
                name: "GarminFcMaxima",
                table: "EntrenamientoAtletas");

            migrationBuilder.DropColumn(
                name: "GarminFcPromedio",
                table: "EntrenamientoAtletas");

            migrationBuilder.DropColumn(
                name: "GarminInicio",
                table: "EntrenamientoAtletas");

            migrationBuilder.DropColumn(
                name: "GarminNombre",
                table: "EntrenamientoAtletas");

            migrationBuilder.DropColumn(
                name: "GarminTipo",
                table: "EntrenamientoAtletas");
        }
    }
}
