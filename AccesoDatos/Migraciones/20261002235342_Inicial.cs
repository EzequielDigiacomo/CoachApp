using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccesoDatos.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Atletas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Club = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Dni = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atletas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entrenamientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Turno = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Sesion = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Club = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entrenamientos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreUsuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Rol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntrenamientoAtletas",
                columns: table => new
                {
                    EntrenamientoId = table.Column<int>(type: "integer", nullable: false),
                    AtletaId = table.Column<int>(type: "integer", nullable: false),
                    Asistio = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrenamientoAtletas", x => new { x.EntrenamientoId, x.AtletaId });
                    table.ForeignKey(
                        name: "FK_EntrenamientoAtletas_Atletas_AtletaId",
                        column: x => x.AtletaId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntrenamientoAtletas_Entrenamientos_EntrenamientoId",
                        column: x => x.EntrenamientoId,
                        principalTable: "Entrenamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anotaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    TranscripcionOriginal = table.Column<string>(type: "text", nullable: true),
                    Origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AudioRuta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AudioContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AudioDuracionSegundos = table.Column<int>(type: "integer", nullable: true),
                    AudioTamanioBytes = table.Column<long>(type: "bigint", nullable: true),
                    ProveedorTranscripcion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ModeloTranscripcion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Idioma = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ConfianzaPromedio = table.Column<double>(type: "double precision", nullable: true),
                    RequiereRevision = table.Column<bool>(type: "boolean", nullable: false),
                    AtletaId = table.Column<int>(type: "integer", nullable: true),
                    EntrenamientoId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anotaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anotaciones_Atletas_AtletaId",
                        column: x => x.AtletaId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Anotaciones_Entrenamientos_EntrenamientoId",
                        column: x => x.EntrenamientoId,
                        principalTable: "Entrenamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Anotaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anotaciones_AtletaId",
                table: "Anotaciones",
                column: "AtletaId");

            migrationBuilder.CreateIndex(
                name: "IX_Anotaciones_EntrenamientoId",
                table: "Anotaciones",
                column: "EntrenamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anotaciones_FechaCreacion",
                table: "Anotaciones",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Anotaciones_Origen",
                table: "Anotaciones",
                column: "Origen");

            migrationBuilder.CreateIndex(
                name: "IX_Anotaciones_UsuarioId",
                table: "Anotaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Atletas_Dni",
                table: "Atletas",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntrenamientoAtletas_AtletaId",
                table: "EntrenamientoAtletas",
                column: "AtletaId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrenamientos_Fecha_Turno_Sesion",
                table: "Entrenamientos",
                columns: new[] { "Fecha", "Turno", "Sesion" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anotaciones");

            migrationBuilder.DropTable(
                name: "EntrenamientoAtletas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Atletas");

            migrationBuilder.DropTable(
                name: "Entrenamientos");
        }
    }
}
