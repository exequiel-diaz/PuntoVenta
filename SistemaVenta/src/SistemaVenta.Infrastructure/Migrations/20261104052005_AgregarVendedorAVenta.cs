using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVendedorAVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VendedorId",
                table: "Ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_VendedorId",
                table: "Ventas",
                column: "VendedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_usuarios_VendedorId",
                table: "Ventas",
                column: "VendedorId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_usuarios_VendedorId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_VendedorId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "VendedorId",
                table: "Ventas");
        }
    }
}
