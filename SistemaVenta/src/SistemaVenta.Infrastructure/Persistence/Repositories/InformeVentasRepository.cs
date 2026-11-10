
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Infrastructure.Persistence.Repositories
{
    public class InformeVentasRepository : IInformeVentasRepository
    {
        private readonly AppDbContext _context;

        public InformeVentasRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InformeVentasPorPeriodoDto> ObtenerVentasPorPeriodoAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            var estadisticas = await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Estado == EstadoVenta.Confirmada &&
                    v.Fecha >= fechaDesde &&
                    v.Fecha <= fechaHasta)
                .Select(v => new
                {
                    Total = v.Detalles.Sum(d =>
                        d.Cantidad * d.PrecioUnitario)
                })
                .GroupBy(v => 1)
                .Select(g => new
                {
                    CantidadVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.Total)
                })
                .FirstOrDefaultAsync();

            var cantidad = estadisticas?.CantidadVentas ?? 0;
            var total = estadisticas?.TotalVendido ?? 0m;

            return new InformeVentasPorPeriodoDto
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                CantidadVentas = cantidad,
                TotalVendido = total,
                TicketPromedio = cantidad > 0
                    ? Math.Round(total / cantidad, 2)
                    : 0m
            };
        }


        public async Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int cantidad,
            string criterio = "cantidad")
        {
            var consulta = _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Estado == EstadoVenta.Confirmada &&
                    v.Fecha >= fechaDesde &&
                    v.Fecha <= fechaHasta)
                .SelectMany(v => v.Detalles)
                .GroupBy(d => new
                {
                    d.ProductoId,
                    d.Producto.Nombre
                })
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.Nombre,
                    UnidadesVendidas = g.Sum(d => d.Cantidad),
                    TotalVendido = g.Sum(d =>
                        d.Cantidad * d.PrecioUnitario)
                });

            var ordenados = criterio == "facturacion"
                ? consulta.OrderByDescending(p => p.TotalVendido)
                    .ThenBy(p => p.NombreProducto)
                : consulta.OrderByDescending(p => p.UnidadesVendidas)
                    .ThenBy(p => p.NombreProducto);

            return await ordenados.Take(cantidad).ToListAsync();
        }

        public async Task<List<VentaDiariaDto>> ObtenerVentasDiariasAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
                "America/Argentina/Buenos_Aires");

            var ventas = await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Estado == EstadoVenta.Confirmada &&
                    v.Fecha >= fechaDesde &&
                    v.Fecha <= fechaHasta)
                .Select(v => new
                {
                    v.Fecha,
                    Total = v.Detalles.Sum(d =>
                        d.Cantidad * d.PrecioUnitario)
                })
                .ToListAsync();

            var resultado = ventas
                .GroupBy(v =>
                    TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(v.Fecha, DateTimeKind.Utc),
                        zonaArgentina).Date)
                .Select(g => new VentaDiariaDto
                {
                    Fecha = g.Key,
                    CantidadVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.Total)
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            return resultado;
        }

        public async Task<List<VentaPorVendedorDto>> ObtenerVentasPorVendedorAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            var estadisticas = await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Estado == EstadoVenta.Confirmada &&
                    v.Fecha >= fechaDesde &&
                    v.Fecha <= fechaHasta)
                .GroupBy(v => v.VendedorId)
                .Select(g => new
                {
                    VendedorId = g.Key,
                    CantidadVentas = g.Count(),
                    TotalVendido = g.Sum(v =>
                        v.Detalles.Sum(d =>
                            d.Cantidad * d.PrecioUnitario))
                })
                .ToListAsync();

            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Select(u => new
                {
                    u.Id,
                    u.NombreCompleto,
                    u.Rol,
                    u.Activo
                })
                .ToListAsync();

            var resultado = estadisticas
                .Select(e =>
                {
                    var usuario = usuarios.FirstOrDefault(
                        u => u.Id == e.VendedorId);

                    return new VentaPorVendedorDto
                    {
                        VendedorId = e.VendedorId,
                        NombreVendedor = usuario?.NombreCompleto
                            ?? "Sin asignar",
                        Rol = usuario?.Rol.ToString()
                            ?? "Sin asignar",
                        Activo = usuario?.Activo,
                        CantidadVentas = e.CantidadVentas,
                        TotalVendido = e.TotalVendido
                    };
                })
                .OrderByDescending(v => v.TotalVendido)
                .ToList();

            return resultado;
        }


        public async Task<List<ClienteTopDto>> ObtenerMejoresClientesAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int cantidad)
        {
            var estadisticas = await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Estado == EstadoVenta.Confirmada &&
                    v.ClienteId != null &&
                    v.Fecha >= fechaDesde &&
                    v.Fecha <= fechaHasta)
                .GroupBy(v => v.ClienteId)
                .Select(g => new
                {
                    ClienteId = g.Key,
                    CantidadVentas = g.Count(),
                    TotalComprado = g.Sum(v =>
                        v.Detalles.Sum(d =>
                            d.Cantidad * d.PrecioUnitario))
                })
                .OrderByDescending(x => x.TotalComprado)
                .ThenBy(x => x.ClienteId)
                .Take(cantidad)
                .ToListAsync();

            var ids = estadisticas
                .Where(e => e.ClienteId.HasValue)
                .Select(e => e.ClienteId!.Value)
                .ToList();

            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            var nombres = clientes.ToDictionary(
                c => c.Id,
                c => c switch
                {
                    ClienteFisico fisico =>
                        $"{fisico.Nombre} {fisico.Apellido}",

                    ClienteEmpresa empresa =>
                        empresa.RazonSocial,

                    _ => $"Cliente #{c.Id}"
                });

            return estadisticas.Select(e =>
            {
                var id = e.ClienteId!.Value;

                return new ClienteTopDto
                {
                    ClienteId = id,

                    NombreCliente = nombres.TryGetValue(
                        id, out var nombre)
                        ? nombre
                        : $"Cliente #{id}",

                    CantidadVentas = e.CantidadVentas,

                    TotalComprado = e.TotalComprado,

                    TicketPromedio = e.CantidadVentas > 0
                        ? Math.Round(
                            e.TotalComprado / e.CantidadVentas, 2)
                        : 0m
                };
            }).ToList();
        }


    }
}
