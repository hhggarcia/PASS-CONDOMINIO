using Prueba.Context;
using Prueba.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NetTopologySuite.Index.HPRtree;
using Prueba.Models;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Prueba.Services
{
    public interface IPdfReportesServices
    {
        Task<byte[]> ConciliacionPDF(ItemConciliacionVM modelo);
        byte[] CuentasCobrarPDF(List<CuentasCobrarVM> modelo);
        byte[] CuentasPagarPDF(List<CuentasPagarVM> modelo);
        Task<byte[]> Deudores(RecibosCreadosVM modelo, int id);
        Task<byte[]> DeudoresResumen(RecibosCreadosVM modelo, int id);
        byte[] EstadoCuentas(List<EstadoCuentasVM> modelo);
        byte[] HistoricoPagosPropiedadPDF(HistoricoPropiedadPagosVM model);
        Task<byte[]> ReporteCompIslr(IEnumerable<ComprobanteRetencion> comprobantes, int id);
        Task<byte[]> ReporteCompIva(IEnumerable<CompRetIva> comprobantes, int id);
        byte[] ReporteHistorico(List<EstadoCuentasVM> modelo);
        Task<byte[]> ReporteLicenciada(RecibosCreadosVM modelo, int id);
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
            var tasaToday = _context.HistorialMoneda.FirstOrDefault(c => c.Actual);
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
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(10).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + condominio.Nombre).FontSize(10).FontColor("#004581").Bold();
                            //col.Item().Text("Relación de Gastos").FontSize(10).FontColor("#004581").Bold();
                        });

                        row.RelativeItem().Padding(3).Column(col =>
                        {
                            col.Item().Text("Fecha de emisión: " + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(8).FontColor("#004581").Bold();
                            col.Item().Text(text =>
                            {
                                text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor("#004581").Bold());
                                text.CurrentPageNumber();
                                text.Span(" / ");
                                text.TotalPages();
                            });

                            col.Item().PaddingTop(10).Text("Tasa $: " + (tasaToday != null ? tasaToday.ConversionRate.ToString("N") : "")).FontSize(10).FontColor("#004581").Bold();

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
                                   .Padding(5).Text("Acumulado Mora 1%").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Acumulado Indexación 30%").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Abonado").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Cuota del Mes").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total a Pagar").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto Ref.").FontColor("#607080").Bold().FontSize(8);

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
                                    .Padding(5).Text(recibos.Sum(c => c.Monto).ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(recibos.Sum(c => c.MontoMora).ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9")
                                    .Padding(5).Text(recibos.Sum(c => c.MontoIndexacion).ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(recibos.Sum(c => c.Abonado).ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(propiedad.Saldo.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text((propiedad.Saldo + propiedad.Deuda).ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(((propiedad.Saldo + propiedad.Deuda)/ (tasaToday != null ? tasaToday.ConversionResult : 1)).ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                    totalSaldo += propiedad.Saldo;
                                    totalDeuda += recibos.Sum(c => c.Monto);
                                    totalMulta += recibos.Sum(c => c.MontoMora);
                                    totalIntereses += recibos.Sum(c => c.MontoIndexacion);
                                    totalCredito += recibos.Sum(c => c.Abonado);
                                    totalPagar += propiedad.Deuda + -(decimal)propiedad.Creditos + propiedad.Saldo;
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

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text((totalPagar/ (tasaToday != null ? tasaToday.ConversionResult : 1)).ToString("N")).Bold().FontColor("#607080").FontSize(8);

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
            var tasaToday = _context.HistorialMoneda.FirstOrDefault(c => c.Actual);

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

                            col.Item().PaddingTop(10).Text("Tasa $: " + (tasaToday != null ? tasaToday.ConversionRate.ToString("N") : "") + "Bs").FontSize(10).FontColor("#004581").Bold();

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
                                        // saldo
                                        columns.RelativeColumn();
                                        // monto $
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
                                       .Padding(5).Text("Saldo").FontColor("#607080").Bold().FontSize(8);

                                        header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Monto Ref.").FontColor("#607080").Bold().FontSize(8);
                                    });

                                    decimal totalMonto = 0;
                                    decimal totalInteres = 0;
                                    decimal totalMulta = 0;
                                    decimal totalAbono = 0;
                                    //decimal totalMontoRef = 0;

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
                                     .Padding(5).Text(saldo.ToString("N"))
                                     .FontColor("#607080")
                                     .Bold()
                                     .FontSize(8);

                                    tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text((saldo/ (tasaToday != null ? tasaToday.ConversionResult : 1)).ToString("N"))
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

                                tabla.Cell().ColumnSpan(4).BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);


                                tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(totalSaldoGlobal.ToString("N"))
                                 .FontColor("#607080")
                                 .Bold()
                                 .FontSize(8);

                                tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text((totalSaldoGlobal / (tasaToday != null ? tasaToday.ConversionResult : 1)).ToString("N"))
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

        public async Task<byte[]> ConciliacionPDF(ItemConciliacionVM modelo)
        {
            var condominio = await _context.Condominios.FindAsync(modelo.ConciliacionAnterior.IdCondominio);

            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Padding(10).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + condominio.Nombre).FontSize(10).FontColor("#004581").Bold();
                            //col.Item().Text("Relación de Gastos").FontSize(10).FontColor("#004581").Bold();
                        });

                        row.RelativeItem().Padding(10).Column(col =>
                        {
                            //col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().BorderBottom(1).PaddingBottom(5).AlignCenter().Text("RESUMEN CONCILIACIÓN BANCARIA").FontSize(8).FontColor("#004581").Bold();

                            col.Item().Text("Fecha de emisión: " + modelo.ConciliacionAnterior.FechaEmision.ToString("dd/MM/yyyy")).FontSize(8).FontColor("#004581").Bold();
                            col.Item().Text("Cuenta: " + modelo.SubCuenta.Descricion).FontSize(8).FontColor("#004581").Bold();
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
                            x.Item().Border(0).BorderColor("#D9D9D9").Table(tabla =>
                           {
                               tabla.ColumnsDefinition(columns =>
                               {
                                   columns.RelativeColumn();
                                   columns.RelativeColumn();
                                   columns.RelativeColumn();
                                   columns.RelativeColumn();
                               });

                               tabla.Header(header =>
                               {
                                   header.Cell().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Saldo Inicial").FontColor("#607080").Bold().FontSize(8);

                                   header.Cell().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total Ingresos").FontColor("#607080").Bold().FontSize(8);

                                   header.Cell().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("Total Egresos").FontColor("#607080").Bold().FontSize(8);

                                   header.Cell().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("Saldo Final").FontColor("#607080").Bold().FontSize(8);


                               });

                               tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(modelo.ConciliacionAnterior.SaldoInicial.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                               tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(modelo.TotalIngreso.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                               tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(modelo.TotalEgreso.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                               tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(modelo.ConciliacionAnterior.SaldoFinal.ToString("N")).FontColor("#607080").Bold().FontSize(8);

                           });
                            //x.Item().AlignCenter().Text("Transacciones del Mes: " + modelo.Transacciones.Fecha.ToString("MM/yyyy")).FontSize(10).FontColor("#004581").Bold();
                            x.Spacing(10);
                            x.Item().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    // fecha
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();

                                    // para span de la descripcion
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();

                                    // ingreso
                                    columns.RelativeColumn();

                                    // egreso 
                                    columns.RelativeColumn();

                                    // saldo
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().ColumnSpan(2).BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(4).BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Descripción").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Ingreso").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Egreso").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().BorderTop(1).BorderBottom(1).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Saldo").FontColor("#607080").Bold().FontSize(8);

                                });

                                decimal saldo = modelo.ConciliacionAnterior.SaldoInicial;

                                foreach (var item in modelo.Pagos)
                                {

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(item.Fecha.ToString("dd/MM/yyyy")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(4).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(item.Concepto).FontColor("#607080").FontSize(8);

                                    if (item.TipoOperacion)
                                    {
                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text(item.Monto.ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9")
                                        .Padding(5).Text("0.00").FontColor("#607080").FontSize(8);

                                        saldo += item.Monto;

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text(saldo.ToString("N")).FontColor("#607080").FontSize(8);
                                    }
                                    else
                                    {


                                        tabla.Cell().Border(0).BorderColor("#D9D9D9")
                                        .Padding(5).Text("0.00").FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text(item.Monto.ToString("N")).FontColor("#607080").FontSize(8);

                                        saldo -= item.Monto;

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text(saldo.ToString("N")).FontColor("#607080").FontSize(8);
                                    }


                                }
                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology").FontSize(8);
                        });
                });
            }).GeneratePdf();

            return data;
        }

        public async Task<byte[]> ReporteLicenciada(RecibosCreadosVM modelo, int id)
        {
            var relacionGasto = await _context.RelacionGastos.FindAsync(id);
            var condominio = await _context.Condominios.FirstOrDefaultAsync(c => c.IdCondominio == relacionGasto.IdCondominio);


            decimal totalDeuda = 0;
            decimal totalIntereses = 0;
            decimal totalMulta = 0;
            decimal totalMontoReciboVencido = 0;
            decimal totalSaldo = 0;
            decimal totalMes = 0;
            decimal totalPagarSuma = 0;

            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(10).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + condominio.Nombre).FontSize(10).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignCenter().Text("Reporte " + (relacionGasto != null ? relacionGasto.Mes : "")).FontSize(10).FontColor("#004581").Bold();
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
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Código").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("RIF").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Razón Social").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Cant. Recibos").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Deuda").FontColor("#607080").Bold().FontSize(8);

                                   // header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   //.Padding(5).Text("Recibo Vencido").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Mora 1% Mes Vencido").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Multa 30% Mes Vencido").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Cuota del Mes").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total del Mes").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total a Pagar").FontColor("#607080").Bold().FontSize(8);

                                });

                                if (modelo.Propiedades != null && modelo.Propiedades.Any()
                                    && modelo.Propietarios != null && modelo.Propietarios.Any()
                                    && modelo.Recibos != null && modelo.Recibos.Any())
                                {
                                    foreach (var propiedad in modelo.Propiedades)
                                    {
                                        var propietario = modelo.Propietarios.First(c => c.Id == propiedad.IdUsuario);

                                        var recibos = modelo.Recibos
                                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad
                                        && c.Fecha < relacionGasto.Fecha
                                        && !c.Pagado
                                        && !c.ReciboActual
                                        && c.IdRgastos != relacionGasto.IdRgastos)
                                        .OrderBy(c => c.Fecha)
                                        .ToList();

                                        var reciboActual = _context.ReciboCobros.FirstOrDefault(c => c.IdRgastos == relacionGasto.IdRgastos && c.IdPropiedad == propiedad.IdPropiedad);

                                        var culturaEspaniola = new CultureInfo("es-ES");

                                        var fechaRel = relacionGasto.Fecha.AddMonths(-2);
                                        var mesAnterior = fechaRel.Month.ToString() + "-" + fechaRel.ToString("MMM", culturaEspaniola).ToUpper() + ".-" + fechaRel.ToString("yyyy");

                                        ReciboCobro? ultimoRecibo = recibos.Find(c => c.Mes == mesAnterior);

                                        var deudaRecibos = recibos.Sum(c => c.TotalPagar);
                                        //var totalPagar = recibos.Sum(c => c.TotalPagar);


                                        tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignLeft()
                                        .Padding(5).Text(propiedad.Codigo).FontColor("#607080").Bold().FontSize(8);

                                        tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignLeft()
                                            .Padding(5).Text(propietario.LastName).FontColor("#607080").Bold().FontSize(8);

                                        tabla.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignLeft()
                                        .Padding(5).Text(propietario.FirstName).FontColor("#607080").Bold().FontSize(8);

                                        tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        .Padding(5).Text(recibos.Any() ? (recibos.Count + 1).ToString() : "1").FontColor("#607080").Bold().FontSize(8);

                                        var aux = (deudaRecibos - (ultimoRecibo != null ? ultimoRecibo.MontoMora : 0) -
                                            (ultimoRecibo != null ? ultimoRecibo.MontoIndexacion : 0));

                                        var auxTotalMes = (reciboActual != null ? reciboActual.Monto : 0) + 
                                        (ultimoRecibo != null ? ultimoRecibo.MontoMora : 0) + 
                                        (ultimoRecibo != null ? ultimoRecibo.MontoIndexacion : 0);

                                        tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        .Padding(5).Text((aux > 0 ? aux : 0).ToString("N")).FontColor("#607080").FontSize(8);

                                        //tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        //.Padding(5).Text((ultimoRecibo != null ? ultimoRecibo.Monto : 0).ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        .Padding(5).Text((ultimoRecibo != null ? ultimoRecibo.MontoMora : 0).ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        .Padding(5).Text((ultimoRecibo != null ? ultimoRecibo.MontoIndexacion : 0).ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        .Padding(5).Text((reciboActual != null ? reciboActual.Monto : 0).ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        .Padding(5).Text(auxTotalMes.ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        .Padding(5).Text((deudaRecibos + (reciboActual != null ? reciboActual.Monto : 0)).ToString("N")).Bold().FontColor("#607080").FontSize(8);
                                        //tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        //.Padding(5).Text(totalPagar.ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                        totalDeuda += aux;
                                        totalMontoReciboVencido += ultimoRecibo != null ? ultimoRecibo.Monto : 0;
                                        totalIntereses += (ultimoRecibo != null ? ultimoRecibo.MontoMora : 0);
                                        totalMulta += (ultimoRecibo != null ? ultimoRecibo.MontoIndexacion : 0);
                                        totalSaldo += (reciboActual != null ? reciboActual.Monto : 0);
                                        totalMes += auxTotalMes;
                                        totalPagarSuma += deudaRecibos + (reciboActual != null ? reciboActual.Monto : 0);
                                    }
                                }

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                        .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().ColumnSpan(3).Border(1).BorderColor("#D9D9D9").AlignLeft()
                                .Padding(5).Text("Totales").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                .Padding(5).Text(totalDeuda.ToString("N")).Bold().FontColor("#607080").FontSize(8);

                               // tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                               //.Padding(5).Text(totalMontoReciboVencido.ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                .Padding(5).Text(totalIntereses.ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                .Padding(5).Text(totalMulta.ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                .Padding(5).Text(totalSaldo.ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                               .Padding(5).Text(totalMes.ToString("N")).Bold().FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(1).BorderColor("#D9D9D9").AlignRight()
                                .Padding(5).Text(totalPagarSuma.ToString("N")).Bold().FontColor("#607080").FontSize(8);

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

        public byte[] ReporteHistorico(List<EstadoCuentasVM> modelo)
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
                            x.Item().AlignCenter().Text("Histórico de Recibos Oficinas").Bold().FontSize(10).FontColor("#004581");

                            decimal totalSaldoGlobal = 0;

                            foreach (var item in modelo.Where(item => item.ReciboCobro.Any()))
                            {
                                x.Item().AlignLeft().Text("Oficina: " + item.Propiedad.Codigo + " Rif/CI: " + item.User.LastName + " " + item.User.FirstName).Bold().FontSize(8).FontColor("#004581");

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
                                        // estado
                                        columns.RelativeColumn();
                                        // total a pagar
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
                                       .Padding(5).Text("Estado").FontColor("#607080").Bold().FontSize(8);

                                        header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Total a Pagar").FontColor("#607080").Bold().FontSize(8);

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

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text(recibo.Pagado ? "Cancelado" : "En Deuda").FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text(recibo.TotalPagar.ToString("N")).FontColor("#607080").FontSize(8);

                                        tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                        if (!recibo.Pagado)
                                        {
                                            totalMonto += recibo.Monto;
                                            totalInteres += recibo.ReciboActual ? 0 : recibo.MontoMora;
                                            totalMulta += recibo.ReciboActual ? 0 : recibo.MontoIndexacion;
                                            totalAbono += recibo.Abonado;
                                        }

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
                                     .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

                                    tabla.Cell().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("").FontColor("#607080").Bold().FontSize(8);

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

                                tabla.Cell().ColumnSpan(6).BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").AlignMiddle()
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

        public byte[] CuentasPagarPDF(List<CuentasPagarVM> modelo)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.Header().Row(row =>
                    {

                        row.RelativeItem().Padding(3).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + modelo.First().Condominio).FontSize(10).FontColor("#004581").Bold();
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
                            x.Item().AlignCenter().Text("Cuentas por Pagar").Bold().FontSize(10).FontColor("#004581");

                            decimal totalSaldoGlobal = 0;

                            x.Item().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {

                                    //proveedor
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    //num factura
                                    columns.RelativeColumn();
                                    // Base
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // iva
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // total
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // ret iva
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // ret islr
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // total a pagar
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().ColumnSpan(3).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Proveedor/Beneficiario").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("N° Factura").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Base Imponible").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("IVA").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Retención IVA").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Retención ISLR").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total a Pagar").FontColor("#607080").Bold().FontSize(8);
                                });


                                foreach (var item in modelo)
                                {
                                    tabla.Cell().ColumnSpan(3).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.Proveedor).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.NumFactura).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.BaseImponible.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text(item.Iva.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.MontoTotal.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.RetIva.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.RetIslr.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.TotalPagar.ToString("N")).FontColor("#607080").FontSize(8);

                                    totalSaldoGlobal += item.TotalPagar;
                                }


                                tabla.Cell().ColumnSpan(3).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("Totales").FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(totalSaldoGlobal.ToString("N")).FontColor("#607080").FontSize(8);


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

        public byte[] CuentasCobrarPDF(List<CuentasCobrarVM> modelo)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.Header().Row(row =>
                    {

                        row.RelativeItem().Padding(3).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + modelo.First().Condominio).FontSize(10).FontColor("#004581").Bold();
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
                            x.Item().AlignCenter().Text("Cuentas por Cobrar").Bold().FontSize(10).FontColor("#004581");

                            decimal totalSaldoGlobal = 0;

                            x.Item().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {

                                    //proveedor
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    //num factura
                                    columns.RelativeColumn();
                                    // Base
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // iva
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // total
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // ret iva
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // ret islr
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // total a pagar
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().ColumnSpan(3).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Cliente").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("N° Factura").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Base Imponible").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("IVA").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Retención IVA").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Retención ISLR").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total a Pagar").FontColor("#607080").Bold().FontSize(8);
                                });


                                foreach (var item in modelo)
                                {
                                    tabla.Cell().ColumnSpan(3).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.Cliente).FontColor("#607080").FontSize(8);

                                    tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.NumFactura).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.BaseImponible.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text(item.Iva.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.MontoTotal.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.RetIva.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.RetIslr.ToString("N")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.TotalPagar.ToString("N")).FontColor("#607080").FontSize(8);

                                    totalSaldoGlobal += item.TotalPagar;
                                }


                                tabla.Cell().ColumnSpan(3).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("Totales").FontColor("#607080").FontSize(8);

                                tabla.Cell().Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(0).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(totalSaldoGlobal.ToString("N")).FontColor("#607080").FontSize(8);


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

        public byte[] HistoricoPagosPropiedadPDF(HistoricoPropiedadPagosVM model)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.Header().Row(row =>
                    {
                        var condominio = _context.Condominios.Find(model.Propiedad.IdCondominio);
                        row.RelativeItem().Padding(3).Column(col =>
                        {
                            col.Item().MaxWidth(100).MaxHeight(60).Image("wwwroot/images/yllenAzul.png");
                            col.Item().PaddingTop(10).Text("Condominio " + condominio.Nombre).FontSize(10).FontColor("#004581").Bold();
                            col.Item().PaddingTop(10).Text("Propiedad " + model.Propiedad.Codigo).FontSize(10).FontColor("#004581").Bold();
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
                            x.Item().AlignCenter().Text("Histórico de Pagos").Bold().FontSize(10).FontColor("#004581");

                            decimal totalSaldoGlobal = 0;

                            x.Item().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {

                                    //deuda
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // recibo actual
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // credito
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // saldo
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();

                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Deuda").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Recibo Actual").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Creditos").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Saldo").FontColor("#607080").Bold().FontSize(8);
                                });

                                tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(model.Propiedad.Deuda.ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(model.Propiedad.Saldo.ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(((decimal)model.Propiedad.Creditos).ToString("N")).FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text((model.Propiedad.Saldo + model.Propiedad.Deuda - (decimal)model.Propiedad.Creditos).ToString("N")).FontColor("#607080").FontSize(8);

                            });

                            x.Item().BorderTop(1).BorderBottom(1).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {

                                    //fecha
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // forma pago
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // banco
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // referencia
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    // monto
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();

                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Forma Pago").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Banco").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Referencia").FontColor("#607080").Bold().FontSize(8);

                                    header.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto").FontColor("#607080").Bold().FontSize(8);
                                });


                                foreach (var item in model.Pagos)
                                {
                                    tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.IdPagoNavigation.Fecha.ToString("dd/MM/yyyy")).FontColor("#607080").FontSize(8);

                                    tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text(item.IdPagoNavigation.FormaPago ? "Transferencia" : "Efectivo").FontColor("#607080").FontSize(8);

                                    if (item.IdPagoNavigation.ReferenciasPrs.Any())
                                    {
                                        var banco = _context.SubCuenta.Find(Convert.ToInt32(item.IdPagoNavigation.ReferenciasPrs.First().Banco));

                                        tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text(banco != null ? banco.Descricion : "").FontColor("#607080").FontSize(8);

                                        tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text(item.IdPagoNavigation.ReferenciasPrs.First().NumReferencia.ToString()).FontColor("#607080").FontSize(8);
                                    }
                                    else
                                    {
                                        tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("").FontColor("#607080").FontSize(8);

                                        tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("").FontColor("#607080").FontSize(8);
                                    }


                                    tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.IdPagoNavigation.Monto.ToString("N")).FontColor("#607080").FontSize(8);

                                    totalSaldoGlobal += item.IdPagoNavigation.Monto;
                                }


                                tabla.Cell().ColumnSpan(8).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("Total de Pagos").FontColor("#607080").FontSize(8);

                                tabla.Cell().ColumnSpan(2).Border(1).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(totalSaldoGlobal.ToString("N")).FontColor("#607080").FontSize(8);

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
    }
}
