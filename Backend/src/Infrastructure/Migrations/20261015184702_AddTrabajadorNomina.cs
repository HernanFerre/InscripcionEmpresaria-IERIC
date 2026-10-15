using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrabajadorNomina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrabajadorNomina",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSolicitud = table.Column<long>(type: "bigint", nullable: false),
                    ApellidoYNombre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Cuil = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false),
                    Ingreso = table.Column<DateTime>(type: "date", nullable: false),
                    Egreso = table.Column<DateTime>(type: "date", nullable: true),
                    PerteneceAConstr = table.Column<bool>(type: "bit", nullable: false),
                    EsManual = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrabajadorNomina", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_TrabajadorNomina_Solicitud",
                        column: x => x.IdSolicitud,
                        principalSchema: "dbo",
                        principalTable: "SolicitudInscripcionDigitalEmpresaria",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrabajadorNomina_IdSolicitud",
                schema: "dbo",
                table: "TrabajadorNomina",
                column: "IdSolicitud");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrabajadorNomina",
                schema: "dbo");
        }
    }
}
