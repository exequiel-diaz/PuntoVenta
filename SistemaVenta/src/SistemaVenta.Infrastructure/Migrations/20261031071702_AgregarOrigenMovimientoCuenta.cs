using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarOrigenMovimientoCuenta : Migration
    {
        /// <inheritdoc />

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primero agregamos la columna permitiendo NULL.
            migrationBuilder.AddColumn<int>(
                name: "Origen",
                table: "MovimientosCuentaCorriente",
                type: "integer",
                nullable: true);

            // Clasificamos los movimientos históricos conocidos.

            migrationBuilder.Sql("""
                UPDATE "MovimientosCuentaCorriente"
                SET "Origen" = CASE
                    -- Movimientos correspondientes a pagos.
                    WHEN "Tipo" = 2
                        AND "PagoId" IS NOT NULL
                        THEN 2

                    -- Anulaciones de ventas.
                    WHEN "Tipo" = 2
                        AND "VentaId" IS NOT NULL
                        AND "PagoId" IS NULL
                        AND "Descripcion" LIKE 'Anulación de venta #%'
                        THEN 3

                    -- Débitos generados por ventas.
                    WHEN "Tipo" = 1
                        AND "VentaId" IS NOT NULL
                        AND "PagoId" IS NULL
                        THEN 1

                    -- Movimientos manuales sin relaciones.
                    WHEN "VentaId" IS NULL
                        AND "PagoId" IS NULL
                        THEN 6

                    ELSE NULL
                END;
                """);

            // Si existen movimientos no identificados,
            // detenemos la migración antes de asignarles
            // un origen incorrecto.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "MovimientosCuentaCorriente"
                        WHERE "Origen" IS NULL
                    ) THEN
                        RAISE EXCEPTION
                            'Existen movimientos históricos sin origen identificado.';
                    END IF;
                END $$;
                """);

            // Finalmente hacemos obligatoria la columna.
            migrationBuilder.AlterColumn<int>(
                name: "Origen",
                table: "MovimientosCuentaCorriente",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origen",
                table: "MovimientosCuentaCorriente");
        }
    }
}
