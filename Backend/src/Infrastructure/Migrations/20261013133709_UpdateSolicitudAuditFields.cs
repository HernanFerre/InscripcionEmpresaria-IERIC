using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSolicitudAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ua",
                schema: "dbo",
                table: "SolicitudInscripcionDigitalEmpresaria");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                schema: "dbo",
                table: "SolicitudInscripcionDigitalEmpresaria",
                newName: "IdUsuarioAlta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdUsuarioAlta",
                schema: "dbo",
                table: "SolicitudInscripcionDigitalEmpresaria",
                newName: "UsuarioId");

            migrationBuilder.AddColumn<string>(
                name: "ua",
                schema: "dbo",
                table: "SolicitudInscripcionDigitalEmpresaria",
                type: "varchar(23)",
                maxLength: 23,
                nullable: true);
        }
    }
}
