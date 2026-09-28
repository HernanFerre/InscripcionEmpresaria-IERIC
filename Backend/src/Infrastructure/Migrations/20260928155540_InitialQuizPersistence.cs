using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialQuizPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "QuizSesion",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuitEmpresa = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    IntentosTotales = table.Column<byte>(type: "tinyint", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime", nullable: false),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    BloqueadoHasta = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSesion", x => x.Id);
                    table.CheckConstraint("CK_QuizSesion_Estado", "[Estado] BETWEEN 1 AND 4");
                    table.CheckConstraint("CK_QuizSesion_IntentosTotales", "[IntentosTotales] > 0");
                });

            migrationBuilder.CreateTable(
                name: "QuizCuilVinculado",
                schema: "dbo",
                columns: table => new
                {
                    QuizSesionId = table.Column<long>(type: "bigint", nullable: false),
                    Cuil = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCuilVinculado", x => new { x.QuizSesionId, x.Cuil });
                    table.ForeignKey(
                        name: "FK_QuizCuilVinculado_QuizSesion_QuizSesionId",
                        column: x => x.QuizSesionId,
                        principalSchema: "dbo",
                        principalTable: "QuizSesion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizDesafio",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizSesionId = table.Column<long>(type: "bigint", nullable: false),
                    Numero = table.Column<byte>(type: "tinyint", nullable: false),
                    Escenario = table.Column<byte>(type: "tinyint", nullable: false),
                    EsActual = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizDesafio", x => x.Id);
                    table.CheckConstraint("CK_QuizDesafio_Escenario", "[Escenario] BETWEEN 1 AND 4");
                    table.CheckConstraint("CK_QuizDesafio_Numero", "[Numero] > 0");
                    table.ForeignKey(
                        name: "FK_QuizDesafio_QuizSesion_QuizSesionId",
                        column: x => x.QuizSesionId,
                        principalSchema: "dbo",
                        principalTable: "QuizSesion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizOpcion",
                schema: "dbo",
                columns: table => new
                {
                    QuizDesafioId = table.Column<long>(type: "bigint", nullable: false),
                    CodigoOpcion = table.Column<byte>(type: "tinyint", nullable: false),
                    Cuil = table.Column<long>(type: "bigint", nullable: true),
                    EsVinculado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizOpcion", x => new { x.QuizDesafioId, x.CodigoOpcion });
                    table.CheckConstraint("CK_QuizOpcion_Codigo", "[CodigoOpcion] BETWEEN 0 AND 5");
                    table.CheckConstraint("CK_QuizOpcion_Cuil", "(\r\n                            (\r\n                                [CodigoOpcion] BETWEEN 0 AND 3\r\n                                AND [Cuil] IS NOT NULL\r\n                            )\r\n                            OR\r\n                            (\r\n                                [CodigoOpcion] BETWEEN 4 AND 5\r\n                                AND [Cuil] IS NULL\r\n                            )\r\n                        )");
                    table.ForeignKey(
                        name: "FK_QuizOpcion_QuizDesafio_QuizDesafioId",
                        column: x => x.QuizDesafioId,
                        principalSchema: "dbo",
                        principalTable: "QuizDesafio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizRespuesta",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizDesafioId = table.Column<long>(type: "bigint", nullable: false),
                    EsCorrecta = table.Column<bool>(type: "bit", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizRespuesta", x => x.Id);
                    table.UniqueConstraint("AK_QuizRespuesta_Id_QuizDesafioId", x => new { x.Id, x.QuizDesafioId });
                    table.UniqueConstraint("UX_QuizRespuesta_QuizDesafio", x => x.QuizDesafioId);
                    table.ForeignKey(
                        name: "FK_QuizRespuesta_QuizDesafio_QuizDesafioId",
                        column: x => x.QuizDesafioId,
                        principalSchema: "dbo",
                        principalTable: "QuizDesafio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizRespuestaOpcion",
                schema: "dbo",
                columns: table => new
                {
                    QuizRespuestaId = table.Column<long>(type: "bigint", nullable: false),
                    CodigoOpcion = table.Column<byte>(type: "tinyint", nullable: false),
                    QuizDesafioId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizRespuestaOpcion", x => new { x.QuizRespuestaId, x.CodigoOpcion });
                    table.ForeignKey(
                        name: "FK_QuizRespuestaOpcion_QuizOpcion",
                        columns: x => new { x.QuizDesafioId, x.CodigoOpcion },
                        principalSchema: "dbo",
                        principalTable: "QuizOpcion",
                        principalColumns: new[] { "QuizDesafioId", "CodigoOpcion" });
                    table.ForeignKey(
                        name: "FK_QuizRespuestaOpcion_QuizRespuesta",
                        columns: x => new { x.QuizRespuestaId, x.QuizDesafioId },
                        principalSchema: "dbo",
                        principalTable: "QuizRespuesta",
                        principalColumns: new[] { "Id", "QuizDesafioId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_QuizDesafio_Sesion_Actual",
                schema: "dbo",
                table: "QuizDesafio",
                column: "QuizSesionId",
                unique: true,
                filter: "[EsActual] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_QuizDesafio_Sesion_Numero",
                schema: "dbo",
                table: "QuizDesafio",
                columns: new[] { "QuizSesionId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizRespuestaOpcion_QuizDesafioId_CodigoOpcion",
                schema: "dbo",
                table: "QuizRespuestaOpcion",
                columns: new[] { "QuizDesafioId", "CodigoOpcion" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizRespuestaOpcion_QuizRespuestaId_QuizDesafioId",
                schema: "dbo",
                table: "QuizRespuestaOpcion",
                columns: new[] { "QuizRespuestaId", "QuizDesafioId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizSesion_Cuit_Bloqueo",
                schema: "dbo",
                table: "QuizSesion",
                columns: new[] { "CuitEmpresa", "Estado", "BloqueadoHasta" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizSesion_Usuario_Fecha",
                schema: "dbo",
                table: "QuizSesion",
                columns: new[] { "UsuarioId", "FechaCreacion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizCuilVinculado",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuizRespuestaOpcion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuizOpcion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuizRespuesta",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuizDesafio",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuizSesion",
                schema: "dbo");
        }
    }
}
