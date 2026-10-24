
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    public partial class AddVentaConcurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL ya proporciona xmin como columna de sistema.
            // No es necesario agregar una columna física.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se elimina xmin porque pertenece a PostgreSQL.
        }
    }
}
