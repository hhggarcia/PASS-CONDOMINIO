using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

//using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using MimeKit;
using NetTopologySuite.Index.HPRtree;
using NPOI.SS.Formula.Functions;
using Prueba.Context;
using Prueba.Controllers;
using Prueba.Models;
using Prueba.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SQLitePCL;
using System.Text.RegularExpressions;

namespace Prueba.Services
{
    public interface IPDFServices
    {
        byte[] ExamplePDF();
        byte[] ReciboPDF(Dictionary<Propiedad, List<ReciboCobro>> modelo);
        byte[] HistorialPDF(Dictionary<Propiedad, List<PagoRecibido>> modelo);
        byte[] DetalleReciboPDF(DetalleReciboVM detalleReciboVM);
        byte[] ComprobantePEVMPDF(ComprobantePEVM comprobanteVM);
        byte[] ComprobantePDF(ComprobanteVM comprobanteVM);
        byte[] ComprobanteCEVMPDF(ComprobanteCEVM comprobanteCEVM);
        byte[] ComprobantePagosRecibidosPDF(IndexPagoRecibdioVM indexPagoRecibdio);
        byte[] LibroDiarioPDF(LibroDiarioVM ldiarioGlobals);
        byte[] EstadoDeResultadoPDF(EstadoResultadoVM estadoResultado);
        byte[] PagosEmitidosPDF(IndexPagosVM indexPagosVM);
        byte[] RelacionGastosPDF(RelacionDeGastosVM relacionDeGastos);
        byte[] ComprobantePagosNominaPDF(ComprobantePagoNomina pagoNomina);
        byte[] ComprobanteOrdenPagoPDF(ComprobanteOrdenPago modelo);
        byte[] Transacciones(TransaccionVM transaccion);
        byte[] MontosPDF(List<MontosVW> montosVW);
        byte[] ComprobanteRetencionesISLR(ComprobanteRetencionesISLRVM comprobante);
        byte[] ComprobanteRetencionesIVA(ComprobanteRetencionesIVAVM retencionesIVAVM);
        byte[] LibroVentas(List<LibroVentasVM> libroVentas);
        byte[] EstadoDeCuentas(List<EstadoCuentasVM> estadoCuentasVM);
        byte[] ReciboCobroPDF(List<ReciboCobroVM> reciboCobroVM);
        byte[] RetencionesIva(List<ListaRetencionesIVAVM> listaRetencionesIVAVM);
        byte[] ComprobanteAnticipoPDF(ComprobanteAnticipoVM modelo);
        byte[] ComprobanteBonosPDF(ComprobantePagoNomina pagoNomina);
    }
    public class PDFServices : IPDFServices
    {
        private readonly IWebHostEnvironment _host;
        private readonly NuevaAppContext _context;

        public PDFServices(IWebHostEnvironment host,
            NuevaAppContext context)
        {
            _host = host;
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ExamplePDF()
        {
            var data = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);

                    page.Header().ShowOnce().Row(row =>
                    {
                        //var rutaImagen = Path.Combine(_host.WebRootPath, "images/VisualStudio.png");
                        //byte[] imageData = System.IO.File.ReadAllBytes(rutaImagen);

                        row.ConstantItem(140).Height(60).Placeholder();
                        //row.ConstantItem(150).Image(imageData);


                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().Text("Codigo Estudiante SAC").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Jr. Las mercedes N378 - Lima").FontSize(9);
                            col.Item().AlignCenter().Text("987 987 123 / 02 213232").FontSize(9);
                            col.Item().AlignCenter().Text("codigo@example.com").FontSize(9);

                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Border(1).BorderColor("#257272")
                            .AlignCenter().Text("RUC 21312312312");

                            col.Item().Background("#257272").Border(1)
                            .BorderColor("#257272").AlignCenter()
                            .Text("Boleta de venta").FontColor("#fff");

                            col.Item().Border(1).BorderColor("#257272").
                            AlignCenter().Text("B0001 - 234");


                        });
                    });

                    page.Content().PaddingVertical(10).Column(col1 =>
                    {
                        col1.Item().Column(col2 =>
                        {
                            col2.Item().Text("Datos del cliente").Underline().Bold();

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Nombre: ").SemiBold().FontSize(10);
                                txt.Span("Mario mendoza").FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("DNI: ").SemiBold().FontSize(10);
                                txt.Span("978978979").FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Direccion: ").SemiBold().FontSize(10);
                                txt.Span("av. miraflores 123").FontSize(10);
                            });
                        });

                        col1.Item().LineHorizontal(0.5f);

                        col1.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background("#257272")
                                .Padding(2).Text("Producto").FontColor("#fff");

                                header.Cell().Background("#257272")
                               .Padding(2).Text("Precio Unit").FontColor("#fff");

                                header.Cell().Background("#257272")
                               .Padding(2).Text("Cantidad").FontColor("#fff");

                                header.Cell().Background("#257272")
                               .Padding(2).Text("Total").FontColor("#fff");
                            });

                            foreach (var item in Enumerable.Range(1, 45))
                            {
                                var cantidad = Placeholders.Random.Next(1, 10);
                                var precio = Placeholders.Random.Next(5, 15);
                                var total = cantidad * precio;

                                tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text(Placeholders.Label()).FontSize(10);

                                tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text(cantidad.ToString()).FontSize(10);

                                tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text($"S/. {precio}").FontSize(10);

                                tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).AlignRight().Text($"S/. {total}").FontSize(10);
                            }

                        });

                        col1.Item().AlignRight().Text("Total: 1500").FontSize(12);

                        if (1 == 1)
                            col1.Item().Background(Colors.Grey.Lighten3).Padding(10)
                            .Column(column =>
                            {
                                column.Item().Text("Comentarios").FontSize(14);
                                column.Item().Text(Placeholders.LoremIpsum());
                                column.Spacing(5);
                            });

                        col1.Spacing(10);
                    });


                    page.Footer()
                    .AlignRight()
                    .Text(txt =>
                    {
                        txt.Span("Software desarrollado por: Password Technology");
                    });
                });
            }).GeneratePdf();

            return data;
        }
        public byte[] ReciboPDF(Dictionary<Propiedad, List<ReciboCobro>> modelo)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Recibo").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {

                            x.Spacing(20);
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                });
                                tabla.Header(header =>
                                {

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Propiedad").FontColor("#607080").Bold().FontSize(12);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(12);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto").FontColor("#607080").Bold().FontSize(12);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Abonado").FontColor("#607080").Bold().FontSize(12);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Número de relación de Gastos").FontColor("#607080").Bold().FontSize(12);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Gastos").FontColor("#607080").Bold().FontSize(12);
                                });
                                foreach (var item in modelo)
                                {
                                    foreach (var recibo in item.Value)
                                    {
                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(item.Key.Codigo).FontSize(10).FontColor("#607080");

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(recibo.Fecha.ToString("dd/MM/yyyy")).FontSize(10).FontColor("#607080");

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(recibo.Monto.ToString("N")).FontSize(10).FontColor("#607080");

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(recibo.Abonado.ToString("N")).FontSize(10).FontColor("#607080");

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                     .Padding(5).Text(recibo.IdRgastos.ToString("N")).FontSize(10).FontColor("#607080");

                                        if (recibo.EnProceso)
                                        {
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                           .Padding(5).Text("En proceso").FontSize(10).FontColor("#607080");
                                        }
                                        else if (recibo.Pagado)
                                        {
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                           .Padding(5).Text("Pagado").FontSize(10).FontColor("#607080");
                                        }
                                        else
                                        {
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                           .Padding(5).Text("En deuda").FontSize(10).FontColor("#607080");
                                        }

                                    }
                                }
                            });

                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] HistorialPDF(Dictionary<Propiedad, List<PagoRecibido>> modelo)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Historial").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {

                            x.Spacing(20);
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {

                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                tabla.Header(header =>
                                {

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Propiedad").FontColor("#607080").Bold().FontSize(12);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(12);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto").FontColor("#607080").Bold().FontSize(12);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Forma de Pago").FontColor("#607080").Bold().FontSize(12);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Concepto").FontColor("#607080").Bold().FontSize(12);

                                });

                                foreach (var item in modelo)
                                {
                                    if (item.Value.Count() > 0)
                                    {
                                        foreach (var historial in item.Value)
                                        {
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(item.Key.Codigo).FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(historial.Fecha.ToString("dd/MM/yyyy")).FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(historial.Monto.ToString("N")).FontSize(10).FontColor("#607080");

                                            if (historial.FormaPago)
                                            {
                                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                .Padding(5).Text("Transferencia").FontSize(10).FontColor("#607080");
                                            }
                                            else
                                            {
                                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                               .Padding(5).Text("Efectivo").FontSize(10).FontColor("#607080");
                                            }
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(historial.Concepto).FontSize(10).FontColor("#607080");

                                        }
                                    }
                                    else
                                    {
                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text("No").FontSize(10).FontColor("#607080");

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text("Hay").FontSize(10).FontColor("#607080");

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text("Pagos").FontSize(10).FontColor("#607080");

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text("En esta propiedad").FontSize(10).FontColor("#607080");
                                    }

                                }
                            });

                        });

                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] DetalleReciboPDF(DetalleReciboVM detalleReciboVM)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().AlignCenter().Text("Junta de Condominio " + detalleReciboVM.condominio.Nombre).Bold().FontSize(20).FontColor("#004581");

                    page.Content()
                        .Column(x =>
                        {

                            x.Spacing(10);
                            x.Item().AlignCenter().Text("RECIBO DE COBRO").FontSize(14).Bold().FontColor("#004581");
                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Padding(10, 0).Column(col =>
                                {
                                    col.Item().Image("wwwroot/images/logo-1.png");
                                });
                                row.RelativeItem().Padding(10, 0).Column(col =>
                                {
                                    //var propiedad = detalleReciboVM.Propiedad.Codigo;
                                    //var torrePiso = propiedad.Split("-");
                                    //col.Item().Text("Torre: " + torrePiso[0].Trim()).FontSize(12).Bold();
                                    //col.Item().Text("Piso: " + torrePiso[1].Trim()).FontSize(12).Bold();
                                    //col.Item().Text("Partamento: " + propiedad).FontSize(12).Bold();
                                    //col.Item().Text("Propietario: " + detalleReciboVM.Propietario.FirstName + " " + detalleReciboVM.Propietario.LastName).FontSize(12).Bold();
                                });
                                row.RelativeItem().Padding(10, 0).Column(col =>
                                {
                                    col.Item().Text("Alícuota").FontSize(12).Bold();
                                    col.Item().Text(detalleReciboVM.Propiedad.Alicuota.ToString());
                                });
                            });
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").PaddingBottom(5f).Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    for (var i = 0; detalleReciboVM.CuotasRecibosCobros.Count > i; ++i)
                                    {
                                        columns.RelativeColumn();
                                    }
                                    columns.RelativeColumn();
                                });
                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Saldo Anterior").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Saldo mes actual").Bold().FontSize(10);

                                    if (detalleReciboVM.CuotasRecibosCobros.Count > 0)
                                    {
                                        foreach (var cuotas in detalleReciboVM.CuotasRecibosCobros)
                                        {
                                            header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(cuotas.CuotasEspeciale.Descripcion + "\n" +
                                            " Cuotas Por Pagar " + cuotas.ReciboCuota.CuotasFaltantes).Bold().FontSize(10);
                                        }
                                    }
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Saldo Actual").Bold().FontSize(10);
                                });
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(detalleReciboVM.Propiedad.Deuda.ToString("N2") + " Bs.").FontSize(8);
                                decimal totalCuotasSubCuotas = 0;

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(detalleReciboVM.Propiedad.Saldo.ToString("N2") + " Bs.").FontSize(8);
                                if (detalleReciboVM.CuotasRecibosCobros.Count > 0)
                                {
                                    foreach (var cuotas in detalleReciboVM.CuotasRecibosCobros)
                                    {
                                        if (cuotas.ReciboCuota.CuotasFaltantes != 0)
                                        {
                                            totalCuotasSubCuotas += (decimal)cuotas.ReciboCuota.SubCuotas;
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(cuotas.ReciboCuota.SubCuotas.ToString("N2") + " Bs.").FontSize(8);
                                        }
                                        else
                                        {
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                           .Padding(5).Text("No posee deudas.").FontSize(8);
                                        }
                                    }
                                }

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text((detalleReciboVM.Propiedad.Deuda + detalleReciboVM.Propiedad.Saldo + totalCuotasSubCuotas).ToString("N2") + " Bs.").FontSize(8);
                            });


                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            tabla.Header(header =>
                            {
                                header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("Fecha").Bold().FontSize(10);

                                header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Tipo de Gasto").Bold().FontSize(10);

                                header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("Descripción").Bold().FontSize(10);

                                header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("Monto Bs").Bold().FontSize(10);

                                header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("Alícuota").Bold().FontSize(10);

                                header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("Pagar Bs").Bold().FontSize(10);
                            });

                            if (detalleReciboVM.RelacionGastos.SubcuentasGastos != null && detalleReciboVM.RelacionGastos.SubcuentasGastos.Any()
                            && detalleReciboVM.RelacionGastos.GastosDiario != null && detalleReciboVM.RelacionGastos.GastosDiario.Any()
                            && detalleReciboVM.RelacionGastos.CCGastos != null && detalleReciboVM.RelacionGastos.CCGastos.Any())
                            {
                                foreach (var item in detalleReciboVM.RelacionGastos.GastosDiario)
                                {
                                    var idcc = detalleReciboVM.RelacionGastos.CCGastos.Where(c => c.IdCodCuenta == item.IdCodCuenta);
                                    if (idcc != null && idcc.Any())
                                    {
                                        var subcuenta = detalleReciboVM.RelacionGastos.SubcuentasGastos.Where(c => c.Id == idcc.First().IdSubCuenta).ToList();

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(item.Fecha.ToString("dd/MM/yyyy")).FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                       .Padding(5).Text("Ordinario").FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(subcuenta.First().Descricion).FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(item.Monto.ToString("N") + " " + item.SimboloRef).FontSize(8);


                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(detalleReciboVM.Propiedad.Alicuota.ToString()).FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(((item.MontoRef * (detalleReciboVM.Propiedad.Alicuota / 100)).ToString("N"))).FontSize(8);


                                        //tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        //.Padding(5).Text(item.MontoRef.ToString("N")+ " "+ item.SimboloRef).FontSize(8);


                                    }
                                }
                            }
                            else
                            {
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("No").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("hay").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("Gastos").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(":D").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(8);

                            }

                            if (detalleReciboVM.RelacionGastos.Provisiones != null && detalleReciboVM.RelacionGastos.Provisiones.Any()
                            && detalleReciboVM.RelacionGastos.SubCuentasProvisiones != null && detalleReciboVM.RelacionGastos.SubCuentasProvisiones.Any()
                            && detalleReciboVM.RelacionGastos.CCProvisiones != null && detalleReciboVM.RelacionGastos.CCProvisiones.Any())
                            {
                                foreach (var provisiones in detalleReciboVM.RelacionGastos.Provisiones)
                                {
                                    var idcc = detalleReciboVM.RelacionGastos.CCProvisiones.Where(c => c.IdSubCuenta == provisiones.IdCodCuenta);
                                    if (idcc != null && idcc.Any())
                                    {
                                        var subcuenta = detalleReciboVM.RelacionGastos.SubCuentasProvisiones.Where(c => idcc.First().IdSubCuenta == c.Id).ToList();

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text("").FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                          .Padding(5).Text("Ordinario").FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                           .Padding(5).Text(subcuenta.First().Descricion).FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(provisiones.Monto.ToString("N")).FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(detalleReciboVM.Propiedad.Alicuota.ToString()).FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                         .Padding(5).Text(((provisiones.MontoRef * (detalleReciboVM.Propiedad.Alicuota / 100)).ToString("N"))).FontSize(8);

                                        //tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        //.Padding(5).Text(provisiones.ValorDolar.ToString("N")).FontSize(8);

                                        //tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        //.Padding(5).Text(provisiones.MontoRef.ToString("N") + " " + provisiones.SimboloRef).FontSize(8);

                                        //tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        //.Padding(5).Text(((provisiones.MontoRef * (detalleReciboVM.Propiedad.Alicuota / 100)).ToString("N"))).FontSize(8);
                                    }
                                }
                            }

                            tabla.Cell().BorderRight(0.5f).BorderTop(0.5f).BorderBottom(0.5f).BorderColor("#D9D9D9")
                            .Padding(5).Text("").FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f).BorderBottom(0.5f)
                            .Padding(5).Text("").FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f).BorderBottom(0.5f)
                            .Padding(5).Text("Sub Total").FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f).BorderBottom(0.5f)
                          .Padding(5).Text(detalleReciboVM.RelacionGastos.SubTotal.ToString("N")).FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f).BorderBottom(0.5f)
                            .Padding(5).Text("").FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f).BorderBottom(0.5f)
                            .Padding(5).Text(((detalleReciboVM.RelacionGastos.SubTotal * (detalleReciboVM.Propiedad.Alicuota / 100)).ToString("N"))).FontSize(8);

                            if (detalleReciboVM.RelacionGastos.Fondos != null && detalleReciboVM.RelacionGastos.Fondos.Any()
                             && detalleReciboVM.RelacionGastos.SubCuentasFondos != null && detalleReciboVM.RelacionGastos.SubCuentasFondos.Any()
                            && detalleReciboVM.RelacionGastos.CCFondos != null && detalleReciboVM.RelacionGastos.CCFondos.Any())
                            {
                                foreach (var fondo in detalleReciboVM.RelacionGastos.Fondos)
                                {
                                    var idcc = detalleReciboVM.RelacionGastos.CCFondos.Where(c => c.IdSubCuenta == fondo.IdCodCuenta).ToList();
                                    if (idcc != null && idcc.Any())
                                    {
                                        var subcuenta = detalleReciboVM.RelacionGastos.SubCuentasFondos.Where(c => c.Id == idcc.First().IdSubCuenta).ToList();

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text("").FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text("Gastos").FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text($"{subcuenta.First().Descricion} - {fondo.Porcentaje}  %").FontSize(8);


                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(((detalleReciboVM.RelacionGastos.SubTotal * fondo.Porcentaje / 100).ToString("N"))).FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text(detalleReciboVM.Propiedad.Alicuota.ToString()).FontSize(8);

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text((((detalleReciboVM.RelacionGastos.SubTotal * fondo.Porcentaje / 100) * (detalleReciboVM.Propiedad.Alicuota / 100)).ToString("N"))).FontSize(8);

                                    }
                                }

                            }

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f)
                            .Padding(5).Text("").FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f)
                            .Padding(5).Text("Total gasto del mes").FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f)
                            .Padding(5).Text("").FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f)
                            .Padding(5).Text("").FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f)
                            .Padding(5).Text(detalleReciboVM.RelacionGastos.Total.ToString("N")).FontSize(8);

                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9").BorderTop(0.5f)
                            .Padding(5).Text(((detalleReciboVM.RelacionGastos.Total * (detalleReciboVM.Propiedad.Alicuota / 100)).ToString("N"))).FontSize(8);
                        });
                            x.Item().Column(c =>
                            {
                                c.Item().AlignCenter().Text("Elaborado por Junta de Condominio " + detalleReciboVM.condominio.Nombre).FontSize(8);
                                c.Item().AlignCenter().Text("Junta de Condominio " + detalleReciboVM.condominio.Nombre).FontSize(10).Bold();
                                c.Item().AlignCenter().Text("Rif " + detalleReciboVM.condominio.Rif).FontSize(10).Bold();
                                c.Item().AlignCenter().Text("Enviar al correo: " + detalleReciboVM.condominio.Email).FontSize(12).Bold().FontColor("#FF0000");
                            });


                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] ComprobantePDF(ComprobanteVM comprobanteVM)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                        });
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Text("CONSTANCIA DE PAGO").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().AlignCenter().Text("DATOS DEL PAGO");
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("FECHA DEL PAGO").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("DINERO RECIBIDO").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("TASA DE CAMBIO").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("EQUIVALENTE EN DOLARES").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("SALDO GENERAL").Bold().FontSize(10);
                                });
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(DateTime.Today.ToString("dd/MM/yyyy")).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(comprobanteVM.PagoRecibido.Monto.ToString("N") + " " + comprobanteVM.PagoRecibido.SimboloRef).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(comprobanteVM.PagoRecibido.ValorDolar.ToString() + " " + comprobanteVM.PagoRecibido.SimboloRef).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(((comprobanteVM.PagoRecibido.Monto / comprobanteVM.PagoRecibido.ValorDolar).ToString("N2") + " $")).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text((comprobanteVM.Propiedad.Saldo).ToString("N2") + " " + comprobanteVM.PagoRecibido.SimboloRef).FontSize(8);

                            });
                            //if (comprobanteVM.Condominio != null && comprobanteVM.Inmueble != null && comprobanteVM.Propiedad != null)
                            //{
                            //    x.Item().AlignCenter().Text("Condominio").FontColor("#004581").FontSize(12).Bold();
                            //    x.Item().AlignCenter().Text("Condominio:" + comprobanteVM.Condominio.Nombre);
                            //    x.Item().AlignCenter().Text("Inmueble:" + comprobanteVM.Inmueble.Nombre);
                            //    x.Item().AlignCenter().Text("Propiedad:" + comprobanteVM.Propiedad.Codigo);
                            //}
                            //if (comprobanteVM.PagoRecibido != null)
                            //{
                            //    if (comprobanteVM.PagoRecibido.FormaPago && comprobanteVM.Referencias != null)
                            //    {
                            //        x.Item().AlignCenter().Text("Transferencia").FontColor("#004581").FontSize(12).Bold(); ;
                            //        x.Item().AlignCenter().Text("Fecha:" + comprobanteVM.PagoRecibido.Fecha.ToString("dd/MM/yyyy"));
                            //        x.Item().AlignCenter().Text("Referencia:" + comprobanteVM.Referencias.NumReferencia);
                            //        x.Item().AlignCenter().Text("Monto:" + comprobanteVM.PagoRecibido.Monto.ToString("N"));
                            //        x.Item().AlignCenter().Text("Fecha de comprobante:" + DateTime.Today.ToString("dd/MM/yyyy"));
                            //    }
                            //    else
                            //    {
                            //        x.Item().AlignCenter().Text("Efectivo").FontColor("#004581").FontSize(12).Bold(); ;
                            //        x.Item().AlignCenter().Text("Monto:" + comprobanteVM.PagoRecibido.Monto.ToString("N"));
                            //        x.Item().AlignCenter().Text("Fecha de comprobante:" + DateTime.Today.ToString("dd/MM/yyyy"));
                            //    }
                            //}

                        });

                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] ComprobanteCEVMPDF(ComprobanteCEVM comprobanteCEVM)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                        });
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Text("CONSTANCIA DE PAGO").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().AlignCenter().Text("DATOS DEL PAGO");
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("FECHA DEL PAGO").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("DINERO RECIBIDO").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("TASA DE CAMBIO").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("EQUIVALENTE EN DOLARES").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("DIFERENCIA").Bold().FontSize(10);
                                });
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(DateTime.Today.ToString("dd/MM/yyyy")).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(comprobanteCEVM.PagoRecibido.Monto.ToString("N") + " Bs.").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(comprobanteCEVM.PagoRecibido.ValorDolar.ToString() + " Bs.").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text((comprobanteCEVM.PagoRecibido.Monto / comprobanteCEVM.PagoRecibido.ValorDolar).ToString("N2") + " $").FontSize(8);
                                if (comprobanteCEVM.Restante != 0 && comprobanteCEVM.Restante != null)
                                {
                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                     .Padding(5).Text(comprobanteCEVM.Restante.ToString("N2") + " Bs.").FontSize(8);
                                }
                                else
                                {
                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                     .Padding(5).Text("0,00" + " Bs.").FontSize(8);
                                }

                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] ComprobantePEVMPDF(ComprobantePEVM comprobanteVM)
        {
            var proveedor = _context.Proveedors.Where(c => c.IdProveedor == comprobanteVM.Factura.IdProveedor).FirstOrDefault();
            var retIslr = _context.Islrs.Where(c => c.Id == proveedor.IdRetencionIslr).FirstOrDefault();
            var retIva = _context.Ivas.Where(c => c.Id == proveedor.IdRetencionIva).FirstOrDefault();

            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                            col.Item().PaddingTop(10).Text("Fecha: " + DateTime.Today.ToString("MM/yyyy")).Bold().FontColor("#004581").Bold();
                            col.Item().Text("FACTURA: " + comprobanteVM.Factura.NumControl).Bold().FontColor("#004581").Bold();
                            col.Item().Text("Beneficiario").FontColor("#004581").Bold();
                            col.Item().Text(comprobanteVM.Beneficiario).Bold().FontColor("#004581");
                            col.Item().Text("Concepto").Bold().FontColor("#004581").Bold();
                            col.Item().Text(comprobanteVM.Concepto).Bold().FontColor("#004581");

                        });
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Text("CONSTANCIA DE PAGO").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().AlignCenter().Text("DATOS FACTURA");
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(50);
                                    columns.ConstantColumn(50);
                                    columns.ConstantColumn(50);
                                });

                                tabla.Cell()
                               .Padding(5).Text("BASE");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text((comprobanteVM.Factura.Subtotal).ToString("N"));

                                tabla.Cell().Padding(5).Text("IVA");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text((comprobanteVM.Factura.Iva).ToString("N"));

                                tabla.Cell().Padding(5).Text("TOTAL");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text((comprobanteVM.Factura.MontoTotal).ToString("N"));

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("TOTAL A PAGAR");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text((comprobanteVM.Factura.MontoTotal).ToString("N"));

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                if (comprobanteVM.retencionesIslr && retIslr != null)
                                {
                                    tabla.Cell().Padding(5).Text("Retención ISLR Decreto 1808 Art.9 numeral " + retIslr.Tarifa.ToString() + "%");
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text(comprobanteVM.Islr.ToString("N"));

                                    comprobanteVM.Factura.MontoTotal -= comprobanteVM.Islr;
                                    comprobanteVM.Pago.Monto -= comprobanteVM.Islr;
                                }

                                if (comprobanteVM.retencionesIva && retIva != null)
                                {
                                    tabla.Cell().Padding(5).Text("Retención IVA " + retIva.Porcentaje.ToString() + "%");
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text(comprobanteVM.Iva.ToString());

                                    comprobanteVM.Factura.MontoTotal -= comprobanteVM.Iva;
                                    comprobanteVM.Pago.Monto -= comprobanteVM.Iva;
                                }

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("Total a pagar ");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text((comprobanteVM.Factura.MontoTotal).ToString("N"));


                                if (comprobanteVM.Anticipo != null)
                                {
                                    tabla.Cell().Padding(5).Text("ANTICIPO").FontColor("#FF0000");
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text(comprobanteVM.Anticipo.Saldo).FontColor("#FF0000");
                                }             
                            });

                            x.Item().AlignCenter().Text("DATOS PAGO");
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(50);
                                    columns.ConstantColumn(50);
                                    columns.ConstantColumn(50);
                                });

                                tabla.Cell()
                               .Padding(5).Text("Forma de Pago");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (comprobanteVM.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(3).Text("Transferencia");

                                } else
                                {
                                    tabla.Cell().Padding(3).Text("Efectivo");

                                }

                                tabla.Cell().Padding(5).Text("Fecha");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text(comprobanteVM.Pago.Fecha.ToString("dd/MM/yyyy"));

                                tabla.Cell().Padding(5).Text("Cuenta");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (comprobanteVM.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(5).Text(comprobanteVM.Banco.Descricion);

                                }
                                else
                                {
                                    tabla.Cell().Padding(5).Text(comprobanteVM.Caja.Descricion);

                                }

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("Monto");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text((comprobanteVM.Pago.Monto).ToString("N"));

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                            });
                            x.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });


                                table.Cell().Padding(5).Text("Elaborado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Revisado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Karina Lopez");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Roberto Villagras");
                                table.Cell().Padding(5).Text("Massimo Ruggiero");



                            });
                        });

                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] ComprobantePagosRecibidosPDF(IndexPagoRecibdioVM indexPagoRecibdio)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Pagos Recibidos").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {

                            x.Spacing(20);
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Propietario").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Propiedad").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Concepto").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Forma de Pago").FontColor("#607080").Bold().FontSize(10);
                                });
                                if (indexPagoRecibdio.UsuariosPropiedad != null
                               && indexPagoRecibdio.UsuariosPropiedad.Count() > 0
                               && indexPagoRecibdio.PropiedadPagos != null
                               && indexPagoRecibdio.PropiedadPagos.Count() > 0)
                                {
                                    foreach (var diccUsuarioPropiedad in indexPagoRecibdio.UsuariosPropiedad)
                                    {
                                        foreach (var propiedad in @diccUsuarioPropiedad.Value)
                                        {
                                            foreach (var propiedadPago in indexPagoRecibdio.PropiedadPagos)
                                            {
                                                if (propiedadPago.Key.IdPropiedad == propiedad.IdPropiedad)
                                                {
                                                    foreach (var pago in propiedadPago.Value)
                                                    {
                                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                        .Padding(5).Text((pago.Fecha.ToString("dd/MM/yyyy"))).FontSize(10).FontColor("#607080");

                                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                        .Padding(5).Text(diccUsuarioPropiedad.Key.FirstName + " " + diccUsuarioPropiedad.Key.LastName).FontSize(10).FontColor("#607080");

                                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                        .Padding(5).Text(propiedad.Codigo).FontSize(10).FontColor("#607080");

                                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                        .Padding(5).Text(pago.Concepto).FontSize(10).FontColor("#607080");

                                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                        .Padding(5).Text(pago.Monto.ToString("N") + " Bsf").FontSize(10).FontColor("#607080");

                                                        if (pago.FormaPago)
                                                        {
                                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                            .Padding(5).Text("Transferencia").FontSize(10).FontColor("#607080");
                                                        }
                                                        else
                                                        {
                                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                            .Padding(5).Text("Efectivo").FontSize(10).FontColor("#607080");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] LibroDiarioPDF(LibroDiarioVM ldiarioGlobals)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Libro Diario").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {

                            x.Spacing(20);

                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
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
                                });
                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Asiento").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Código").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Descripción").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Concepto").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Debe").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Haber").FontColor("#607080").Bold().FontSize(10);
                                });
                                if (ldiarioGlobals.AsientosCondominio != null)
                                {
                                    foreach (var asiento in ldiarioGlobals.AsientosCondominio)
                                    {
                                        var cc = ldiarioGlobals.CuentasCondominio.Where(c => c.IdCodCuenta == asiento.IdCodCuenta).ToList();

                                        var subcuenta = ldiarioGlobals.CuentasDiarioCondominio.Where(c => c.Id == cc.First().IdSubCuenta).ToList();

                                        var cuenta = ldiarioGlobals.Cuentas.Where(c => c.Id == cc.First().IdCuenta).ToList();

                                        var grupo = ldiarioGlobals.Grupos.Where(c => c.Id == cc.First().IdGrupo).ToList();

                                        var clase = ldiarioGlobals.Clases.Where(c => c.Id == cc.First().IdClase).ToList();


                                        if (asiento.IdCodCuenta == cc.First().IdCodCuenta)
                                        {
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(asiento.NumAsiento.ToString()).FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(asiento.Fecha.ToString("dd/MM/yyyy")).FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text((clase.First().Codigo + grupo.First().Codigo + cuenta.First().Codigo + subcuenta.First().Codigo).ToString()).FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(subcuenta.First().Descricion.ToString()).FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(asiento.Concepto.ToString()).FontSize(10).FontColor("#607080");
                                            if (asiento.TipoOperacion)
                                            {
                                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                .Padding(5).Text(asiento.MontoRef.ToString("N")).FontSize(10).FontColor("#607080");

                                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                .Padding(5).Text("").FontSize(10).FontColor("#607080");
                                            }
                                            else
                                            {
                                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                                .Padding(5).Text(asiento.MontoRef.ToString("N")).FontSize(10).FontColor("#607080");
                                            }
                                        }


                                    }

                                }

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text($"Diferencia: {(ldiarioGlobals.Diferencia.ToString("N"))}").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text($"Total Debe: {(ldiarioGlobals.TotalDebe.ToString("N"))}").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text($"Total Haber: {(ldiarioGlobals.TotalHaber.ToString("N"))}").FontSize(10).FontColor("#607080");

                            });

                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] RelacionGastosPDF(RelacionDeGastosVM relacionDeGastos)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().AlignCenter().Text("Junta de Condominio" + relacionDeGastos.Condominio.Nombre).Bold().FontSize(20).FontColor("#004581");

                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Relación Gastos").Bold().FontSize(20).FontColor("#004581").Bold();

                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {

                            x.Spacing(10);
                            x.Item().AlignCenter().Text(relacionDeGastos.Condominio.Nombre).FontSize(12);
                            x.Item().AlignCenter().Text(relacionDeGastos.Condominio.Rif).FontSize(12);
                            x.Item().AlignCenter().Text("RECIBO DE COBRO MES:" + DateTime.Today.ToString("MM/yyyy")).FontSize(12);

                            x.Item().AlignCenter().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Descripción").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Tasa de Cambio").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto Referencia").Bold().FontSize(10);
                                });
                                if ((relacionDeGastos.SubcuentasGastos != null && relacionDeGastos.SubcuentasGastos.Any())
                                  && (relacionDeGastos.GastosDiario != null && relacionDeGastos.GastosDiario.Any())
                                  && relacionDeGastos.CCGastos != null && relacionDeGastos.CCGastos.Any())
                                {
                                    foreach (var item in relacionDeGastos.GastosDiario)
                                    {
                                        var idcc = relacionDeGastos.CCGastos.Where(c => c.IdCodCuenta == item.IdCodCuenta);
                                        if (idcc != null && idcc.Any())
                                        {
                                            var subcuenta = relacionDeGastos.SubcuentasGastos.Where(c => c.Id == idcc.First().IdSubCuenta).ToList();

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(item.Fecha.ToString("dd/MM/yyyy")).FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(subcuenta.First().Descricion).FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text((item.Monto / item.ValorDolar).ToString("N2")).FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(item.ValorDolar.ToString()).FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(item.Monto.ToString()).FontSize(10);
                                        }
                                    }
                                }
                                else
                                {
                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("No").FontSize(10);

                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("hay").FontSize(10);

                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("Gastos").FontSize(10);

                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("").FontSize(10);

                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("").FontSize(10);
                                }
                                if (relacionDeGastos.Provisiones != null && relacionDeGastos.Provisiones.Any()
                                && relacionDeGastos.SubCuentasProvisiones != null && relacionDeGastos.SubCuentasProvisiones.Any()
                                && relacionDeGastos.CCProvisiones != null && relacionDeGastos.CCProvisiones.Any())
                                {
                                    foreach (var provisiones in relacionDeGastos.Provisiones)
                                    {
                                        var idcc = relacionDeGastos.CCProvisiones.Where(c => c.IdSubCuenta == provisiones.IdCodCuenta);
                                        if (idcc != null && idcc.Any())
                                        {
                                            var subcuenta = relacionDeGastos.SubCuentasProvisiones.Where(c => idcc.First().IdSubCuenta == c.Id).ToList();

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text("").FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(subcuenta.First().Descricion).FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text((provisiones.ValorDolar * provisiones.Monto)
                                            .ToString("N2")).FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(provisiones.ValorDolar.ToString("N"))
                                            .FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(provisiones.Monto.ToString()).FontSize(10);
                                        }
                                    }
                                }

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                             .Padding(5).Text("").FontSize(10);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("SUBTOTAL").FontSize(10);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(10);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text("").FontSize(10);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text(relacionDeGastos.SubTotal.ToString("N")).FontSize(10);

                                if (relacionDeGastos.Fondos != null && relacionDeGastos.Fondos.Any()
                              && relacionDeGastos.SubCuentasFondos != null && relacionDeGastos.SubCuentasFondos.Any()
                              && relacionDeGastos.CCFondos != null && relacionDeGastos.CCFondos.Any())
                                {
                                    foreach (var fondo in relacionDeGastos.Fondos)
                                    {
                                        var idcc = relacionDeGastos.CCFondos.Where(c => c.IdSubCuenta == fondo.IdCodCuenta).ToList();
                                        if (idcc != null && idcc.Any())
                                        {
                                            var subcuenta = relacionDeGastos.SubCuentasFondos.Where(c => c.Id == idcc.First().IdSubCuenta).ToList();

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                             .Padding(5).Text("").FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text($"{subcuenta.First().Descricion} - {fondo.Porcentaje}%").FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text("").FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text("").FontSize(10);

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text((relacionDeGastos.SubTotal * fondo.Porcentaje / 100)
                                            .ToString("N")).FontSize(10);
                                        }
                                    }
                                }
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                          .Padding(5).Text("").FontSize(10).FontColor("#607080");
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("Total gasto del mes").FontSize(10);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text("").FontSize(10);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text("").FontSize(10);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text(relacionDeGastos.Total.ToString("N")).FontSize(10);
                            });
                            x.Item().Column(c =>
                            {
                                c.Item().AlignCenter().Text("Elaborado por Junta de Condominio " + relacionDeGastos.Condominio.Nombre).FontSize(8);
                                c.Item().AlignCenter().Text("Junta de Condominio " + relacionDeGastos.Condominio.Nombre).FontSize(10).Bold();
                                c.Item().AlignCenter().Text("Rif " + relacionDeGastos.Condominio.Rif).FontSize(10).Bold();
                                c.Item().AlignCenter().Text("Enviar al correo: " + relacionDeGastos.Condominio.Email).FontSize(12).Bold().FontColor("#FF0000");
                            });
                        });

                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
          .GeneratePdf();
            return data;
        }
        public byte[] EstadoDeResultadoPDF(EstadoResultadoVM estadoResultado)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Estado de Resultados").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {

                            x.Spacing(20);
                            x.Item().Row(row =>
                            {
                                row.RelativeColumn().Text("Total Egresos: " + estadoResultado.TotalEgresos.ToString("N"));
                                row.RelativeColumn().Text("Total Ingresos: " + estadoResultado.TotalIngresos.ToString("N"));
                                row.RelativeColumn().Text("Diferencia:" + estadoResultado.Difenrencia.ToString("N"));

                            });
                            x.Item().Text("Ingresos").Bold().FontSize(14).FontColor("#004581").Bold();


                            //Tabla ingresos

                            if (estadoResultado.AsientosIngresos != null && estadoResultado.AsientosIngresos.Any()
                                   && estadoResultado.AsientosCondominio != null && estadoResultado.AsientosCondominio.Any()
                                   && estadoResultado.SubCuentas != null && estadoResultado.SubCuentas.Any()
                                   && estadoResultado.Cuentas != null && estadoResultado.Cuentas.Any()
                                   && estadoResultado.Grupos != null && estadoResultado.Grupos.Any()
                                   && estadoResultado.Clases != null && estadoResultado.Clases.Any()
                                   )
                            {
                                x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                                {
                                    tabla.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    tabla.Header(header =>
                                    {
                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("Código").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Descripción").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Concepto").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Monto").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    });

                                    foreach (var ingreso in estadoResultado.AsientosIngresos)
                                    {
                                        // var asiento = estadoResultado.AsientosCondominio.Where(c => c.IdAsiento == ingreso.IdAsiento).ToList();
                                        // var subcuenta = estadoResultado.SubCuentas.Where(c => c.Id == asiento.First().IdCodCuenta).ToList();
                                        // var cuenta = estadoResultado.Cuentas.Where(c => c.Id == subcuenta.First().IdCuenta).ToList();
                                        // var grupo = estadoResultado.Grupos.Where(c => c.Id == cuenta.First().IdGrupo).ToList();
                                        // var clase = estadoResultado.Clases.Where(c => c.Id == grupo.First().IdClase).ToList();

                                        // tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        // .Padding(5).Text((clase.First().Codigo + "." + grupo.First().Codigo + "." + cuenta.First().Codigo
                                        // + "." + subcuenta.First().Codigo)).FontSize(10).FontColor("#607080");


                                        // tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        //.Padding(5).Text(subcuenta.First().Descricion).FontColor("#607080").Bold().FontSize(10);

                                        // tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        //.Padding(5).Text(asiento.First().Concepto).FontColor("#607080").Bold().FontSize(10);

                                        // tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        //.Padding(5).Text(asiento.First().Monto.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                        // tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        // .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
                                    }

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Total Ingresos: " + estadoResultado.TotalIngresos.ToString("N"))
                                    .FontColor("#607080").Bold().FontSize(10);


                                });

                            }
                            else
                            {
                                x.Item().Text("No hay Ingreso").Bold().FontSize(14).FontColor("#004581");
                            }
                            //Tabla Egresos
                            if (estadoResultado.AsientosEgresos != null && estadoResultado.AsientosEgresos.Any()
                                        && estadoResultado.AsientosCondominio != null && estadoResultado.AsientosCondominio.Any()
                                        && estadoResultado.SubCuentas != null && estadoResultado.SubCuentas.Any()
                                        && estadoResultado.Cuentas != null && estadoResultado.Cuentas.Any()
                                        && estadoResultado.Grupos != null && estadoResultado.Grupos.Any()
                                        && estadoResultado.Clases != null && estadoResultado.Clases.Any()
                                        )
                            {
                                x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                                {
                                    tabla.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    tabla.Header(header =>
                                    {
                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("Código").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Descripción").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Concepto").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Monto").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    });

                                    foreach (var egreso in estadoResultado.AsientosEgresos)
                                    {
                                        // var asiento = estadoResultado.AsientosCondominio.Where(c => c.IdAsiento == egreso.IdAsiento).ToList();
                                        // var subcuenta = estadoResultado.SubCuentas.Where(c => c.Id == asiento.First().IdCodCuenta).ToList();
                                        // var cuenta = estadoResultado.Cuentas.Where(c => c.Id == subcuenta.First().IdCuenta).ToList();
                                        // var grupo = estadoResultado.Grupos.Where(c => c.Id == cuenta.First().IdGrupo).ToList();
                                        // var clase = estadoResultado.Clases.Where(c => c.Id == grupo.First().IdClase).ToList();

                                        // tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        // .Padding(5).Text((clase.First().Codigo + "." + grupo.First().Codigo 
                                        // + "." + cuenta.First().Codigo + "." + subcuenta.First().Codigo)).FontSize(10).FontColor("#607080");


                                        // tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        //.Padding(5).Text(subcuenta.First().Descricion).FontColor("#607080").Bold().FontSize(10);

                                        // tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        //.Padding(5).Text(asiento.First().Concepto).FontColor("#607080").Bold().FontSize(10);

                                        // tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        //.Padding(5).Text(asiento.First().Monto.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                        // tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        // .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
                                    }

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Total Egresos: " + estadoResultado.TotalEgresos.ToString("N"))
                                    .FontColor("#607080").Bold().FontSize(10);


                                });

                            }
                            else
                            {
                                x.Item().Text("No hay Egresos").Bold().FontSize(14).FontColor("#004581").Bold();
                            }

                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
          .GeneratePdf();
            return data;
        }

        public void BalanceComprobacionPDF()
        {

        }
        public byte[] PagosEmitidosPDF(IndexPagosVM indexPagosVM)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Pagos Emitidos").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {

                            x.Spacing(20);
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
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
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Tasa de Cambio").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto ref").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Forma de pago").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Referencia").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Banco").FontColor("#607080").Bold().FontSize(10);

                                });

                                if (indexPagosVM.PagosEmitidos != null && indexPagosVM.PagosEmitidos.Count() > 0)
                                {
                                    foreach (var item in indexPagosVM.PagosEmitidos)
                                    {
                                        if (item.FormaPago)
                                        {
                                            if (indexPagosVM.BancosCondominio != null && indexPagosVM.BancosCondominio.Any()
                                             && indexPagosVM.Referencias != null && indexPagosVM.Referencias.Any())
                                            {
                                                foreach (var referencia in indexPagosVM.Referencias)
                                                {
                                                    if (item.IdPagoEmitido == referencia.IdPagoEmitido)
                                                    {
                                                        var idBanco = Convert.ToInt32(referencia.Banco);
                                                        var banco = indexPagosVM.BancosCondominio.Where(c => c.Id == idBanco).First();

                                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                                        .Padding(5).Text(item.Fecha.ToString("dd/MM/yyyy")).FontColor("#607080").Bold().FontSize(10);

                                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                                        .Padding(5).Text((item.Monto.ToString("N")) + " " +
                                                        item.SimboloMoneda).FontColor("#607080").Bold().FontSize(10);

                                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                                        .Padding(5).Text(item.ValorDolar.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                                        .Padding(5).Text(item.MontoRef.ToString("N") + " "
                                                        + item.SimboloRef).FontColor("#607080").Bold().FontSize(10);

                                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                                        .Padding(5).Text(FormaPago.Transferencia).FontColor("#607080").Bold().FontSize(10);

                                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                                        .Padding(5).Text(referencia.NumReferencia).FontColor("#607080").Bold().FontSize(10);

                                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                                       .Padding(5).Text(banco.Descricion).FontColor("#607080").Bold().FontSize(10);

                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(item.Fecha.ToString("dd/MM/yyyy")).FontColor("#607080").Bold().FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(item.Monto.ToString("N") + " " +
                                            item.SimboloMoneda).FontColor("#607080").Bold().FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(item.ValorDolar.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(item.MontoRef.ToString("N") + " "
                                            + item.SimboloRef).FontColor("#607080").Bold().FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(FormaPago.Efectivo).FontColor("#607080").Bold().FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                           .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                           .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
                                        }
                                    }
                                }
                                else
                                {
                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("No").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Existen").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Datos").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
                                }



                            });


                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
         .GeneratePdf();
            return data;
        }

        public byte[] ComprobantePagosNominaPDF(ComprobantePagoNomina pagoNomina)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                            col.Item().PaddingTop(10).Text("Fecha: " + DateTime.Today.ToString("MM/yyyy")).Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text("FACTURA: " + comprobanteVM.Factura.NumControl).Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text("Beneficiario").Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text(comprobanteVM.Beneficiario).Bold().FontSize(14).FontColor("#004581");
                            col.Item().Text("Concepto").Bold().FontSize(14).FontColor("#004581").Bold();
                            col.Item().Text(pagoNomina.Concepto).Bold().FontSize(14).FontColor("#004581");

                        });
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Text("CONSTANCIA DE PAGO").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().AlignCenter().Text("DATOS NOMINA").Bold();
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Cell().Padding(5).Text("Nombre y Apellido:").Bold();
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text(pagoNomina.Empleado.Nombre);
                                tabla.Cell().Padding(5).Text(pagoNomina.Empleado.Apellido);

                                tabla.Cell().Padding(5).Text("Ingreso:").Bold();
                                tabla.Cell().Padding(5).Text(pagoNomina.Empleado.FechaIngreso.ToString("dd/MM/yyyy"));
                                tabla.Cell().Padding(5).Text("Sueldo Base:").Bold();
                                tabla.Cell().Padding(5).Text(pagoNomina.Empleado.BaseSueldo.ToString("N") + " Bs");

                                if (pagoNomina.Percepciones.Any())
                                {
                                    tabla.Cell().Padding(5).Text("Percepciones").Bold();

                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text("");

                                    foreach (var percepcion in pagoNomina.Percepciones)
                                    {
                                        tabla.Cell().Padding(5).Text("Concepto:").Bold();
                                        tabla.Cell().Padding(5).Text(percepcion.Concepto);
                                        tabla.Cell().Padding(5).Text("Monto:").Bold();
                                        tabla.Cell().Padding(5).Text("+"+percepcion.Monto.ToString("N")+" Bs");
                                    }
                                }

                                if (pagoNomina.Deducciones.Any())
                                {
                                    tabla.Cell().Padding(5).Text("Deducciones").Bold();
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text("");
                                    foreach (var deduccion in pagoNomina.Deducciones)
                                    {
                                        tabla.Cell().Padding(5).Text("Concepto:").Bold();
                                        tabla.Cell().Padding(5).Text(deduccion.Concepto);
                                        tabla.Cell().Padding(5).Text("Monto:").Bold();
                                        tabla.Cell().Padding(5).Text("-"+deduccion.Monto.ToString("N")+" Bs");
                                    }
                                }

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("Total a pagar ").FontSize(12).Bold();
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text((pagoNomina.Pago.Monto).ToString("N") + " Bs").FontSize(12);

                            });

                            x.Item().AlignCenter().Text("DATOS PAGO").Bold();
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Cell()
                               .Padding(5).Text("Forma de Pago");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (pagoNomina.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(3).Text("Transferencia").FontSize(12);

                                }
                                else
                                {
                                    tabla.Cell().Padding(3).Text("Efectivo").FontSize(12);

                                }

                                tabla.Cell().Padding(5).Text("Fecha");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text(pagoNomina.Pago.Fecha.ToString("dd/MM/yyyy")).FontSize(12);

                                tabla.Cell().Padding(5).Text("Cuenta");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (pagoNomina.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(5).Text(pagoNomina.Banco.Descricion).FontSize(12);

                                }
                                else
                                {
                                    tabla.Cell().Padding(5).Text(pagoNomina.Caja.Descricion).FontSize(12);

                                }

                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("Monto").FontSize(12);

                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);


                                tabla.Cell().Padding(5).Text((pagoNomina.Pago.Monto).ToString("N")+" Bs").FontSize(12);

                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                            });
                            x.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });


                                table.Cell().Padding(5).Text("Elaborado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Revisado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Karina Lopez");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Roberto Villagras");
                                table.Cell().Padding(5).Text("Massimo Ruggiero");



                            });
                        });

                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }

        public byte[] ComprobanteBonosPDF(ComprobantePagoNomina pagoNomina)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                            col.Item().PaddingTop(10).Text("Fecha: " + DateTime.Today.ToString("MM/yyyy")).Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text("FACTURA: " + comprobanteVM.Factura.NumControl).Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text("Beneficiario").Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text(comprobanteVM.Beneficiario).Bold().FontSize(14).FontColor("#004581");
                            col.Item().Text("Concepto").Bold().FontSize(14).FontColor("#004581").Bold();
                            col.Item().Text(pagoNomina.Concepto).Bold().FontSize(14).FontColor("#004581");

                        });
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Text("CONSTANCIA DE PAGO").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().AlignCenter().Text("DATOS BONIFICACIONES").Bold();
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Cell().Padding(5).Text("Nombre y Apellido:").Bold();
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text(pagoNomina.Empleado.Nombre);
                                tabla.Cell().Padding(5).Text(pagoNomina.Empleado.Apellido);

                                //tabla.Cell().Padding(5).Text("Ingreso:").Bold();
                                //tabla.Cell().Padding(5).Text(pagoNomina.Empleado.FechaIngreso.ToString("dd/MM/yyyy"));
                                //tabla.Cell().Padding(5).Text("Sueldo Base:").Bold();
                                //tabla.Cell().Padding(5).Text(pagoNomina.Empleado.BaseSueldo.ToString("N") + " Bs");
                                decimal totalBonos = 0;
                                if (pagoNomina.Bonos.Any())
                                {
                                    tabla.Cell().Padding(5).Text("Bonos").Bold();

                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text("");
                                    tabla.Cell().Padding(5).Text("");

                                    foreach (var bono in pagoNomina.Bonos)
                                    {
                                        tabla.Cell().Padding(5).Text("Concepto:").Bold();
                                        tabla.Cell().Padding(5).Text(bono.Concepto);
                                        tabla.Cell().Padding(5).Text("Monto:").Bold();
                                        tabla.Cell().Padding(5).Text("+" + bono.Monto.ToString("N") + " Bs");

                                        totalBonos += bono.Monto;
                                    }
                                }

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("Total bonos:").FontSize(12).Bold();
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text((totalBonos).ToString("N") + " Bs").FontSize(12);

                            });

                            x.Item().AlignCenter().Text("DATOS PAGO").Bold();
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Cell()
                               .Padding(5).Text("Forma de Pago");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (pagoNomina.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(3).Text("Transferencia").FontSize(12);

                                }
                                else
                                {
                                    tabla.Cell().Padding(3).Text("Efectivo").FontSize(12);

                                }

                                tabla.Cell().Padding(5).Text("Fecha");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text(pagoNomina.Pago.Fecha.ToString("dd/MM/yyyy")).FontSize(12);

                                tabla.Cell().Padding(5).Text("Cuenta");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (pagoNomina.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(5).Text(pagoNomina.Banco.Descricion).FontSize(12);

                                }
                                else
                                {
                                    tabla.Cell().Padding(5).Text(pagoNomina.Caja.Descricion).FontSize(12);

                                }

                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("Monto").FontSize(12);

                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);


                                tabla.Cell().Padding(5).Text((pagoNomina.Pago.Monto).ToString("N") + " Bs").FontSize(12);

                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                            });
                            x.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });


                                table.Cell().Padding(5).Text("Elaborado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Revisado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Karina Lopez");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Roberto Villagras");
                                table.Cell().Padding(5).Text("Massimo Ruggiero");



                            });
                        });

                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] ComprobanteOrdenPagoPDF(ComprobanteOrdenPago modelo)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                            col.Item().PaddingTop(10).Text("Fecha: " + DateTime.Today.ToString("MM/yyyy")).Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text("FACTURA: " + comprobanteVM.Factura.NumControl).Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text("Beneficiario").Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text(comprobanteVM.Beneficiario).Bold().FontSize(14).FontColor("#004581");
                            col.Item().Text("Concepto").Bold().FontSize(14).FontColor("#004581").Bold();
                            col.Item().Text(modelo.Concepto).Bold().FontSize(14).FontColor("#004581");

                        });
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Text("CONSTANCIA DE PAGO").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().AlignCenter().Text("ORDEN DE PAGO").Bold();
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Cell().Padding(5).Text("Beneficiario:").Bold();
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text(modelo.Beneficiario);                           

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                            });

                            x.Item().AlignCenter().Text("DATOS PAGO").Bold();
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Cell()
                               .Padding(5).Text("Forma de Pago");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (modelo.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(3).Text("Transferencia").FontSize(12);

                                }
                                else
                                {
                                    tabla.Cell().Padding(3).Text("Efectivo").FontSize(12);

                                }

                                tabla.Cell().Padding(5).Text("Fecha");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text(modelo.Pago.Fecha.ToString("dd/MM/yyyy")).FontSize(12);

                                tabla.Cell().Padding(5).Text("Cuenta");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (modelo.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(5).Text(modelo.Banco.Descricion).FontSize(12);

                                }
                                else
                                {
                                    tabla.Cell().Padding(5).Text(modelo.Caja.Descricion).FontSize(12);

                                }

                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("Monto").FontSize(12);

                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);


                                tabla.Cell().Padding(5).Text((modelo.Pago.Monto).ToString("N") + " Bs").FontSize(12);

                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                            });
                            x.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });


                                table.Cell().Padding(5).Text("Elaborado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Revisado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Karina Lopez");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Roberto Villagras");
                                table.Cell().Padding(5).Text("Massimo Ruggiero");



                            });
                        });

                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] ComprobanteAnticipoPDF(ComprobanteAnticipoVM modelo)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Image("wwwroot/images/logo-1.png");
                            col.Item().PaddingTop(10).Text("Fecha: " + DateTime.Today.ToString("MM/yyyy")).Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text("FACTURA: " + comprobanteVM.Factura.NumControl).Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text("Beneficiario").Bold().FontSize(14).FontColor("#004581").Bold();
                            //col.Item().Text(comprobanteVM.Beneficiario).Bold().FontSize(14).FontColor("#004581");
                            col.Item().Text("Concepto").Bold().FontSize(14).FontColor("#004581").Bold();
                            col.Item().Text(modelo.Concepto).Bold().FontSize(14).FontColor("#004581");

                        });
                        row.RelativeItem().Padding(15).Column(col =>
                        {
                            col.Item().Text("CONSTANCIA DE PAGO").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().AlignCenter().Text("ANTICIPO").Bold();
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Cell().Padding(5).Text("Beneficiario:").Bold();
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text(modelo.Beneficiario);

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                            });

                            x.Item().AlignCenter().Text("DATOS PAGO").Bold();
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Cell()
                               .Padding(5).Text("Forma de Pago");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (modelo.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(3).Text("Transferencia").FontSize(12);

                                }
                                else
                                {
                                    tabla.Cell().Padding(3).Text("Efectivo").FontSize(12);

                                }

                                tabla.Cell().Padding(5).Text("Fecha");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text(modelo.Pago.Fecha.ToString("dd/MM/yyyy")).FontSize(12);

                                tabla.Cell().Padding(5).Text("Cuenta");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                if (modelo.Pagoforma == FormaPago.Transferencia)
                                {
                                    tabla.Cell().Padding(5).Text(modelo.Banco.Descricion).FontSize(12);

                                }
                                else
                                {
                                    tabla.Cell().Padding(5).Text(modelo.Caja.Descricion).FontSize(12);

                                }

                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");
                                tabla.Cell().Padding(5).Text("");

                                tabla.Cell().Padding(5).Text("Monto").FontSize(12);

                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);


                                tabla.Cell().Padding(5).Text((modelo.Pago.Monto).ToString("N") + " Bs").FontSize(12);

                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                                tabla.Cell().Padding(5).Text("").FontSize(8);
                            });
                            x.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });


                                table.Cell().Padding(5).Text("Elaborado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Revisado por:").Bold();
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Karina Lopez");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("______________________");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("");
                                table.Cell().Padding(5).Text("Roberto Villagras");
                                table.Cell().Padding(5).Text("Massimo Ruggiero");



                            });
                        });

                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        // CRISTHIAN
        public byte[] Transacciones(TransaccionVM transaccion)
        {
            decimal totalIngresosSector = 0;
            decimal totalEgresoSector = 0;
            decimal totalIngresosIndividual = 0;
            decimal totalEgresoIndividual = 0;


            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("CONDIOMINIO " + transaccion.Condominio.Nombre).Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Fecha: " + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(16).FontColor("#004581");
                            col.Item().AlignRight().Text(x =>
                            {
                                x.Span("Página: " + x.CurrentPageNumber()).FontSize(14);
                            });
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignRight().Text("Transacciones del Mes: FEBRERO 2024").Bold().FontSize(18).FontColor("#004581");
                            x.Item().AlignRight().Text("Desde: 01/02/2024 Hasta: 29/02/2024").FontSize(16).FontColor("#004581");

                            x.Spacing(20);
                            x.Item().AlignRight().Text("Sector").FontSize(16).FontColor("#004581");
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
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
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Trans. Ordinarias°").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Descripción").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Documento").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Inmueble").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Egresos").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Ingresos").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Saldo").FontColor("#607080").Bold().FontSize(10);

                                });
                                //foreach (var item in transaccion.Transaccions)
                                //{
                                //    decimal totalGrupoGastosEgreso = 0;
                                //    decimal totalGrupoGastosIngresos = 0;

                                //    foreach (var grupo in transaccion.Transaccions)
                                //    {
                                //        if (item.IdGrupoGasto == grupo.IdGrupoGasto)
                                //        {
                                //            if (item.IdPropiedad == null)
                                //            {
                                //                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //         .Padding(5).Text(item.Descripcion).FontColor("#607080").Bold().FontSize(10);

                                //                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                .Padding(5).Text(item.NumDocumento).FontColor("#607080").Bold().FontSize(10);

                                //                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                //                if (item.TipoTransaccion)
                                //                {
                                //                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                    .Padding(5).Text(item.MontoTotal).FontColor("#607080").FontSize(10);

                                //                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                    .Padding(5).Text("0.00").FontColor("#607080").FontSize(10);
                                //                    totalGrupoGastosIngresos = (decimal)(totalGrupoGastosIngresos + Convert.ToDecimal(item.MontoTotal));
                                //                    totalIngresosSector = (decimal)(totalIngresosSector + Convert.ToDecimal(item.MontoTotal));
                                //                }
                                //                else
                                //                {
                                //                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                    .Padding(5).Text("0.00").FontColor("#607080").FontSize(10);

                                //                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                    .Padding(5).Text(item.MontoTotal).FontColor("#607080").FontSize(10);
                                //                    totalGrupoGastosEgreso = (decimal)(totalGrupoGastosEgreso + Convert.ToDecimal(item.MontoTotal));
                                //                    totalEgresoSector = (decimal)(totalEgresoSector + Convert.ToDecimal(item.MontoTotal));
                                //                }
                                //            }
                                //        }
                                //    }
                                //    var nombreGrupo = _context.GrupoGastos.Where(c => c.IdGrupoGasto == item.IdGrupoGasto).Select(c => c.NombreGrupo).First();
                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text("Total " + nombreGrupo).FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text(totalGrupoGastosEgreso.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text(totalIngresosSector.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
                                //}

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Total Ordinarias => ").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(totalEgresoSector.ToString("N")).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(totalIngresosSector.ToString("N")).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text((totalEgresoSector - totalIngresosSector).ToString("N")).FontColor("#607080").FontSize(10);
                            });
                            x.Spacing(20);
                            x.Item().AlignRight().Text("Individual").FontSize(16).FontColor("#004581");
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(5);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Trans. Ordinarias°").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Descripción").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Documento").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Inmueble").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Egresos").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Ingresos").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Saldo").FontColor("#607080").Bold().FontSize(10);

                                });
                                //foreach (var item in transaccion.Transaccions)
                                //{
                                //    decimal totalGrupoGastosEgreso = 0;
                                //    decimal totalGrupoGastosIngresos = 0;

                                //    foreach (var grupo in transaccion.Transaccions)
                                //    {
                                //        if (item.IdGrupoGasto == grupo.IdGrupoGasto)
                                //        {
                                //            if (item.IdPropiedad == null)
                                //            {
                                //                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //         .Padding(5).Text(item.Descripcion).FontColor("#607080").Bold().FontSize(10);

                                //                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                .Padding(5).Text(item.NumDocumento).FontColor("#607080").Bold().FontSize(10);

                                //                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                //                if (item.TipoTransaccion)
                                //                {
                                //                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                    .Padding(5).Text(item.MontoTotal).FontColor("#607080").FontSize(10);

                                //                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                    .Padding(5).Text("0.00").FontColor("#607080").FontSize(10);
                                //                    totalGrupoGastosIngresos = (decimal)(totalGrupoGastosIngresos + Convert.ToDecimal(item.MontoTotal));
                                //                    totalIngresosIndividual = (decimal)(totalIngresosIndividual + Convert.ToDecimal(item.MontoTotal));
                                //                }
                                //                else
                                //                {
                                //                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                    .Padding(5).Text("0.00").FontColor("#607080").FontSize(10);

                                //                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //                    .Padding(5).Text(item.MontoTotal).FontColor("#607080").FontSize(10);
                                //                    totalGrupoGastosEgreso = (decimal)(totalGrupoGastosEgreso + Convert.ToDecimal(item.MontoTotal));
                                //                    totalEgresoIndividual = (decimal)(totalEgresoIndividual + Convert.ToDecimal(item.MontoTotal));
                                //                }
                                //            }
                                //        }
                                //    }
                                //    var nombreGrupo = _context.GrupoGastos.Where(c => c.IdGrupoGasto == item.IdGrupoGasto).Select(c => c.NombreGrupo).First();
                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text("Total " + nombreGrupo).FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text(totalGrupoGastosEgreso.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text(totalIngresosSector.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                //    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                //    .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
                                //}

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Total Individual => ").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text((totalEgresoIndividual + totalEgresoSector).ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text((totalIngresosIndividual + totalIngresosSector).ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(((totalEgresoIndividual + totalEgresoSector) - (totalIngresosIndividual + totalIngresosSector)).ToString("N")).FontColor("#607080").Bold().FontSize(10);
                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
         .GeneratePdf();
            return data;
        }
        public byte[] MontosPDF(List<MontosVW> montosVW)
        {
            decimal totalSubTotal = 0;
            decimal totalInteresMora = 0;
            decimal totalGastosCod = 0;
            decimal totalCuotaMes = 0;
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("CONDIOMINIO" + " ").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Fecha: " + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(16).FontColor("#004581");
                            col.Item().AlignRight().Text(x =>
                            {
                                x.Span("Página: " + x.CurrentPageNumber()).FontSize(14);
                            });
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignRight().Text("LISTADO MONTOS DEL MES: ENERO 2024").Bold().FontSize(18).FontColor("#004581");
                            x.Item().AlignRight().Text("Desde: 01/02/2024 Hasta: 29/02/2024").FontSize(16).FontColor("#004581");

                            x.Spacing(20);
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
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
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Código").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Propietario").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Grupos").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Sub-total").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Interés Mora").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Gastos Cob.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total Cuota Mes").FontColor("#607080").Bold().FontSize(10);

                                });
                                foreach (var item in montosVW)
                                {
                                    var grupos = "";
                                    tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(item.propiedad.Codigo).Bold().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(item.propiedad.IdUsuario).FontColor("#607080").Bold().FontSize(10);

                                    foreach (var grupo in item.grupo)
                                    {
                                        grupos = grupos + " " + grupo.Codigo;
                                    }

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(grupos).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.propiedad.Deuda.ToString("N")).FontColor("#607080").Bold().FontSize(10);
                                    totalSubTotal += item.propiedad.Deuda;

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(item.propiedad.MontoIntereses.ToString("N")).FontColor("#607080").Bold().FontSize(10);
                                    totalInteresMora += item.propiedad.MontoIntereses;

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(item.propiedad.MontoMulta).FontColor("#607080").Bold().FontSize(10);
                                    totalGastosCod += item.propiedad.MontoIntereses;

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.propiedad.Saldo.ToString("N")).FontColor("#607080").Bold().FontSize(10);
                                    totalCuotaMes += item.propiedad.MontoIntereses;
                                }
                                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Totales =>").Bold().FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(totalSubTotal.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(totalInteresMora.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(totalGastosCod.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(totalCuotaMes.ToString("N")).FontColor("#607080").Bold().FontSize(10);
                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
         .GeneratePdf();
            return data;
        }
        public byte[] ComprobanteRetencionesISLR(ComprobanteRetencionesISLRVM comprobante)
        {

            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignRight().Text("Comprobante de Retención de Impuesto Sobre La Renta").Bold().FontSize(18).FontColor("#004581");
                            x.Item().AlignRight().Text("Decreto Retenciones vigente N° 1.808, Gaceta Oficial 36.203 de fecha 12-05-1997").Bold().FontSize(18).FontColor("#004581");

                            x.Spacing(20);

                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("N° Comprobante").FontColor("#607080").Bold().FontSize(10);

                                });

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(comprobante.ComprobanteRetencion.FechaEmision.ToString("dd/MM/yyyy")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(comprobante.ComprobanteRetencion.IdComprobante.ToString()).FontColor("#607080").Bold().FontSize(10);
                            });

                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(100);
                                });

                                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("DATOS DEL AGENTE DE RETENCIÓN").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Nombre o Razón Social").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(comprobante.Condominio.Nombre).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Dirección Fiscal").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(comprobante.Condominio.Direccion).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("RIF").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(comprobante.Condominio.Rif).FontColor("#607080").Bold().FontSize(10);
                            });
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(100);
                                });

                                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("DATOS DEL AGENTE RETENIDO").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Nombre o Razón Social").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(comprobante.Proveedor.Nombre).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Dirección Fiscal").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(comprobante.Proveedor.Direccion).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("RIF").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(comprobante.Proveedor.Rif).FontColor("#607080").Bold().FontSize(10);
                            });
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
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
                                    columns.RelativeColumn();
                                });
                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha Emisión").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Nro. Factura").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Nro. Control").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Descripción Retención").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Monto Total").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Base Retención").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("% Retención").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Sustraendo").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Valor Retención").FontColor("#607080").Bold().FontSize(10);

                                });
                                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(comprobante.ComprobanteRetencion.FechaEmision.ToString("dd/MM/yyyy")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(comprobante.ComprobanteRetencion.IdFactura).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("00" + comprobante.ComprobanteRetencion.IdFactura).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(comprobante.ComprobanteRetencion.Descripcion).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(comprobante.ComprobanteRetencion.TotalImpuesto).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(comprobante.ComprobanteRetencion.Retencion).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("3.00").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(comprobante.ComprobanteRetencion.Sustraendo).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(comprobante.ComprobanteRetencion.ValorRetencion).FontColor("#607080").Bold().FontSize(10);
                            });

                            x.Item().Row(x =>
                            {
                                x.RelativeItem().Text("");
                                x.RelativeItem().Text("");
                                x.RelativeItem().Text("");
                                x.RelativeItem().Text("");
                                x.RelativeItem().Text("");
                                x.RelativeItem().Text(comprobante.ComprobanteRetencion.Retencion);
                                x.RelativeItem().Text("");
                                x.RelativeItem().Text("");
                            });

                            x.Item().Row(x =>
                            {
                                x.RelativeItem().Text("TOTAL DE IMPUESTO RETENIDO =>");
                                x.RelativeItem().Text(comprobante.ComprobanteRetencion.ValorRetencion);
                            });

                            x.Item().Row(x =>
                            {
                                x.RelativeItem().Text("____________________________");
                                x.RelativeItem().Text("CONDOMINIO " + " sdas");
                            });

                            x.Item().Row(x =>
                            {
                                x.RelativeItem().Text("____________________________");
                                x.RelativeItem().Text("Firma del Beneficiario");
                            });
                        });


                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
         .GeneratePdf();
            return data;
        }
        public byte[] ComprobanteRetencionesIVA(ComprobanteRetencionesIVAVM retencionesIVAVM)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignCenter().Text("COMPROBANTE DE RETENCIÓN DEL IMPUESTO AL VALOR AGREGADO").Bold().FontSize(18).FontColor("#004581");
                            x.Item().AlignCenter().Text("(Ley IVA - Art.11.\"Serán responsables del pago del Impuesto en canlidad de agente de retención)").FontSize(14).FontColor("#004581");
                            x.Item().AlignCenter().Text("comprodaores o adqirientes de determinados bienes muebles y los receptores de ciertos servicios, a").FontSize(14).FontColor("#004581");
                            x.Item().AlignCenter().Text("quienes la Administración Tributaria designe como tal.\"").FontSize(14).FontColor("#004581");
                            x.Spacing(20);


                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("N° Comprobante").FontColor("#607080").Bold().FontSize(10);

                                });

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(retencionesIVAVM.compRetIva.FechaEmision.ToString("dd/MM/yyyy")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.IdComprobanteIva).FontColor("#607080").Bold().FontSize(10);
                            });

                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(100);
                                });

                                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("DATOS DEL AGENTE DE RETENCIÓN").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Nombre o Razón Social").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(retencionesIVAVM.Condominio.Nombre).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Dirección Fiscal").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(retencionesIVAVM.Condominio.Direccion).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("RIF").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.Condominio.Rif).FontColor("#607080").Bold().FontSize(10);
                            });
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(100);
                                });

                                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("DATOS DEL AGENTE RETENIDO").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Nombre o Razón Social").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(retencionesIVAVM.Proveedor.Nombre).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Dirección Fiscal").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(retencionesIVAVM.Proveedor.Direccion).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("RIF").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.Proveedor.Rif).FontColor("#607080").Bold().FontSize(10);
                            });
                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
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
                                    columns.RelativeColumn();
                                });
                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha Factura").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Número Factura").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Número Control").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Nro. Nota Crédito").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Tipo Trans.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Número Factura Afectada").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total Compras Incluyendo el I.V.A.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total Compras Incluyendo el I.V.A.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Base imponible").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Alicuota").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Imp. I.V.A.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("I.V.A Retenido").FontColor("#607080").Bold().FontSize(10);

                                });
                                tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(retencionesIVAVM.compRetIva.FechaEmision.ToString("dd/MM/yyyy")).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(retencionesIVAVM.compRetIva.IdFactura).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("00" + retencionesIVAVM.compRetIva.IdFactura).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(retencionesIVAVM.compRetIva.IdNotaDebito).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(retencionesIVAVM.compRetIva.IdNotaCredito).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.TipoTransaccion).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.NumFacturaAfectada).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.TotalCompraIva.ToString("N")).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.CompraSinCreditoIva.ToString("N")).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.BaseImponible.ToString("N")).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.Alicuota.ToString("N")).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.ImpIva.ToString("N")).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.IvaRetenido.ToString("N")).FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("Totales =>").Bold().FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.TotalCompraIva.ToString("N")).Bold().FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.CompraSinCreditoIva.ToString("N")).Bold().FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.BaseImponible.ToString("N")).Bold().FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.Alicuota.ToString("N")).Bold().FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.ImpIva.ToString("N")).Bold().FontColor("#607080").FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(retencionesIVAVM.compRetIva.IvaRetenido.ToString("N")).Bold().FontColor("#607080").FontSize(10);

                                tabla.Cell().AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().ColumnSpan(4).AlignMiddle()
                                .Padding(5).Text("TOTAL COMPRAS MENOS I.V.A. RETENIDO =>").Bold().FontColor("#607080").FontSize(10);

                                tabla.Cell().AlignMiddle()
                                .Padding(5).Text("").FontColor("#607080").FontSize(10);

                                tabla.Cell().AlignMiddle()
                                .Padding(5).Text(retencionesIVAVM.compRetIva.TotalCompraIva).Bold().FontColor("#607080").FontSize(10);

                            });
                            x.Spacing(60);

                            x.Item().Row(x =>
                            {
                                x.RelativeItem().Text("____________________________");
                                x.RelativeItem().Text("CONDOMINIO " + " sdas");
                                x.RelativeItem().Text("");
                                x.RelativeItem().Text("____________________________");
                                x.RelativeItem().Text("Firma del Beneficiario");
                            });

                            x.Item().Row(x =>
                            {

                            });
                        });


                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {

                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
         .GeneratePdf();
            return data;
        }
        public byte[] LibroVentas(List<LibroVentasVM> libroVentas)
        {
            decimal TotalVentasIva = 0;
            decimal VentasExentas = 0;
            decimal VentasGravables = 0;
            decimal Iva = 0;
            decimal IvaRet = 0;


            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("CONDIOMINIO" + " ").Bold().FontSize(20).FontColor("#004581").Bold();
                            col.Item().Text("Caracas").FontSize(16).FontColor("#004581");
                            col.Item().Text("Venezuela").FontSize(16).FontColor("#004581");
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Fecha: " + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(16).FontColor("#004581");
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignCenter().Text("Libro de Ventas I.V.A").Bold().FontSize(18).FontColor("#004581");
                            x.Item().AlignRight().Text("Desde: 01/02/2024 Hasta: 29/02/2024").FontSize(16).FontColor("#004581");

                            x.Spacing(20);

                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
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
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("N°").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("R.I.F.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Nombre o Rázón Social").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Número Docum.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Número Control").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Número Control").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Tipo Tran.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Nro. Fac. Afectada").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total Ventas incluye I.V.A.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Ventas Exentas").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Ventas Gravables").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("I.V.A.").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("I.V.A. Retenido").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Comprob. Retención").FontColor("#607080").Bold().FontSize(10);

                                });
                                foreach (var item in libroVentas)
                                {
                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.libroVenta.NumComprobanteRet).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.FacturaEmitida.FechaEmision.ToString("dd/MM/yyyy")).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.cliente.Rif).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.cliente.Nombre).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("FAC").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.FacturaEmitida.NumFactura.ToString()).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.FacturaEmitida.NumControl).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("FAC").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.libroVenta.NumComprobanteRet).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.libroVenta.TotalVentaIva?.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.libroVenta.VentaExenta?.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.libroVenta.VentaGravable?.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.libroVenta.Iva.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.libroVenta.IvaRetenido?.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(item.libroVenta.ComprobanteRetencion).FontColor("#607080").Bold().FontSize(10);
                                    TotalVentasIva = TotalVentasIva + (decimal)item.libroVenta.TotalVentaIva;
                                    VentasExentas = VentasExentas + (decimal)item.libroVenta.VentaExenta;
                                    VentasGravables = VentasGravables + (decimal)item.libroVenta.VentaGravable;
                                    Iva = Iva + (decimal)item.libroVenta.Iva;
                                    IvaRet = IvaRet + (decimal)item.libroVenta.IvaRetenido;
                                }


                                tabla.Cell().ColumnSpan(3).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Totales=>").Bold().FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(TotalVentasIva.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(VentasExentas.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(VentasGravables.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(Iva.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(IvaRet.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
         .GeneratePdf();
            return data;
        }
        public byte[] EstadoDeCuentas(List<EstadoCuentasVM> estadoCuentasVM)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("CONDIOMINIO" + " ").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Fecha: " + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(16).FontColor("#004581");
                            col.Item().AlignRight().Text(x =>
                            {
                                x.Span("Página: " + x.CurrentPageNumber()).FontSize(14);
                            });
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignRight().Text("Estado de Cuentas").Bold().FontSize(18).FontColor("#004581");
                            x.Item().AlignLeft().Text("Montos Reflejados: Toda la Deuda - Con Anticipos").FontSize(16).FontColor("#004581");
                            x.Spacing(20);
                            foreach (var item in estadoCuentasVM)
                            {
                                decimal totalIntereses = 0;
                                decimal acumulado = 0;
                                x.Item().Row(row =>
                                {
                                    row.RelativeItem().AlignLeft().Text("OFICINA: " + item.Propiedad.Codigo + " " + item.AspNetUser.FirstName + " " + item.AspNetUser.LastName).FontSize(16).FontColor("#004581");
                                    row.RelativeItem().AlignLeft().Text("Telefonos: " + item.AspNetUser.PhoneNumber).FontSize(16).FontColor("#004581");
                                });
                                x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
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
                                    });

                                    tabla.Header(header =>
                                    {
                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("Tipo").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("Emisión").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Número").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Detalle").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Monto").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Intereses").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Multa").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Créditos").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Saldo").FontColor("#607080").Bold().FontSize(10);

                                        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text("Acumulado").FontColor("#607080").Bold().FontSize(10);

                                    });

                                    foreach (var propiedad in estadoCuentasVM)
                                    {
                                        if (item.Propiedad.IdPropiedad == propiedad.Propiedad.IdPropiedad)
                                        {
                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text("FAC").FontColor("#607080").FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(propiedad.ReciboCobro.Fecha.ToString("dd/MM/yyyy")).FontColor("#607080").FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(propiedad.ReciboCobro.IdReciboCobro + "-" + propiedad.ReciboCobro.Fecha.ToString("MMMM/yyyy")).Bold().FontColor("#607080").FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text("Condominio Mes: " + propiedad.ReciboCobro.Fecha.ToString("MMMM/yyyy")).FontColor("#607080").FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(propiedad.ReciboCobro.Monto.ToString("N")).FontColor("#607080").FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(propiedad.Propiedad.MontoIntereses.ToString("N")).FontColor("#607080").FontSize(10);
                                            totalIntereses += propiedad.Propiedad.MontoIntereses;
                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(propiedad.Propiedad.MontoMulta?.ToString("N")).FontColor("#607080").FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(propiedad.Propiedad.Creditos?.ToString("N")).FontColor("#607080").FontSize(10);

                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(propiedad.Propiedad.Saldo.ToString("N")).FontColor("#607080").FontSize(10);
                                            acumulado += propiedad.Propiedad.Saldo;
                                            tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(acumulado.ToString("N")).FontColor("#607080").FontSize(10);
                                        }
                                    }
                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().ColumnSpan(2).Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(totalIntereses.ToString("N")).Bold().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(acumulado.ToString("N")).FontColor("#607080").Bold().FontSize(10);
                                });
                                x.Spacing(20);
                            }

                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
         .GeneratePdf();
            return data;
        }
        public byte[] ReciboCobroPDF(List<ReciboCobroVM> reciboCobroVM)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    foreach (var item in reciboCobroVM)
                    {
                        decimal totalGastosGenerales = 0;
                        decimal totalGastosIndividuales = 0;
                        decimal totalGasto1 = 0;
                        decimal totalCuota = 0;
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.Header().ShowOnce().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("CONDIOMINIO" + " ").Bold().FontSize(20).FontColor("#004581").Bold();
                                col.Item().Text("Caracas").Bold().FontSize(18).FontColor("#004581").Bold();
                                col.Item().Text("Venezuela").Bold().FontSize(18).FontColor("#004581").Bold();
                            });
                        });
                        page.Content()
                     .PaddingVertical(1, Unit.Centimetre)
                     .Column(x =>
                     {
                         x.Item().AlignRight().Text("AVISO DE COBRO").Bold().FontSize(20).FontColor("#004581");
                         x.Item().Row(x =>
                         {
                             x.RelativeItem().Column(x => {
                                 x.Item().AlignLeft().Text("OFICINA: " + item.propiedad.Codigo).FontSize(16).FontColor("#004581");
                                 x.Item().AlignLeft().Text("Propietario: " + item.AspNetUser.FirstName + " " + item.AspNetUser.LastName).FontSize(16).FontColor("#004581");
                             });
                             x.RelativeItem().Column(x =>
                             {
                                 x.Item().AlignRight().Text("Relación de Gastos: 001-ENE-24").FontSize(16).FontColor("#004581");
                                 x.Item().AlignRight().Text("Mes Relacionado:ENERO 2024").FontSize(16).FontColor("#004581");
                                 x.Item().AlignRight().Text("Fecha Emisión:" + item.RelacionGasto.Fecha).FontSize(16).FontColor("#004581");
                                 x.Item().AlignRight().Text("Fecha Vencimiento:23/02/2024").FontSize(16).FontColor("#004581");

                             });
                             x.RelativeItem().Text("");
                             x.RelativeItem().Text("");
                             x.RelativeItem().Text("");
                             x.RelativeItem().Text("");
                         });
                         //x.Spacing(20);
                         x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                         {
                             tabla.ColumnsDefinition(columns =>
                             {
                                 columns.RelativeColumn(100);
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                             });

                             tabla.Header(header =>
                             {
                                 header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Descripción").FontColor("#607080").Bold().FontSize(10);

                                 header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("Gtos Generales").FontColor("#607080").Bold().FontSize(10);

                                 foreach (var grupos in item.grupos)
                                 {
                                     var nombreGrupo = _context.GrupoGastos.Where(c => c.IdGrupoGasto == grupos.IdGrupoGasto).Select(c => c.NombreGrupo).First();
                                     header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Gtos" + nombreGrupo + grupos.Alicuota + "%").FontColor("#607080").Bold().FontSize(10);
                                 }

                                 header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("Cuota").FontColor("#607080").Bold().FontSize(10);
                             });

                             foreach (var transacciones in item.transaccions)
                             {
                                 if (transacciones.IdPropiedad == null)
                                 {
                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("Gastos Ordinarios:").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(transacciones.Descripcion).FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(transacciones.MontoTotal).Bold().FontColor("#607080").Bold().FontSize(10);


                                     //foreach (var grupos in item.grupos)
                                     //{
                                     //    var nombreGrupo = _context.GrupoGastos.Where(c => c.IdGrupoGasto == grupos.IdGrupoGasto).Select(c => c.NombreGrupo).First();
                                     //    if (nombreGrupo == "Grupo 1")
                                     //    {
                                     //        totalGasto1 += Convert.ToDecimal(transacciones.MontoTotal);
                                     //    }
                                     //    if (transacciones.IdGrupoGasto == grupos.IdGrupoGasto)
                                     //    {
                                     //        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     //       .Padding(5).Text(transacciones.MontoTotal).FontColor("#607080").Bold().FontSize(10);
                                     //    }
                                     //    else
                                     //    {
                                     //        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     //      .Padding(5).Text("0,00").FontColor("#607080").Bold().FontSize(10);
                                     //    }

                                     //}
                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text((item.RelacionGasto.TotalMensual * item.propiedad.Alicuota).ToString("N")).Bold().FontColor("#607080").Bold().FontSize(10);

                                     totalGastosGenerales += Convert.ToDecimal(transacciones.MontoTotal);
                                     totalCuota += item.RelacionGasto.TotalMensual * item.propiedad.Alicuota;
                                 }
                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("Total Gastos Ordinarios =>").FontColor("#607080").Bold().FontSize(10);


                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(totalGastosGenerales.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                 foreach (var grupos in item.grupos)
                                 {
                                     var nombreGrupo = _context.GrupoGastos.Where(c => c.IdGrupoGasto == grupos.IdGrupoGasto).Select(c => c.NombreGrupo).First();
                                     if (nombreGrupo == "Grupo 1")
                                     {
                                         tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text(totalGasto1.ToString("N")).FontColor("#607080").Bold().FontSize(10);
                                     }
                                     else
                                     {
                                         tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("0,00").FontColor("#607080").Bold().FontSize(10);
                                     }
                                 }

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalCuota.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                             }
                             foreach (var transacciones in item.transaccions)
                             {
                                 if (transacciones.IdPropiedad != null)
                                 {
                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Gastos Individuales:").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                      .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(transacciones.Descripcion).FontColor("#607080").Bold().FontSize(10);

                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(transacciones.MontoTotal).Bold().FontColor("#607080").Bold().FontSize(10);


                                     //foreach (var grupos in item.grupos)
                                     //{
                                     //    var nombreGrupo = _context.GrupoGastos.Where(c => c.IdGrupoGasto == grupos.IdGrupoGasto).Select(c => c.NombreGrupo).First();
                                     //    if (nombreGrupo == "Grupo 1")
                                     //    {
                                     //        totalGasto1 += Convert.ToDecimal(transacciones.MontoTotal);
                                     //    }
                                     //    if (transacciones.IdGrupoGasto == grupos.IdGrupoGasto)
                                     //    {
                                     //        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     //       .Padding(5).Text(transacciones.MontoTotal).FontColor("#607080").Bold().FontSize(10);
                                     //    }
                                     //    else
                                     //    {
                                     //        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     //      .Padding(5).Text("0,00").FontColor("#607080").Bold().FontSize(10);
                                     //    }

                                     //}
                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text((item.RelacionGasto.TotalMensual * item.propiedad.Alicuota).ToString("N")).Bold().FontColor("#607080").Bold().FontSize(10);

                                     totalGastosIndividuales += Convert.ToDecimal(transacciones.MontoTotal);
                                     totalCuota += item.RelacionGasto.TotalMensual * item.propiedad.Alicuota;
                                 }
                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("Total Gastos Individuales =>").FontColor("#607080").Bold().FontSize(10);


                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text(totalGastosIndividuales.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                 foreach (var grupos in item.grupos)
                                 {
                                     var nombreGrupo = _context.GrupoGastos.Where(c => c.IdGrupoGasto == grupos.IdGrupoGasto).Select(c => c.NombreGrupo).First();
                                     if (nombreGrupo == "Grupo 1")
                                     {
                                         tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                         .Padding(5).Text(totalGasto1.ToString("N")).FontColor("#607080").Bold().FontSize(10);
                                     }
                                     else
                                     {
                                         tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("0,00").FontColor("#607080").Bold().FontSize(10);
                                     }
                                 }

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalCuota.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                             }

                             if (item.provisiones.Count > 0)
                             {
                                 decimal totalProvisiones = 0;
                                 decimal totalProvisionesCuota = 0;
                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("Previsiones:").FontColor("#607080").Bold().FontSize(10);

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                  .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
                                 foreach (var provisiones in item.provisiones)
                                 {
                                     var idsubcuenta = _context.CodigoCuentasGlobals.Where(c => c.IdCodCuenta == provisiones.IdCodCuenta).Select(c => c.IdSubCuenta).First();
                                     var subcuenta = _context.SubCuenta.Where(c => c.Id == idsubcuenta).First();
                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text(subcuenta.Descricion).FontColor("#607080").Bold().FontSize(10);
                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text(provisiones.Monto.ToString("N")).Bold().FontColor("#607080").Bold().FontSize(10);
                                     totalProvisiones += provisiones.Monto;
                                     foreach (var grupos in item.grupos)
                                     {
                                         var grupoGasto = _context.GrupoGastos.Where(c => c.IdGrupoGasto == grupos.IdGrupoGasto).First();
                                         var grupoCuenta = _context.CuentasGrupos.Where(c => c.IdGrupoGasto == grupoGasto.IdGrupoGasto).First();
                                         var codigCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdCodCuenta == grupoCuenta.IdCodCuenta).First();
                                         if (codigCuenta.IdSubCuenta != subcuenta.IdCuenta)
                                         {
                                             tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                             .Padding(5).Text(codigCuenta.Saldo.ToString("N")).Bold().FontColor("#607080").Bold().FontSize(10);
                                         }
                                         else
                                         {
                                             tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                           .Padding(5).Text("0,00").FontColor("#607080").Bold().FontSize(10);
                                         }
                                     }
                                     tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text((provisiones.Monto * item.propiedad.Alicuota).ToString("N")).Bold().FontColor("#607080").Bold().FontSize(10);
                                     totalProvisionesCuota += provisiones.Monto * item.propiedad.Alicuota;
                                 }
                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("Total Previsiones=>").FontColor("#607080").Bold().FontSize(10);
                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(totalProvisiones.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("0,00").FontColor("#607080").Bold().FontSize(10);

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("0,00").FontColor("#607080").Bold().FontSize(10);

                                 tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text(totalProvisionesCuota.ToString("N")).FontColor("#607080").Bold().FontSize(10);
                             }
                         });

                         x.Item().Column(x =>
                         {
                             decimal cuotaMes = totalGastosGenerales + totalGastosIndividuales;
                             x.Item().AlignRight().Text("Cuota del Mes =>" + cuotaMes.ToString("N"));
                             decimal interesMora = item.propiedad.MontoIntereses;
                             x.Item().AlignRight().Text("Interés de mora =>" + interesMora.ToString("N"));
                             decimal Indexación = (decimal)item.propiedad.MontoMulta;
                             x.Item().AlignRight().Text("Indexación =>" + Indexación.ToString("N"));
                             decimal totalCuotaMes = cuotaMes + Indexación + interesMora;

                             x.Item().AlignRight().Text("Total Cuota del Mes =>" + (totalCuotaMes).ToString("N"));
                             decimal saldo = item.propiedad.Saldo;
                             x.Item().AlignRight().Text("Saldo Ant. Cuotas Ord. =>" + saldo.ToString("N"));
                             decimal cuotasExtras = (decimal)item.propiedad.Creditos;
                             x.Item().AlignRight().Text("Saldo Ant. Cuotas Extras =>" + cuotasExtras.ToString("N"));
                             decimal acumulado = cuotasExtras + totalCuotaMes + saldo + cuotasExtras;
                             x.Item().AlignRight().Text("Acumulado =>" + acumulado.ToString("N"));
                             x.Spacing(20);
                         });
                     });
                        page.Footer()
                            .AlignLeft()
                            .Text(x =>
                            {
                                x.Span("Software desarrollado por: Password Technology");
                            });
                    };
                });
            })
        .GeneratePdf();
            return data;
        }
        public byte[] RetencionesIva(List<ListaRetencionesIVAVM> listaRetencionesIVAVM)
        {
            var data = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().ShowOnce().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("CONDOMINIO" + " ").Bold().FontSize(20).FontColor("#004581").Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Fecha: " + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(16).FontColor("#004581");
                            col.Item().AlignRight().Text(x =>
                            {
                                x.Span("Página: " + x.CurrentPageNumber()).FontSize(16);
                            });
                        });
                    });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().AlignRight().Text("Fecha" + DateTime.Today.ToString("dd/MM/yyyy")).FontSize(14).FontColor("#004581");
                            x.Item().AlignRight().Text(x =>
                            {
                                x.Span("Página: " + x.CurrentPageNumber()).FontSize(14);
                            });
                            x.Spacing(20);
                            x.Item().AlignCenter().Text("Listado de Retenciones Impuesto al Valor Agregado (I.V.A.)").FontSize(14).Bold().FontColor("#004581");
                            x.Spacing(20);
                            x.Item().AlignCenter().Text("Desde: " + "fecha" + " Hasta: " + " fecha").FontSize(14).Bold().FontColor("#004581");


                            x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            {
                                decimal totalGeneral = 0;
                                tabla.ColumnsDefinition(columns =>
                                {
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
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Comprobante").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Fecha").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Proveedor").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                });
                                foreach (var item in listaRetencionesIVAVM)
                                {
                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text(item.comprobanteRetencion.IdComprobante).FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.comprobanteRetencion.FechaEmision.ToString("dd/MM/yyyy")).FontColor("#607080").FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.Proveedor.Nombre).FontColor("#607080").FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Fecha").Underline().FontColor("#607080").FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Factura").Underline().FontColor("#607080").FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Concepto").Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Total Factura").Underline().FontColor("Total general =>").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("I.V.A.").FontColor("Fecha").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("% Ret.").Underline().FontColor("Fecha").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Retenido").Underline().FontColor("Fecha").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.compRetIva.FechaEmision.ToString("dd/MM/yyyy")).Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.compRetIva.IdFactura.ToString("N")).Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.comprobanteRetencion.Descripcion).Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.compRetIva.TotalCompraRetIva.ToString("N")).Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.compRetIva.ImpIva.ToString("N")).Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.compRetIva.Alicuota.ToString()).Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.compRetIva.IvaRetenido.ToString("N")).Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                .Padding(5).Text("").Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("").Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                     .Padding(5).Text("").Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("").Underline().FontColor("#607080").Bold().FontSize(10);

                                    tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text(item.compRetIva.IvaRetenido.ToString("N")).Underline().FontColor("Fecha").Bold().FontSize(10);
                                    x.Spacing(20);

                                    totalGeneral += item.compRetIva.IvaRetenido;
                                }


                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").Underline().FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                 .Padding(5).Text("").Underline().FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("").Underline().FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text("Total General => ").Underline().FontColor("#607080").Bold().FontSize(10);

                                tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                               .Padding(5).Text(totalGeneral.ToString("N")).Underline().FontColor("#607080").Bold().FontSize(10);
                            });
                        });
                    page.Footer()
                        .AlignLeft()
                        .Text(x =>
                        {
                            x.Span("Software desarrollado por: Password Technology");
                        });
                });
            })
         .GeneratePdf();
            return data;
        }
    }
}
