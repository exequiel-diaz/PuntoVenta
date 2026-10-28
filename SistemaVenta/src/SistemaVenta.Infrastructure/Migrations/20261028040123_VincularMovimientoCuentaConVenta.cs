using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VincularMovimientoCuentaConVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VentaId",
                table: "MovimientosCuentaCorriente",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCuentaCorriente_VentaId",
                table: "MovimientosCuentaCorriente",
                column: "VentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCuentaCorriente_Ventas_VentaId",
                table: "MovimientosCuentaCorriente",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCuentaCorriente_Ventas_VentaId",
                table: "MovimientosCuentaCorriente");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosCuentaCorriente_VentaId",
                table: "MovimientosCuentaCorriente");

            migrationBuilder.DropColumn(
                name: "VentaId",
                table: "MovimientosCuentaCorriente");
        }
    }
}
