
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Integration.Tests.Informes;

internal static class PeriodoPruebaInformes
{
    public static async Task<DateTime> ObtenerDiaArgentinaLibreAsync(
        AppDbContext contexto)
    {
        // Buscamos la fecha más reciente registrada.
        var ultimaFecha = await contexto.Ventas
            .MaxAsync(v => (DateTime?)v.Fecha);

        // Si no hay ventas, utilizamos una fecha base.
        var fechaBase = ultimaFecha
            ?? new DateTime(
                2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Elegimos un día posterior, con margen.
        var diaArgentina = fechaBase.Date.AddDays(10);

        return DateTime.SpecifyKind(
            diaArgentina,
            DateTimeKind.Unspecified);
    }
}
