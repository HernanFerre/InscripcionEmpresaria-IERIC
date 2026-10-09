using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaSolicitudPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizRespuestaOpcion_QuizDesafioId_CodigoOpcion",
                schema: "dbo",
                table: "QuizRespuestaOpcion");

            migrationBuilder.DropIndex(
                name: "IX_QuizRespuestaOpcion_QuizRespuestaId_QuizDesafioId",
                schema: "dbo",
                table: "QuizRespuestaOpcion");

            migrationBuilder.CreateTable(
                name: "Empresa",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RazonSocial = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Cuit = table.Column<long>(type: "bigint", nullable: false),
                    EsCooperativa = table.Column<bool>(type: "bit", nullable: false),
                    EstadoActivo = table.Column<bool>(type: "bit", nullable: false),
                    LegacyId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    fa = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ua = table.Column<string>(type: "varchar(23)", maxLength: 23, nullable: true),
                    fm = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    um = table.Column<string>(type: "varchar(23)", maxLength: 23, nullable: true),
                    Calle = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false, defaultValueSql: "''"),
                    Numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValueSql: "''"),
                    Piso = table.Column<byte>(type: "tinyint", nullable: true),
                    DeptoOficina = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    CodigoPostal = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValueSql: "''"),
                    IdProvincia = table.Column<int>(type: "int", nullable: true),
                    IdLocalidad = table.Column<int>(type: "int", nullable: true),
                    Correo = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false, defaultValueSql: "''"),
                    Telefono = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    IdActividadsolicitud = table.Column<int>(type: "int", nullable: false, defaultValueSql: "''"),
                    IdCaracter = table.Column<int>(type: "int", nullable: false, defaultValueSql: "''"),
                    IdTipoSoc = table.Column<int>(type: "int", nullable: false, defaultValueSql: "''"),
                    DDJJ = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "EmpresasCuit",
                schema: "dbo",
                columns: table => new
                {
                    IdEmpresa = table.Column<long>(type: "bigint", nullable: false),
                    IdEmpresaIntegrante = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresasCuit", x => new { x.IdEmpresa, x.IdEmpresaIntegrante })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_EmpresasCuit_EmpresaIntegrante",
                        column: x => x.IdEmpresaIntegrante,
                        principalSchema: "dbo",
                        principalTable: "Empresa",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmpresasCuit_EmpresaPrincipal",
                        column: x => x.IdEmpresa,
                        principalSchema: "dbo",
                        principalTable: "Empresa",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SolicitudInscripcionDigitalEmpresaria",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Idempresa = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comentarios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ua = table.Column<string>(type: "varchar(23)", maxLength: 23, nullable: true),
                    um = table.Column<string>(type: "varchar(23)", maxLength: 23, nullable: true),
                    fa = table.Column<DateTime>(type: "datetime", nullable: true),
                    fm = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolInscEmprDig", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_SolInscEmprDig_Empresa",
                        column: x => x.Idempresa,
                        principalSchema: "dbo",
                        principalTable: "Empresa",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "UX_Empresa_Cuit",
                schema: "dbo",
                table: "Empresa",
                column: "Cuit",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpresasCuit",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SolicitudInscripcionDigitalEmpresaria",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Empresa",
                schema: "dbo");

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
        }
    }
}
