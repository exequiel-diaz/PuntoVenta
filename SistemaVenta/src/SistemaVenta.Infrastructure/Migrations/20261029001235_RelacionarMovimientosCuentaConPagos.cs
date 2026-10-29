using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelacionarMovimientosCuentaConPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PagoId",
                table: "MovimientosCuentaCorriente",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCuentaCorriente_PagoId",
                table: "MovimientosCuentaCorriente",
                column: "PagoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCuentaCorriente_Pagos_PagoId",
                table: "MovimientosCuentaCorriente",
                column: "PagoId",
                principalTable: "Pagos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCuentaCorriente_Pagos_PagoId",
                table: "MovimientosCuentaCorriente");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosCuentaCorriente_PagoId",
                table: "MovimientosCuentaCorriente");

            migrationBuilder.DropColumn(
                name: "PagoId",
                table: "MovimientosCuentaCorriente");
        }
    }
}
