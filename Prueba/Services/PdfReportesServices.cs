using Prueba.Context;
using Prueba.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NetTopologySuite.Index.HPRtree;
using Prueba.Models;

namespace Prueba.Services
{
    public interface IPdfReportesServices
    {
        Task<byte[]> Deudores(RecibosCreadosVM modelo, int id);
        Task<byte[]> DeudoresResumen(RecibosCreadosVM modelo, int id);
        byte[] EstadoCuentas(List<EstadoCuentasVM> modelo);
        Task<byte[]> ReporteCompIslr(IEnumerable<ComprobanteRetencion> comprobantes, int id);
        Task<byte[]> ReporteCompIva(IEnumerable<CompRetIva> comprobantes, int id);
    }

    public class PdfReportesServices : IPdfReportesServices
    {
        private readonly NuevaAppContext _context;

        public PdfReportesServices(NuevaAppContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Deudores(RecibosCreadosVM modelo, int id)
        {
            var condominio = await _context.Condominios.FindAsync(id);
            decimal totalDeuda = 0;
            decimal totalIntereses = 0;
            decimal totalMulta = 0;
            decimal totalCredito = 0;
            decimal totalSaldo = 0;
            decimal totalPagar = 0;

            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(10).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + condominio.Nombre).FontSize(10).FontColor("#004581").Bold();
                            //col.Item().Text("Relación de Gastos").FontSize(10).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignCenter().Text("Deudores día: " + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(10).FontColor("#004581").Bold();
                            x.Spacing(20);
                            x.Item().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();

                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();

                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Código").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Propietario").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Cont. Recibos").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Acumulado Deuda").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Acumulado Mora 1%").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Acumulado Multa 30%").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Crédito").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Saldo Actual").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total a Pagar").FontColor("#607080").Bold().FontSize(8);

                                });


                                foreach (var propiedad in modelo.Propiedades)
                                {
                                    var propietario = modelo.Propietarios.First(c => c.Id == propiedad.IdUsuario);
                                    var recibos = modelo.Recibos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToList();

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text(propiedad.Codigo).FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(propietario.FirstName).FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(recibos.Any() ? recibos.Count.ToString() : "").FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(propiedad.Deuda.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(propiedad.MontoIntereses.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9")
                                    .Padding(5).Text(((decimal)propiedad.MontoMulta).ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(((decimal)propiedad.Creditos).ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(propiedad.Saldo.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text((propiedad.Deuda + propiedad.MontoIntereses + (decimal)propiedad.MontoMulta - (decimal)propiedad.Creditos + propiedad.Saldo).ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                    totalSaldo += propiedad.Saldo;
                                    totalDeuda += propiedad.Deuda;
                                    totalMulta += (decimal)propiedad.MontoMulta;
                                    totalIntereses += propiedad.MontoIntereses;
                                    totalCredito += (decimal)propiedad.Creditos;
                                    totalPagar += propiedad.Deuda + propiedad.MontoIntereses + (decimal)propiedad.MontoMulta - (decimal)propiedad.Creditos + propiedad.Saldo;
                                }

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("Totales").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalDeuda.ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalIntereses.ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9")
                                .Padding(5).Text(totalMulta.ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalCredito.ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalSaldo.ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalPagar.ToString("N")).Bold().FontColor("#607080").FontSize(8);

                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology").FontSize(8);
                        });
                });
            })
         .GeneratePdf();
            return data;
        }

        public async Task<byte[]> DeudoresResumen(RecibosCreadosVM modelo, int id)
        {
            var condominio = await _context.Condominios.FindAsync(id);
            decimal totalAcumulado = 0;
            decimal totalSaldo = 0;
            decimal totalMontosPagar = 0;

            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(10).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + condominio.Nombre).FontSize(10).FontColor("#004581").Bold();
                            //col.Item().Text("Relación de Gastos").FontSize(10).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().AlignCenter().Text("Deudores día: " + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(10).FontColor("#004581").Bold();
                            x.Item().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();

                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();

                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();

                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Código").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Propietario").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Cant. Recibos").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Acumulado Deuda").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Saldo Actual").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total a Pagar").FontColor("#607080").Bold().FontSize(8);

                                });


                                foreach (var propiedad in modelo.Propiedades)
                                {
                                    var propietario = modelo.Propietarios.First(c => c.Id == propiedad.IdUsuario);
                                    var recibos = modelo.Recibos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToList();

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text(propiedad.Codigo).FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(propietario.FirstName).FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(recibos.Any() ? recibos.Count.ToString() : "").FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(propiedad.Deuda.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(propiedad.Saldo.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text((propiedad.Deuda + propiedad.MontoIntereses + (decimal)propiedad.MontoMulta - (decimal)propiedad.Creditos + propiedad.Saldo).ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                    totalAcumulado += propiedad.Deuda;
                                    totalSaldo += propiedad.Saldo;
                                    totalMontosPagar += propiedad.Deuda + propiedad.MontoIntereses + (decimal)propiedad.MontoMulta - (decimal)propiedad.Creditos + propiedad.Saldo;
                                }

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("Totales").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalAcumulado.ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalSaldo.ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalMontosPagar.ToString("N")).Bold().FontColor("#607080").FontSize(8);

                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology").FontSize(8);
                        });
                });
            })
         .GeneratePdf();
            return data;
        }

        public byte[] EstadoCuentas(List<EstadoCuentasVM> modelo)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.Header().Row(row =>
                    {
                        var uno = modelo.First().Condominio;

                        row.RelativeItem().Padding(3).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + uno.Nombre).FontSize(10).FontColor("#004581").Bold();
                            //col.Item().Text("Relación de Gastos").FontSize(10).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Padding(3).Column(col =>
                        {
                            //col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/imagSes/yllenAzul.png");
                            //col.Item().BorderBottom(1).PaddingBottom(5).AlignCenter().Text("AVISO DE COBRO").FontSize(8).FontColor("#004581").Bold();
                            //col.Item().Text("Oficina: " + modelo.Propiedad.Codigo).FontSize(8).FontColor("#004581").Bold();
                            //col.Item().Text("Propietario: " + propietario.FirstName).FontSize(8).FontColor("#004581").Bold();
                            col.Item().Text("Fecha de emisión: " + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(8).FontColor("#004581").Bold();
                            //col.Item().Text("Mes: " + modelo.RelacionGasto.Mes).FontSize(8).FontColor("#004581").Bold();
                            col.Item().Text(text =>
                            {
                                text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor("#004581").Bold());
                                text.CurrentPageNumber();
                                text.Span(" / ");
                                text.TotalPages();
                            });
                        });
                    });
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(2);
                            x.Item().AlignCenter().Text("Estado de Cuentas Oficinas").Bold().FontSize(10).FontColor("#004581");

                            decimal totalSaldoGlobal = 0;

                            foreach (var item in modelo.Where(item => item.ReciboCobro.Any()))
                            {
                                x.Item().AlignLeft().Text("Oficina: " + item.Propiedad.Codigo + " " + item.User.FirstName).Bold().FontSize(8).FontColor("#004581");

                                x.Item().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").Table(tabla =>
                                {
                                    tabla.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        //detalle
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        //monto
                                        columns.RelativeColumn();
                                        // intereses 1%
                                        columns.RelativeColumn();
                                        // multa 30%
                                        columns.RelativeColumn();
                                        // abono
                                        columns.RelativeColumn();
                                        // credito
                                        columns.RelativeColumn();
                                        // saldo
                                        columns.RelativeColumn();
                                    });

                                    tabla.Header(header =>
                                    {
                                        header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("Fecha Emisión").FontColor("#607080").Bold().FontSize(8);

                                        header.Cell().ColumnSpan(3).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("Detalle").FontColor("#607080").Bold().FontSize(8);

                                        header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Monto").FontColor("#607080").Bold().FontSize(8);

                                        header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Intereses").FontColor("#607080").Bold().FontSize(8);

                                        header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Indexación").FontColor("#607080").Bold().FontSize(8);

                                        header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Abono").FontColor("#607080").Bold().FontSize(8);

                                        header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Crédito").FontColor("#607080").Bold().FontSize(8);

                                        header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Saldo").FontColor("#607080").Bold().FontSize(8);
                                    });

                                    decimal totalMonto = 0;
                                    decimal totalInteres = 0;
                                    decimal totalMulta = 0;
                                    decimal totalAbono = 0;

                                    foreach (var recibo in item.ReciboCobro)
                                    {
                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text(recibo.Fecha.ToString("dd/MM/yyyy")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().ColumnSpan(3).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text("Condominio Mes: " + recibo.Mes).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text(recibo.Monto.ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text((recibo.ReciboActual ? 0 : recibo.MontoMora).ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text((recibo.ReciboActual ? 0 : recibo.MontoIndexacion).ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text(recibo.Abonado.ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                        totalMonto += recibo.Monto;
                                        totalInteres += recibo.ReciboActual ? 0 : recibo.MontoMora;
                                        totalMulta += recibo.ReciboActual ? 0 : recibo.MontoIndexacion;
                                        totalAbono += recibo.Abonado;
                                    }

                                    var saldo = totalMonto + totalInteres + totalMulta - totalAbono - (decimal)item.Propiedad.Creditos;
                                    totalSaldoGlobal += saldo;

                                    tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().ColumnSpan(3).BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("Total: ").FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(totalMonto.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(totalInteres.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(totalMulta.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(totalAbono.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(((decimal)item.Propiedad.Creditos).ToString("N")).FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(saldo.ToString("N"))
                                     .FontColor("#607080")
                                     .Bold()
                                     .FontSize(8);


                                });

                            }


                            x.Item().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    //detalle
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    //monto
                                    columns.RelativeColumn();
                                    // intereses 1%
                                    columns.RelativeColumn();
                                    // multa 30%
                                    columns.RelativeColumn();
                                    // abono
                                    columns.RelativeColumn();
                                    // credito
                                    columns.RelativeColumn();
                                    // saldo
                                    columns.RelativeColumn();
                                });

                                tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().ColumnSpan(3).BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Total Saldo Global: ").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().ColumnSpan(5).BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);


                                tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(totalSaldoGlobal.ToString("N"))
                                 .FontColor("#607080")
                                 .Bold()
                                 .FontSize(8);
                            });

                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {

                            x.Span("Software desarrollado por: Password Technology").Bold().FontSize(8).FontColor("#004581");
                        });
                });
            })
         .GeneratePdf();

            return data;
        }

        public async Task<byte[]> ReporteCompIva(IEnumerable<CompRetIva> comprobantes, int id)
        {
            var condominio = await _context.Condominios.FindAsync(id);
            decimal totalRetenido = 0;

            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(10).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + (condominio != null ? condominio.Nombre : "")).FontSize(10).FontColor("#004581").Bold();
                            //col.Item().Text("Relación de Gastos").FontSize(10).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignCenter().Text("Comprobantes de Retención de I.V.A").FontSize(10).FontColor("#004581").Bold();
                            x.Spacing(20);
                            x.Item().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    // FECHA
                                    columns.RelativeColumn();
                                    // NOMBRE PROVEEDOR
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // # FACTURA
                                    columns.RelativeColumn();
                                    // # CONTROL
                                    columns.RelativeColumn();
                                    // # COMPROBANTE
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // MONTO RETENIDO
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha Emisión").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Proveedor").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Nro. Factura").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Nro. Control").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Nro. Comprobante").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Monto Retenido").FontColor("#607080").Bold().FontSize(8);                             

                                });


                                foreach (var comprobante in comprobantes)
                                {                                   

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text(comprobante.FechaEmision.ToString("dd/MM/yyyy")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(comprobante.IdProveedorNavigation.Nombre).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(comprobante.IdFacturaNavigation.NumFactura.ToString()).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(comprobante.IdFacturaNavigation.NumControl.ToString()).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(comprobante.NumCompRet).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9")
                                    .Padding(5).Text(comprobante.IvaRetenido.ToString("N")).FontColor("#607080").FontSize(8);

                                    totalRetenido += comprobante.IvaRetenido;
                                }

                                tabla.Cell().ColumnSpan(8).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Total Retenido").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(totalRetenido.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology").FontColor("#607080").Bold().FontSize(8);
                        });
                });
            })
         .GeneratePdf();
            return data;
        }

        public async Task<byte[]> ReporteCompIslr(IEnumerable<ComprobanteRetencion> comprobantes, int id)
        {
            var condominio = await _context.Condominios.FindAsync(id);
            decimal totalRetenido = 0;

            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(10).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + (condominio != null ? condominio.Nombre : "")).FontSize(10).FontColor("#004581").Bold();
                            //col.Item().Text("Relación de Gastos").FontSize(10).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignCenter().Text("Comprobantes de Retención de ISLR").FontSize(10).FontColor("#004581").Bold();
                            x.Spacing(20);
                            x.Item().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    // FECHA
                                    columns.RelativeColumn();
                                    // NOMBRE PROVEEDOR
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // # FACTURA
                                    columns.RelativeColumn();
                                    // # CONTROL
                                    columns.RelativeColumn();
                                    // # COMPROBANTE
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // MONTO RETENIDO
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha Emisión").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Proveedor").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Nro. Factura").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Nro. Control").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Nro. Comprobante").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Monto Retenido").FontColor("#607080").Bold().FontSize(8);

                                });


                                foreach (var comprobante in comprobantes)
                                {

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text(comprobante.FechaEmision.ToString("dd/MM/yyyy")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(comprobante.IdProveedorNavigation.Nombre).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(comprobante.IdFacturaNavigation.NumFactura.ToString()).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(comprobante.IdFacturaNavigation.NumControl.ToString()).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(comprobante.NumCompRet).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9")
                                    .Padding(5).Text(comprobante.ValorRetencion.ToString("N")).FontColor("#607080").FontSize(8);

                                    totalRetenido += comprobante.ValorRetencion;
                                }

                                tabla.Cell().ColumnSpan(8).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Total Retenido").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(totalRetenido.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology").FontColor("#607080").Bold().FontSize(8);
                        });
                });
            })
         .GeneratePdf();
            return data;
        }
    }
}
