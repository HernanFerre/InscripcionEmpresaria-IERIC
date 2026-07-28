using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Empresa",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cuit = table.Column<long>(type: "bigint", nullable: true),
                    EsCooperativa = table.Column<bool>(type: "bit", nullable: false),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false),
                    LegacyId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioAlta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizSesion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuitEmpresa = table.Column<string>(type: "char(11)", fixedLength: true, maxLength: 11, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    IntentosTotales = table.Column<int>(type: "int", nullable: false),
                    IntentosRestantes = table.Column<int>(type: "int", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinalizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSesion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizCuilVinculado",
                columns: table => new
                {
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cuil = table.Column<string>(type: "char(11)", fixedLength: true, maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCuilVinculado", x => new { x.QuizId, x.Cuil });
                    table.ForeignKey(
                        name: "FK_QuizCuilVinculado_QuizSesion_QuizId",
                        column: x => x.QuizId,
                        principalTable: "QuizSesion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizDesafio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Escenario = table.Column<int>(type: "int", nullable: false),
                    EsActual = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizDesafio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizDesafio_QuizSesion_QuizId",
                        column: x => x.QuizId,
                        principalTable: "QuizSesion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizOpcion",
                columns: table => new
                {
                    QuizDesafioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    Cuil = table.Column<string>(type: "char(11)", fixedLength: true, maxLength: 11, nullable: false),
                    EsVinculado = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizOpcion", x => new { x.QuizDesafioId, x.Id });
                    table.ForeignKey(
                        name: "FK_QuizOpcion_QuizDesafio_QuizDesafioId",
                        column: x => x.QuizDesafioId,
                        principalTable: "QuizDesafio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizRespuesta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizDesafioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpcionesSeleccionadas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EsCorrecta = table.Column<bool>(type: "bit", nullable: false),
                    FechaRespuestaUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizRespuesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizRespuesta_QuizDesafio_QuizDesafioId",
                        column: x => x.QuizDesafioId,
                        principalTable: "QuizDesafio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_QuizDesafio_Quiz_Actual",
                table: "QuizDesafio",
                column: "QuizId",
                unique: true,
                filter: "[EsActual] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_QuizDesafio_Quiz_Numero",
                table: "QuizDesafio",
                columns: new[] { "QuizId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_QuizOpcion_Desafio_Orden",
                table: "QuizOpcion",
                columns: new[] { "QuizDesafioId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizRespuesta_Desafio_Fecha",
                table: "QuizRespuesta",
                columns: new[] { "QuizDesafioId", "FechaRespuestaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizSesion_CuitEmpresa",
                table: "QuizSesion",
                column: "CuitEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSesion_Estado",
                table: "QuizSesion",
                column: "Estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Empresa",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuizCuilVinculado");

            migrationBuilder.DropTable(
                name: "QuizOpcion");

            migrationBuilder.DropTable(
                name: "QuizRespuesta");

            migrationBuilder.DropTable(
                name: "QuizDesafio");

            migrationBuilder.DropTable(
                name: "QuizSesion");
        }
    }
}
