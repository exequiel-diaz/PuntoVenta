
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Infrastructure.Informes;

public sealed class GeneradorInformeMovimientosPdf
    : IGeneradorInformeMovimientosPdf
{
    public byte[] Generar(
        IReadOnlyList<MovimientoStockInformeDto> movimientos,
        DateTime fechaDesde,
        DateTime fechaHasta)
    {
        var cultura = new CultureInfo("es-AR");

        var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var fechaGeneracion = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            zonaArgentina);

        return Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4.Landscape());
                pagina.Margin(25);
                pagina.DefaultTextStyle(
                    estilo => estilo.FontSize(8));

                pagina.Header().Column(columna =>
                {
                    columna.Item().Text("SistemaVenta")
                        .FontSize(18)
                        .Bold();

                    columna.Item()
                        .Text("Informe de movimientos de stock")
                        .FontSize(13);

                    columna.Item().Text(
                        $"Período: {fechaDesde:dd/MM/yyyy} " +
                        $"- {fechaHasta:dd/MM/yyyy}");

                    columna.Item().Text(
                        $"Generado: {fechaGeneracion:dd/MM/yyyy HH:mm}");

                    columna.Item().PaddingTop(5)
                        .Text($"Total de movimientos: {movimientos.Count}");
                });

                pagina.Content().PaddingVertical(15).Column(columna =>
                {
                    if (movimientos.Count == 0)
                    {
                        columna.Item().Text(
                            "No se encontraron movimientos para el período seleccionado.");
                        return;
                    }

                    columna.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            columnas.ConstantColumn(75);
                            columnas.RelativeColumn(1);
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(1);
                            columnas.ConstantColumn(45);
                            columnas.ConstantColumn(50);
                            columnas.ConstantColumn(50);
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(1);
                        });

                        tabla.Header(encabezado =>
                        {
                            Encabezado(encabezado.Cell(), "Fecha");
                            Encabezado(encabezado.Cell(), "Código");
                            Encabezado(encabezado.Cell(), "Producto");
                            Encabezado(encabezado.Cell(), "Tipo");
                            Encabezado(encabezado.Cell(), "Cantidad");
                            Encabezado(encabezado.Cell(), "Anterior");
                            Encabezado(encabezado.Cell(), "Posterior");
                            Encabezado(encabezado.Cell(), "Motivo");
                            Encabezado(encabezado.Cell(), "Responsable");
                            Encabezado(encabezado.Cell(), "Operación");
                        });

                        foreach (var movimiento in movimientos)
                        {
                            var fechaLocal =
                                TimeZoneInfo.ConvertTimeFromUtc(
                                    DateTime.SpecifyKind(
                                        movimiento.Fecha,
                                        DateTimeKind.Utc),
                                    zonaArgentina);

                            Celda(tabla.Cell(),
                                fechaLocal.ToString("dd/MM/yy HH:mm"));

                            Celda(tabla.Cell(),
                                movimiento.CodigoInterno);

                            Celda(tabla.Cell(),
                                movimiento.Producto);

                            Celda(tabla.Cell(),
                                movimiento.Tipo);

                            Celda(tabla.Cell(),
                                movimiento.Cantidad.ToString("N0", cultura));

                            Celda(tabla.Cell(),
                                movimiento.StockAnterior.ToString("N0", cultura));

                            Celda(tabla.Cell(),
                                movimiento.StockPosterior.ToString("N0", cultura));

                            Celda(tabla.Cell(),
                                string.IsNullOrWhiteSpace(movimiento.Motivo)
                                    ? "-"
                                    : movimiento.Motivo);

                            Celda(tabla.Cell(),
                                movimiento.Responsable);

                            Celda(tabla.Cell(),
                                movimiento.VentaId.HasValue
                                    ? $"Venta #{movimiento.VentaId.Value}"
                                    : "-");
                        }
                    });
                });

                pagina.Footer()
                    .AlignRight()
                    .Text(texto =>
                    {
                        texto.Span("Página ");
                        texto.CurrentPageNumber();
                        texto.Span(" de ");
                        texto.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    private static void Encabezado(
        IContainer contenedor,
        string texto)
    {
        contenedor
            .Background(Colors.Grey.Lighten2)
            .Padding(4)
            .Text(texto)
            .Bold();
    }

    private static void Celda(
        IContainer contenedor,
        string? texto)
    {
        contenedor
            .BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(4)
            .Text(texto ?? "");
    }
}
