
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Infrastructure.Persistence.Repositories;

public class InformeInventarioRepository
    : IInformeInventarioRepository
{
    private readonly AppDbContext _context;

    public InformeInventarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventarioProductoDto>>
        ObtenerInventarioActualAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Where(p => p.Estado == EstadoProducto.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new InventarioProductoDto
            {
                ProductoId = p.Id,
                CodigoInterno = p.CodigoInterno,
                Nombre = p.Nombre,
                Categoria = p.Categoria.Nombre,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                PrecioVenta = p.PrecioVenta
            })
            .ToListAsync();
    }

    public async Task<List<MovimientoStockInformeDto>>
        ObtenerMovimientosAsync(
            DateTime fechaDesdeUtc,
            DateTime fechaHastaUtcExclusiva)
    {
        var registros = await _context.MovimientosStock
            .AsNoTracking()
            .Where(m =>
                m.Fecha >= fechaDesdeUtc &&
                m.Fecha < fechaHastaUtcExclusiva)
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.Id)
            .Select(m => new
            {
                m.Id,
                m.Fecha,
                m.Tipo,
                m.Cantidad,
                m.StockAnterior,
                m.StockPosterior,
                m.Motivo,
                m.UsuarioId,
                m.VentaId,

                Responsable = m.Usuario != null
                    ? m.Usuario.NombreCompleto
                    : "No registrado",

                CodigoInterno = m.Producto.CodigoInterno,
                Producto = m.Producto.Nombre
            })
            .ToListAsync();

        return registros.Select(m => new MovimientoStockInformeDto
        {
            Id = m.Id,
            UsuarioId = m.UsuarioId,
            Responsable = m.Responsable,
            VentaId = m.VentaId,
            Fecha = m.Fecha,
            Tipo = m.Tipo.ToString(),
            Cantidad = m.Cantidad,
            StockAnterior = m.StockAnterior,
            StockPosterior = m.StockPosterior,
            Motivo = m.Motivo ?? "",
            CodigoInterno = m.CodigoInterno,
            Producto = m.Producto
        }).ToList();
    }
}
