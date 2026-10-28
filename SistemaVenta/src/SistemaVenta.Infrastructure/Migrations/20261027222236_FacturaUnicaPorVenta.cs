using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FacturaUnicaPorVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentosComerciales_VentaId",
                table: "DocumentosComerciales");

            

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosComerciales_VentaId",
                table: "DocumentosComerciales",
                column: "VentaId",
                unique: true,
                filter: "\"TipoDocumento\" = 'Factura'");

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentosComerciales_VentaId",
                table: "DocumentosComerciales");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosComerciales_VentaId",
                table: "DocumentosComerciales",
                column: "VentaId");
        }

    }
}
