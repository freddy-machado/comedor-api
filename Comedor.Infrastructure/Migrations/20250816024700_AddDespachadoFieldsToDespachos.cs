using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comedor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDespachadoFieldsToDespachos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Despachado",
                table: "CM_Despachos",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HoraDespacho",
                table: "CM_Despachos",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Despachado",
                table: "CM_Despachos");

            migrationBuilder.DropColumn(
                name: "HoraDespacho",
                table: "CM_Despachos");
        }
    }
}
