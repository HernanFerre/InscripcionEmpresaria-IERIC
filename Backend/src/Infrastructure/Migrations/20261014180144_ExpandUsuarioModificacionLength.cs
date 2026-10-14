using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandUsuarioModificacionLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "um",
                schema: "dbo",
                table: "SolicitudInscripcionDigitalEmpresaria",
                type: "varchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(23)",
                oldMaxLength: 23,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "um",
                schema: "dbo",
                table: "Empresa",
                type: "varchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(23)",
                oldMaxLength: 23,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "um",
                schema: "dbo",
                table: "SolicitudInscripcionDigitalEmpresaria",
                type: "varchar(23)",
                maxLength: 23,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "um",
                schema: "dbo",
                table: "Empresa",
                type: "varchar(23)",
                maxLength: 23,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(254)",
                oldMaxLength: 254,
                oldNullable: true);
        }
    }
}
