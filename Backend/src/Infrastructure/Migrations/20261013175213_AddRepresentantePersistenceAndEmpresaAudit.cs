using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRepresentantePersistenceAndEmpresaAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ua",
                schema: "dbo",
                table: "Empresa");

            migrationBuilder.AddColumn<Guid>(
                name: "IdUsuarioAlta",
                schema: "dbo",
                table: "Empresa",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.CreateTable(
                name: "Representante",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEmpresa = table.Column<long>(type: "bigint", nullable: false),
                    Apellido = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Cuil = table.Column<long>(type: "bigint", nullable: false),
                    IdTipoRepresentante = table.Column<int>(type: "int", nullable: false),
                    Correo = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Representante", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_Representante_Empresa",
                        column: x => x.IdEmpresa,
                        principalSchema: "dbo",
                        principalTable: "Empresa",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SolicitudRepresentante",
                schema: "dbo",
                columns: table => new
                {
                    IdSolicitud = table.Column<long>(type: "bigint", nullable: false),
                    IdRepresentante = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudRepresentante", x => new { x.IdSolicitud, x.IdRepresentante })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_SolicitudRepresentante_Representante",
                        column: x => x.IdRepresentante,
                        principalSchema: "dbo",
                        principalTable: "Representante",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudRepresentante_Solicitud",
                        column: x => x.IdSolicitud,
                        principalSchema: "dbo",
                        principalTable: "SolicitudInscripcionDigitalEmpresaria",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "UX_Representante_Empresa_Cuil",
                schema: "dbo",
                table: "Representante",
                columns: new[] { "IdEmpresa", "Cuil" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudRepresentante",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Representante",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "IdUsuarioAlta",
                schema: "dbo",
                table: "Empresa");

            migrationBuilder.AddColumn<string>(
                name: "ua",
                schema: "dbo",
                table: "Empresa",
                type: "varchar(23)",
                maxLength: 23,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);
        }
    }
}
