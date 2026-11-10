using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCostoHistoricoDetalleVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoUnitario",
                table: "DetallesVenta",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoUnitario",
                table: "DetallesVenta");
        }
    }
}
