
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Infrastructure.Informes;

public sealed class GeneradorInformeInventarioPdf
    : IGeneradorInformeInventarioPdf
{
    public byte[] Generar(
        IReadOnlyList<InventarioProductoDto> productos)
    {
        var cultura = CultureInfo.GetCultureInfo("es-AR");

        var zona = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Argentina/Buenos_Aires");

        var fecha = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, zona);

        var sinStock = productos.Count(p => p.StockActual == 0);
        var stockBajo = productos.Count(p =>
            p.StockActual > 0 && p.StockActual <= p.StockMinimo);

        var valorTotal = productos.Sum(p => p.ValorPotencialVenta);

        return Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4.Landscape());
                pagina.Margin(25);
                pagina.DefaultTextStyle(e => e.FontSize(9));

                pagina.Header().Column(col =>
                {
                    col.Item().Text("SistemaVenta")
                        .FontSize(18).Bold();

                    col.Item().Text("Informe de inventario actual")
                        .FontSize(13);

                    col.Item().Text(
                        $"Generado: {fecha:dd/MM/yyyy HH:mm}");

                    col.Item().PaddingTop(8).Text(
                        $"Productos: {productos.Count} | " +
                        $"Sin stock: {sinStock} | " +
                        $"Stock bajo: {stockBajo}");

                    col.Item().Text(
                        $"Valor potencial de venta: " +
                        $"{valorTotal.ToString("C2", cultura)}");
                });

                pagina.Content().PaddingVertical(15).Column(col =>
                {
                    if (productos.Count == 0)
                    {
                        col.Item().Text(
                            "No hay productos activos para mostrar.");
                        return;
                    }

                    col.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1.5f);
                            cols.RelativeColumn(0.7f);
                            cols.RelativeColumn(0.7f);
                            cols.RelativeColumn(1.2f);
                            cols.RelativeColumn(1.4f);
                            cols.RelativeColumn(1);
                        });

                        tabla.Header(header =>
                        {
                            foreach (var titulo in new[]
                            {
                                "Código", "Producto", "Categoría",
                                "Stock", "Mínimo", "Precio venta",
                                "Valor potencial", "Estado"
                            })
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text(titulo).Bold();
                            }
                        });

                        foreach (var p in productos)
                        {
                            Celda(tabla.Cell(), p.CodigoInterno);
                            Celda(tabla.Cell(), p.Nombre);
                            Celda(tabla.Cell(), p.Categoria);
                            Celda(tabla.Cell(),
                                p.StockActual.ToString("N0", cultura));
                            Celda(tabla.Cell(),
                                p.StockMinimo.ToString("N0", cultura));
                            Celda(tabla.Cell(),
                                p.PrecioVenta.ToString("C2", cultura));
                            Celda(tabla.Cell(),
                                p.ValorPotencialVenta.ToString("C2", cultura));
                            Celda(tabla.Cell(), p.EstadoStock);
                        }
                    });

                    col.Item().PaddingTop(10).Text(
                        "El valor potencial utiliza precios de venta, " +
                        "no costos de adquisición.")
                        .FontSize(8).Italic();
                });

                pagina.Footer().AlignRight().Text(texto =>
                {
                    texto.Span("Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void Celda(
        IContainer contenedor,
        string valor)
    {
        contenedor.BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(4)
            .Text(valor ?? "-");
    }
}
