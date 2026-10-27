using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentosComercialesYPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FormaPago = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosComerciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TipoDocumento = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    VentaId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ImporteTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FacturaId = table.Column<int>(type: "integer", nullable: true),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NotaCredito_ImporteTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    NotaDebito_FacturaId = table.Column<int>(type: "integer", nullable: true),
                    NotaDebito_Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NotaDebito_ImporteTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PagoId = table.Column<int>(type: "integer", nullable: true),
                    Recibo_ImporteTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Ticket_VentaId = table.Column<int>(type: "integer", nullable: true),
                    Ticket_Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Ticket_ImporteTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosComerciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosComerciales_DocumentosComerciales_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "DocumentosComerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosComerciales_DocumentosComerciales_NotaDebito_Fact~",
                        column: x => x.NotaDebito_FacturaId,
                        principalTable: "DocumentosComerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosComerciales_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosComerciales_Ventas_Ticket_VentaId",
                        column: x => x.Ticket_VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosComerciales_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AplicacionesPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PagoId = table.Column<int>(type: "integer", nullable: false),
                    DocumentoComercialId = table.Column<int>(type: "integer", nullable: false),
                    ImporteAplicado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AplicacionesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AplicacionesPago_DocumentosComerciales_DocumentoComercialId",
                        column: x => x.DocumentoComercialId,
                        principalTable: "DocumentosComerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AplicacionesPago_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AplicacionesPago_DocumentoComercialId",
                table: "AplicacionesPago",
                column: "DocumentoComercialId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacionesPago_PagoId",
                table: "AplicacionesPago",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosComerciales_FacturaId",
                table: "DocumentosComerciales",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosComerciales_NotaDebito_FacturaId",
                table: "DocumentosComerciales",
                column: "NotaDebito_FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosComerciales_Numero",
                table: "DocumentosComerciales",
                column: "Numero");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosComerciales_PagoId",
                table: "DocumentosComerciales",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosComerciales_Ticket_VentaId",
                table: "DocumentosComerciales",
                column: "Ticket_VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosComerciales_VentaId",
                table: "DocumentosComerciales",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_UsuarioId",
                table: "Pagos",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AplicacionesPago");

            migrationBuilder.DropTable(
                name: "DocumentosComerciales");

            migrationBuilder.DropTable(
                name: "Pagos");
        }
    }
}
