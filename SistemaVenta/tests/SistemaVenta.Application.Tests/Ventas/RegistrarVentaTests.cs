
using System.Reflection;
using SistemaVenta.Application.DTOs.Ventas;
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using Xunit;

namespace SistemaVenta.Application.Tests.Ventas
{
    public class RegistrarVentaTests
    {
        private readonly FakeVentaRepository _ventas = new();
        private readonly FakeClienteRepository _clientes = new();
        private readonly FakeProductoRepository _productos = new();
        private readonly FakeMovimientoStockRepository _movimientos = new();
        private readonly FakeUnitOfWork _unitOfWork = new();

        private RegistrarVenta CrearCasoDeUso()
        {
            return new RegistrarVenta(
                _ventas,
                _clientes,
                _productos,
                _movimientos,
                _unitOfWork);
        }

        private static Producto CrearProducto(
            int id,
            string codigo,
            decimal precio,
            decimal stock)
        {
            var categoria = new Categoria(
                "Periféricos",
                "Productos periféricos");

            var producto = new Producto(
                codigoInterno: codigo,
                nombre: codigo,
                precioVenta: precio,
                costo: 1000m,
                stockMinimo: 1m,
                categoria: categoria);

            producto.AumentarStock(stock);

            var propiedad = typeof(Producto).GetProperty(
                nameof(Producto.Id),
                BindingFlags.Instance | BindingFlags.Public);

            propiedad!.SetValue(producto, id);

            return producto;
        }


        private void ConfigurarClienteActivo()
        {
            var cliente = new ClienteFisico(
                nombre: "Juan",
                apellido: "Pérez",
                dni: "12345678",
                telefono: "3811234567",
                email: "juan@test.com",
                direccion: "Calle 123",
                cuit: null!,
                fechaNacimiento: null,
                observaciones: null);

            ClienteTestFactory.ConId(cliente, 1);

            _clientes.ConfigurarCliente(1, cliente);
        }


        [Fact]
        public async Task RegistrarVenta_Valida_ConfirmaVenta()
        {
            ConfigurarClienteActivo();

            var producto = CrearProducto(
                1, "TEC-001", 10000m, 10m);

            _productos.ConfigurarProducto(1, producto);

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
                {
                    new()
                    {
                        ProductoId = 1,
                        Cantidad = 2m
                    }
                }
            };

            await CrearCasoDeUso().EjecutarAsync(solicitud);

            var venta = Assert.Single(_ventas.Ventas);

            Assert.Equal(EstadoVenta.Confirmada, venta.Estado);
            Assert.Equal(20000m, venta.Total);
            Assert.Equal(8m, producto.StockActual);
            Assert.True(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task RegistrarVenta_GeneraMovimientoStock()
        {
            ConfigurarClienteActivo();

            var producto = CrearProducto(
                1, "TEC-001", 10000m, 10m);

            _productos.ConfigurarProducto(1, producto);

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
                {
                    new()
                    {
                        ProductoId = 1,
                        Cantidad = 3m
                    }
                }
            };

            await CrearCasoDeUso().EjecutarAsync(solicitud);

            var movimiento = Assert.Single(_movimientos.Movimientos);

            Assert.Equal(TipoMovimientoStock.Venta, movimiento.Tipo);
            Assert.Equal(3m, movimiento.Cantidad);
            Assert.Equal(10m, movimiento.StockAnterior);
            Assert.Equal(7m, movimiento.StockPosterior);
        }

        [Fact]
        public async Task RegistrarVenta_ConVariosProductos_DescuentaStock()
        {
            ConfigurarClienteActivo();

            var producto1 = CrearProducto(
                1, "TEC-001", 10000m, 10m);

            var producto2 = CrearProducto(
                2, "MOU-001", 5000m, 8m);

            _productos.ConfigurarProducto(1, producto1);
            _productos.ConfigurarProducto(2, producto2);

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
                {
                    new() { ProductoId = 1, Cantidad = 2m },
                    new() { ProductoId = 2, Cantidad = 3m }
                }
            };

            await CrearCasoDeUso().EjecutarAsync(solicitud);

            var venta = Assert.Single(_ventas.Ventas);

            Assert.Equal(35000m, venta.Total);
            Assert.Equal(2, venta.Detalles.Count);

            Assert.Equal(8m, producto1.StockActual);
            Assert.Equal(5m, producto2.StockActual);

            Assert.Equal(2, _movimientos.Movimientos.Count);
        }

        [Fact]
        public async Task RegistrarVenta_ClienteInexistente_NoGuardaVenta()
        {
            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 99,
                Detalles = new()
                {
                    new() { ProductoId = 1, Cantidad = 1m }
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearCasoDeUso().EjecutarAsync(solicitud));

            Assert.Empty(_ventas.Ventas);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task RegistrarVenta_StockInsuficiente_NoModificaInventario()
        {
            ConfigurarClienteActivo();

            var producto = CrearProducto(
                1, "TEC-001", 10000m, 5m);

            _productos.ConfigurarProducto(1, producto);

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
                {
                    new() { ProductoId = 1, Cantidad = 10m }
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearCasoDeUso().EjecutarAsync(solicitud));

            Assert.Equal(5m, producto.StockActual);
            Assert.Empty(_ventas.Ventas);
            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task RegistrarVenta_ProductoRepetido_NoGuardaVenta()
        {
            ConfigurarClienteActivo();

            var producto = CrearProducto(
                1, "TEC-001", 10000m, 10m);

            _productos.ConfigurarProducto(1, producto);

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
                {
                    new() { ProductoId = 1, Cantidad = 1m },
                    new() { ProductoId = 1, Cantidad = 2m }
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => CrearCasoDeUso().EjecutarAsync(solicitud));

            Assert.Equal(10m, producto.StockActual);
            Assert.Empty(_ventas.Ventas);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }


        [Fact]
        public async Task RegistrarVenta_ClienteInactivo_NoGuardaVenta()
        {
            ConfigurarClienteActivo();

            var cliente = await _clientes.ObtenerPorIdAsync(1);
            cliente!.Desactivar();

            var producto = CrearProducto(
                1, "TEC-001", 10000m, 10m);

            _productos.ConfigurarProducto(1, producto);

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
        {
            new() { ProductoId = 1, Cantidad = 2m }
        }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearCasoDeUso().EjecutarAsync(solicitud));

            Assert.Equal(10m, producto.StockActual);
            Assert.Empty(_ventas.Ventas);
            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task RegistrarVenta_ProductoInexistente_NoGuardaVenta()
        {
            ConfigurarClienteActivo();

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
        {
            new() { ProductoId = 99, Cantidad = 1m }
        }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearCasoDeUso().EjecutarAsync(solicitud));

            Assert.Empty(_ventas.Ventas);
            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task RegistrarVenta_ProductoInactivo_NoGuardaVenta()
        {
            ConfigurarClienteActivo();

            var producto = CrearProducto(
                1, "TEC-001", 10000m, 10m);

            producto.Desactivar();
            _productos.ConfigurarProducto(1, producto);

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
        {
            new() { ProductoId = 1, Cantidad = 2m }
        }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearCasoDeUso().EjecutarAsync(solicitud));

            Assert.Equal(10m, producto.StockActual);
            Assert.Empty(_ventas.Ventas);
            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task RegistrarVenta_CantidadInvalida_NoGuardaVenta(
            decimal cantidad)
        {
            ConfigurarClienteActivo();

            var producto = CrearProducto(
                1, "TEC-001", 10000m, 10m);

            _productos.ConfigurarProducto(1, producto);

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
        {
            new() { ProductoId = 1, Cantidad = cantidad }
        }
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => CrearCasoDeUso().EjecutarAsync(solicitud));

            Assert.Equal(10m, producto.StockActual);
            Assert.Empty(_ventas.Ventas);
            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task RegistrarVenta_SinDetalles_NoGuardaVenta()
        {
            ConfigurarClienteActivo();

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => CrearCasoDeUso().EjecutarAsync(solicitud));

            Assert.Empty(_ventas.Ventas);
            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }

        [Fact]
        public async Task RegistrarVenta_ProductoPosteriorSinStock_NoDescuentaNinguno()
        {
            ConfigurarClienteActivo();

            var producto1 = CrearProducto(
                1, "TEC-001", 10000m, 10m);

            var producto2 = CrearProducto(
                2, "MOU-001", 5000m, 2m);

            _productos.ConfigurarProducto(1, producto1);
            _productos.ConfigurarProducto(2, producto2);

            var solicitud = new RegistrarVentaDto
            {
                ClienteId = 1,
                Detalles = new()
        {
            new() { ProductoId = 1, Cantidad = 3m },
            new() { ProductoId = 2, Cantidad = 5m }
        }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearCasoDeUso().EjecutarAsync(solicitud));

            Assert.Equal(10m, producto1.StockActual);
            Assert.Equal(2m, producto2.StockActual);

            Assert.Empty(_ventas.Ventas);
            Assert.Empty(_movimientos.Movimientos);
            Assert.False(_unitOfWork.GuardarCambiosFueLlamado);
        }


    }
}
