using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIndiceUnicoCuit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "IX_Clientes_Cuit"
                ON "Clientes" ("Cuit");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Clientes_Cuit";
                """);
        }
    }
}
