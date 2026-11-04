
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.Informes;
using Xunit;
using SistemaVenta.Application.DTOs.Informes;
using System.Collections.Generic;
using SistemaVenta.Application.Tests.Fakes;


namespace SistemaVenta.Application.Tests.UseCases.Informes
{
    public class ObtenerVentasDiariasTests
    {
        [Fact]
        public async Task EjecutarAsync_ConPeriodoInvalido_LanzaArgumentException()
        {
            // Arrange
            IInformeVentasRepository repositorio = null!;

            var casoDeUso = new ObtenerVentasDiarias(repositorio);

            var fechaDesde = new DateTime(
                2026, 11, 10, 3, 0, 0, DateTimeKind.Utc);

            var fechaHasta = new DateTime(
                2026, 11, 5, 3, 0, 0, DateTimeKind.Utc);

            // Act y Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => casoDeUso.EjecutarAsync(
                    fechaDesde,
                    fechaHasta));
        }


        [Fact]
        public async Task EjecutarAsync_ConDiasSinVentas_CompletaConCeros()
        {
            // Arrange
            var repositorio = new FakeInformeVentasRepository();

            var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
                "America/Argentina/Buenos_Aires");

            var fechaDesde = TimeZoneInfo.ConvertTimeToUtc(
                new DateTime(2026, 11, 1),
                zonaArgentina);

            var fechaHasta = TimeZoneInfo.ConvertTimeToUtc(
                new DateTime(2026, 11, 4),
                zonaArgentina).AddTicks(-1);

            repositorio.VentasDiarias = new List<VentaDiariaDto>
    {
        new VentaDiariaDto
        {
            Fecha = new DateTime(2026, 11, 2),
            CantidadVentas = 2,
            TotalVendido = 50000m
        }
    };

            var casoDeUso = new ObtenerVentasDiarias(repositorio);

            // Act
            var resultado = await casoDeUso.EjecutarAsync(
                fechaDesde,
                fechaHasta);

            // Assert
            Assert.Equal(3, resultado.Count);

            // 1 de noviembre: sin ventas
            Assert.Equal(new DateTime(2026, 11, 1), resultado[0].Fecha);
            Assert.Equal(0, resultado[0].CantidadVentas);
            Assert.Equal(0m, resultado[0].TotalVendido);

            // 2 de noviembre: con ventas
            Assert.Equal(new DateTime(2026, 11, 2), resultado[1].Fecha);
            Assert.Equal(2, resultado[1].CantidadVentas);
            Assert.Equal(50000m, resultado[1].TotalVendido);

            // 3 de noviembre: sin ventas
            Assert.Equal(new DateTime(2026, 11, 3), resultado[2].Fecha);
            Assert.Equal(0, resultado[2].CantidadVentas);
            Assert.Equal(0m, resultado[2].TotalVendido);

            // El repositorio debe consultarse una sola vez
            Assert.Equal(1, repositorio.CantidadConsultas);
        }


        [Fact]
        public async Task EjecutarAsync_ConVentasEnVariosDias_ConservaLosDatos()
        {
            // Arrange
            var repositorio = new FakeInformeVentasRepository();

            var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
                "America/Argentina/Buenos_Aires");

            var fechaDesde = TimeZoneInfo.ConvertTimeToUtc(
                new DateTime(2026, 11, 1),
                zonaArgentina);

            var fechaHasta = TimeZoneInfo.ConvertTimeToUtc(
                new DateTime(2026, 11, 4),
                zonaArgentina).AddTicks(-1);

            repositorio.VentasDiarias = new List<VentaDiariaDto>
    {
        new VentaDiariaDto
        {
            Fecha = new DateTime(2026, 11, 1),
            CantidadVentas = 3,
            TotalVendido = 75000m
        },
        new VentaDiariaDto
        {
            Fecha = new DateTime(2026, 11, 2),
            CantidadVentas = 2,
            TotalVendido = 50000m
        },
        new VentaDiariaDto
        {
            Fecha = new DateTime(2026, 11, 3),
            CantidadVentas = 4,
            TotalVendido = 120000m
        }
    };

            var casoDeUso = new ObtenerVentasDiarias(repositorio);

            // Act
            var resultado = await casoDeUso.EjecutarAsync(
                fechaDesde,
                fechaHasta);

            // Assert
            Assert.Equal(3, resultado.Count);

            Assert.Equal(new DateTime(2026, 11, 1), resultado[0].Fecha);
            Assert.Equal(3, resultado[0].CantidadVentas);
            Assert.Equal(75000m, resultado[0].TotalVendido);

            Assert.Equal(new DateTime(2026, 11, 2), resultado[1].Fecha);
            Assert.Equal(2, resultado[1].CantidadVentas);
            Assert.Equal(50000m, resultado[1].TotalVendido);

            Assert.Equal(new DateTime(2026, 11, 3), resultado[2].Fecha);
            Assert.Equal(4, resultado[2].CantidadVentas);
            Assert.Equal(120000m, resultado[2].TotalVendido);

            Assert.Equal(9, resultado.Sum(v => v.CantidadVentas));
            Assert.Equal(245000m, resultado.Sum(v => v.TotalVendido));

            Assert.Equal(1, repositorio.CantidadConsultas);
        }



    }
}
