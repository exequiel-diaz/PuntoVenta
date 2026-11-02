using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDocumentoComercialAMovimientoCuenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentoComercialId",
                table: "MovimientosCuentaCorriente",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCuentaCorriente_DocumentoComercialId",
                table: "MovimientosCuentaCorriente",
                column: "DocumentoComercialId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCuentaCorriente_DocumentosComerciales_DocumentoC~",
                table: "MovimientosCuentaCorriente",
                column: "DocumentoComercialId",
                principalTable: "DocumentosComerciales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCuentaCorriente_DocumentosComerciales_DocumentoC~",
                table: "MovimientosCuentaCorriente");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosCuentaCorriente_DocumentoComercialId",
                table: "MovimientosCuentaCorriente");

            migrationBuilder.DropColumn(
                name: "DocumentoComercialId",
                table: "MovimientosCuentaCorriente");
        }
    }
}
