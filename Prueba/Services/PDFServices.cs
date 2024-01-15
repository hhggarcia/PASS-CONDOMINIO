using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
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
    }
    public class PDFServices: IPDFServices
    {
        private readonly IWebHostEnvironment _host;
        private readonly PruebaContext _context;

        public PDFServices(IWebHostEnvironment host,
            PruebaContext context)
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
                        txt.Span("Pagina ").FontSize(10);
                        txt.CurrentPageNumber().FontSize(10);
                        txt.Span(" de ").FontSize(10);
                        txt.TotalPages().FontSize(10);
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
                            x.Span("Page ");
                            x.CurrentPageNumber();
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
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
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
                    page.Header().AlignCenter().Text("Junta de Condominio Residencias Parque Humbolt").Bold().FontSize(20).FontColor("#004581");

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
                                row.RelativeItem().Padding(10,0).Column(col =>
                                {
                                    var propiedad = detalleReciboVM.Propiedad.Codigo;
                                    var torrePiso = propiedad.Split("-");
                                    col.Item().Text("Torre: " + torrePiso[0].Trim()).FontSize(12).Bold();
                                    col.Item().Text("Piso: " + torrePiso[1].Trim()).FontSize(12).Bold();
                                    col.Item().Text("Partamento: " + propiedad).FontSize(12).Bold();
                                    col.Item().Text("Propietario: " + detalleReciboVM.Propietario.FirstName + " " + detalleReciboVM.Propietario.LastName).FontSize(12).Bold();
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
                                    columns.RelativeColumn();
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
                                        foreach(var cuotas in detalleReciboVM.CuotasRecibosCobros)
                                        {
                                            header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                            .Padding(5).Text(cuotas.CuotasEspeciale.Descripcion+ "\n" + 
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
                                if(detalleReciboVM.CuotasRecibosCobros.Count > 0)
                                {
                                    foreach (var cuotas in detalleReciboVM.CuotasRecibosCobros)
                                    {
                                        if (cuotas.ReciboCuota.CuotasFaltantes != 0)
                                        {
                                            totalCuotasSubCuotas += (decimal)cuotas.ReciboCuota.SubCuotas;
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(cuotas.ReciboCuota.SubCuotas?.ToString("N2") + " Bs.").FontSize(8);
                                        }
                                        else
                                        {
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                           .Padding(5).Text("No posee deudas.").FontSize(8);
                                        }
                                    }
                                }
                                
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text((detalleReciboVM.Propiedad.Deuda + detalleReciboVM.Propiedad.Saldo + totalCuotasSubCuotas).ToString("N2")+ " Bs.").FontSize(8);
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
                                            var subcuenta = detalleReciboVM.RelacionGastos.SubcuentasGastos.Where(c => c.Id == idcc.First().IdCodigo).ToList();
                                            
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
                                        var idcc = detalleReciboVM.RelacionGastos.CCProvisiones.Where(c => c.IdCodigo == provisiones.IdCodCuenta);
                                        if (idcc != null && idcc.Any())
                                        {
                                            var subcuenta = detalleReciboVM.RelacionGastos.SubCuentasProvisiones.Where(c => idcc.First().IdCodigo == c.Id).ToList();

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

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(8);
                                
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("Sub Total").FontSize(8);
                               
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                              .Padding(5).Text(detalleReciboVM.RelacionGastos.SubTotal.ToString("N")).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(((detalleReciboVM.RelacionGastos.SubTotal * (detalleReciboVM.Propiedad.Alicuota / 100)).ToString("N"))).FontSize(8);

                                if (detalleReciboVM.RelacionGastos.Fondos != null && detalleReciboVM.RelacionGastos.Fondos.Any()
                                 && detalleReciboVM.RelacionGastos.SubCuentasFondos != null && detalleReciboVM.RelacionGastos.SubCuentasFondos.Any()
                                && detalleReciboVM.RelacionGastos.CCFondos != null && detalleReciboVM.RelacionGastos.CCFondos.Any())
                                {
                                    foreach (var fondo in detalleReciboVM.RelacionGastos.Fondos)
                                    {
                                        var idcc = detalleReciboVM.RelacionGastos.CCFondos.Where(c => c.IdCodigo == fondo.IdCodCuenta).ToList();
                                        if (idcc != null && idcc.Any())
                                        {
                                            var subcuenta = detalleReciboVM.RelacionGastos.SubCuentasFondos.Where(c => c.Id == idcc.First().IdCodigo).ToList();

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

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("Total gasto del mes").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(detalleReciboVM.RelacionGastos.Total.ToString("N")).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(((detalleReciboVM.RelacionGastos.Total * (detalleReciboVM.Propiedad.Alicuota / 100)).ToString("N"))).FontSize(8);
                            });
                            x.Item().Column(c =>
                            {
                                c.Item().AlignCenter().Text("Elaborado por Junta de Condominio Res Parque Humbolt").FontSize(8);
                                c.Item().AlignCenter().Text("Realizar los pagos por Transferencias a la cuenta").FontSize(10);
                                c.Item().AlignCenter().Text("BANCO MERCANTIL").FontSize(10).Bold();
                                c.Item().AlignCenter().Text("Junta de Condominio Residencias Parque Humbolt").FontSize(10).Bold();
                                c.Item().AlignCenter().Text("Rif J-30720421-4").FontSize(10).Bold();
                                c.Item().AlignCenter().Text("Cta. Cte. 0105-0021-46802103-9647").FontSize(10).Bold();
                                c.Item().AlignCenter().Text("Enviar al correo: adm.parque.humboldt@gmail.com").FontSize(12).Bold().FontColor("#FF0000");
                                c.Item().AlignCenter().Text("Favor indicar número de Apartamento").FontSize(12).Bold().FontColor("#FF0000");
                            });


                        });
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
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
                                   .Padding(5).Text("DIFERENCIA").Bold().FontSize(10);
                                });
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(DateTime.Today.ToString("dd/MM/yyyy")).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(comprobanteVM.PagoRecibido.Monto.ToString("N")).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(comprobanteVM.PagoRecibido.ValorDolar.ToString()).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(((comprobanteVM.PagoRecibido.Monto / comprobanteVM.PagoRecibido.ValorDolar ).ToString("N2") + " Bs.")).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text((comprobanteVM.Propiedad.Saldo).ToString("N2")).FontSize(8);

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
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
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
                                .Padding(5).Text((comprobanteCEVM.PagoRecibido.Monto / comprobanteCEVM.PagoRecibido.ValorDolar).ToString("N2") + " Bs.").FontSize(8);
                                if(comprobanteCEVM.Restante !=0 && comprobanteCEVM.Restante != null)
                                {
                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                     .Padding(5).Text(comprobanteCEVM.Restante.ToString("N2") + " Bs.").FontSize(8);
                                }
                                else
                                {
                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                     .Padding(5).Text("0,00"+ " Bs.").FontSize(8);
                                }
                          
                            });
                        });
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
            .GeneratePdf();
            return data;
        }
        public byte[] ComprobantePEVMPDF(ComprobantePEVM comprobanteVM)
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

                            x.Spacing(10);
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
                                .Padding(5).Text(comprobanteVM.Pago.Monto.ToString("N")).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text(comprobanteVM.Pago.ValorDolar.ToString()).FontSize(8);

                                //tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                //.Padding(5).Text((comprobanteVM.Pago.Monto
                                /// comprobanteVM.Pago.ValorDolar).ToString()).FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(8);

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(8);
 
                            });

                            //x.Item().AlignCenter().Text("DATOS DEL PROPIETARIO").FontSize(16);
                            //x.Item().AlignCenter().Text("SALDO PENDIENTE").FontSize(12);

                            //x.Item().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
                            //{
                            //    tabla.ColumnsDefinition(columns =>
                            //    {
                            //        columns.RelativeColumn();
                            //        columns.RelativeColumn();
                            //        columns.RelativeColumn();
                            //    });
                            //    tabla.Header(header =>
                            //    {
                            //        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                            //        .Padding(5).Text("CUOTA ORDINARIA").Bold().FontSize(10);

                            //        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                            //       .Padding(5).Text("CUOTA EXTRAODINARIA").Bold().FontSize(10);

                            //        header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                            //       .Padding(5).Text("ASOTEA").Bold().FontSize(10);

   
                            //    });
                            //    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                            //    .Padding(5).Text(DateTime.Today.ToString("dd/MM/yyyy")).FontSize(8);

                            //    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                            //    .Padding(5).Text(comprobanteVM.Pago.Monto.ToString("N")).FontSize(8);

                            //    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                            //    .Padding(5).Text(comprobanteVM.Pago.ValorDolar.ToString()).FontSize(8);

                            //    //tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                            //    //.Padding(5).Text((comprobanteVM.Pago.Monto / comprobanteVM.Pago.ValorDolar).ToString()).FontSize(8);
                                
                            //    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                            //     .Padding(5).Text("").FontSize(8);

                            //    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                            //    .Padding(5).Text("").FontSize(8);

                            //});


                            //if (comprobanteVM.Condominio != null)
                            //{
                            //    x.Item().AlignCenter().Text("Condominio").FontColor("#004581").FontSize(12).Bold();
                            //    x.Item().AlignCenter().Text("Condominio: " + comprobanteVM.Condominio.Nombre);
                            //    x.Item().AlignCenter().Text("RIF: " + comprobanteVM.Condominio.Rif);
                            //}
                            //if (comprobanteVM.Pago != null)
                            //{
                            //    if (comprobanteVM.Pagoforma == FormaPago.Transferencia)
                            //    {
                            //        x.Item().AlignCenter().Text("Transferencia").FontColor("#004581").FontSize(12).Bold(); ;
                            //        x.Item().AlignCenter().Text("Fecha: " + comprobanteVM.Pago.Fecha.ToString("dd/MM/yyyy"));
                            //        x.Item().AlignCenter().Text("Cuenta: " + comprobanteVM.Banco.Descricion);
                            //        x.Item().AlignCenter().Text("Referencia: " + comprobanteVM.NumReferencia);
                            //        x.Item().AlignCenter().Text("Fecha de comprobante: " + DateTime.Today.ToString("dd/MM/yyyy"));
                            //    }
                            //    else
                            //    {
                            //        x.Item().AlignCenter().Text("Efectivo").FontColor("#004581").FontSize(12).Bold(); 
                            //        x.Item().AlignCenter().Text("Cuenta: " + comprobanteVM.Caja.Descricion);
                            //        x.Item().AlignCenter().Text("Monto: " + comprobanteVM.Pago.Monto.ToString("N"));
                            //        x.Item().AlignCenter().Text("Fecha de comprobante: " + DateTime.Today.ToString("dd/MM/yyyy"));
                            //    }
                            //}

                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
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
                               && indexPagoRecibdio.PropiedadPagos.Count() > 0){
                                    foreach(var diccUsuarioPropiedad in indexPagoRecibdio.UsuariosPropiedad)
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
                                                        .Padding(5).Text(diccUsuarioPropiedad.Key.FirstName+ " "+ diccUsuarioPropiedad.Key.LastName).FontSize(10).FontColor("#607080");

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
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
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
                                if(ldiarioGlobals.AsientosCondominio != null)
                                {
                                    foreach (var asiento in ldiarioGlobals.AsientosCondominio)
                                    {
                                        var cc = ldiarioGlobals.CuentasCondominio.Where(c => c.IdCodCuenta == asiento.IdCodCuenta).ToList();

                                        var subcuenta = ldiarioGlobals.CuentasDiarioCondominio.Where(c => c.Id == cc.First().IdCodigo).ToList();

                                        var cuenta = ldiarioGlobals.Cuentas.Where(c => c.Id == subcuenta.First().IdCuenta).ToList();

                                        var grupo = ldiarioGlobals.Grupos.Where(c => c.Id == cuenta.First().IdGrupo).ToList();

                                        var clase = ldiarioGlobals.Clases.Where(c => c.Id == grupo.First().IdClase).ToList();


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
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
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
                            x.Item().AlignCenter().Text("RESIDENCIAS PARQUE HUMBOLDT").FontSize(12);
                            x.Item().AlignCenter().Text("j-30720421-4").FontSize(12);
                            x.Item().AlignCenter().Text("RECIBO DE COBRO MES:" + DateTime.Today.ToString("MM/yyyy")).FontSize(12);

                            x.Item().AlignCenter().Border(0.5f).BorderColor("#D9D9D9").Table(tabla =>
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
                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                    .Padding(5).Text("Descripción").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Usd").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Tasa").FontColor("#607080").Bold().FontSize(10);

                                    header.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                   .Padding(5).Text("Bs").FontColor("#607080").Bold().FontSize(10);
                                });
                                if((relacionDeGastos.SubcuentasGastos != null && relacionDeGastos.SubcuentasGastos.Any())
                                  && (relacionDeGastos.GastosDiario != null && relacionDeGastos.GastosDiario.Any())
                                  && relacionDeGastos.CCGastos != null && relacionDeGastos.CCGastos.Any())
                                {
                                    foreach (var item in relacionDeGastos.GastosDiario)
                                    {
                                        var idcc = relacionDeGastos.CCGastos.Where(c => c.IdCodCuenta == item.IdCodCuenta);
                                        if (idcc != null && idcc.Any())
                                        {
                                            var subcuenta = relacionDeGastos.SubcuentasGastos.Where(c => c.Id == idcc.First().IdCodigo).ToList();
                                            
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(subcuenta.First().Descricion).FontSize(10).FontColor("#607080");
                                            
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text((item.Monto / item.ValorDolar ).ToString("N2")).FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(item.ValorDolar.ToString()).FontSize(10).FontColor("#607080");
                                            
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(item.Monto.ToString()).FontSize(10).FontColor("#607080");
                                        }
                                    }
                                }
                                else
                                {
                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("No").FontSize(10).FontColor("#607080"); 
                                    
                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("hay").FontSize(10).FontColor("#607080"); 
                                    
                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("Gastos").FontSize(10).FontColor("#607080");

                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                    tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                    .Padding(5).Text("").FontSize(10).FontColor("#607080");
                                }
                                if (relacionDeGastos.Provisiones != null && relacionDeGastos.Provisiones.Any()
                                && relacionDeGastos.SubCuentasProvisiones != null && relacionDeGastos.SubCuentasProvisiones.Any()
                                && relacionDeGastos.CCProvisiones != null && relacionDeGastos.CCProvisiones.Any())
                                {
                                    foreach (var provisiones in relacionDeGastos.Provisiones)
                                    {
                                        var idcc = relacionDeGastos.CCProvisiones.Where(c => c.IdCodigo == provisiones.IdCodCuenta);
                                        if (idcc != null && idcc.Any())
                                        {
                                            var subcuenta = relacionDeGastos.SubCuentasProvisiones.Where(c => idcc.First().IdCodigo == c.Id).ToList();

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(subcuenta.First().Descricion).FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text((provisiones.ValorDolar * provisiones.Monto)
                                            .ToString("N2")).FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(provisiones.ValorDolar.ToString("N"))
                                            .FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text(provisiones.Monto.ToString()).FontSize(10).FontColor("#607080");
                                        }
                                    }
                                }
                          
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("SUBTOTAL").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text(relacionDeGastos.SubTotal.ToString("N")).FontSize(10).FontColor("#607080");

                                if (relacionDeGastos.Fondos != null && relacionDeGastos.Fondos.Any()
                              && relacionDeGastos.SubCuentasFondos != null && relacionDeGastos.SubCuentasFondos.Any()
                              && relacionDeGastos.CCFondos != null && relacionDeGastos.CCFondos.Any())
                                {
                                    foreach (var fondo in relacionDeGastos.Fondos)
                                    {
                                        var idcc = relacionDeGastos.CCFondos.Where(c => c.IdCodigo == fondo.IdCodCuenta).ToList();
                                        if (idcc != null && idcc.Any())
                                        {
                                            var subcuenta = relacionDeGastos.SubCuentasFondos.Where(c => c.Id == idcc.First().IdCodigo).ToList();
                                           
                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text($"{subcuenta.First().Descricion} - {fondo.Porcentaje}%").FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text("").FontSize(10).FontColor("#607080");

                                            tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                            .Padding(5).Text((relacionDeGastos.SubTotal * fondo.Porcentaje / 100)
                                            .ToString("N")).FontSize(10).FontColor("#607080");
                                        }
                                    }
                                }
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                .Padding(5).Text("Total gasto del mes").FontSize(10).FontColor("#607080");

                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text("").FontSize(10).FontColor("#607080");
                                
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text("").FontSize(10).FontColor("#607080");   
                                
                                tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                               .Padding(5).Text(relacionDeGastos.Total.ToString("N")).FontSize(10).FontColor("#607080");
                            });

                        });
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
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
                                        var asiento = estadoResultado.AsientosCondominio.Where(c => c.IdAsiento == ingreso.IdAsiento).ToList();
                                        var subcuenta = estadoResultado.SubCuentas.Where(c => c.Id == asiento.First().IdCodCuenta).ToList();
                                        var cuenta = estadoResultado.Cuentas.Where(c => c.Id == subcuenta.First().IdCuenta).ToList();
                                        var grupo = estadoResultado.Grupos.Where(c => c.Id == cuenta.First().IdGrupo).ToList();
                                        var clase = estadoResultado.Clases.Where(c => c.Id == grupo.First().IdClase).ToList();

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text((clase.First().Codigo + "." + grupo.First().Codigo + "." + cuenta.First().Codigo
                                        + "." + subcuenta.First().Codigo)).FontSize(10).FontColor("#607080");


                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text(subcuenta.First().Descricion).FontColor("#607080").Bold().FontSize(10);

                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text(asiento.First().Concepto).FontColor("#607080").Bold().FontSize(10);

                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text(asiento.First().Monto.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
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
                                        var asiento = estadoResultado.AsientosCondominio.Where(c => c.IdAsiento == egreso.IdAsiento).ToList();
                                        var subcuenta = estadoResultado.SubCuentas.Where(c => c.Id == asiento.First().IdCodCuenta).ToList();
                                        var cuenta = estadoResultado.Cuentas.Where(c => c.Id == subcuenta.First().IdCuenta).ToList();
                                        var grupo = estadoResultado.Grupos.Where(c => c.Id == cuenta.First().IdGrupo).ToList();
                                        var clase = estadoResultado.Clases.Where(c => c.Id == grupo.First().IdClase).ToList();

                                        tabla.Cell().BorderRight(0.5f).BorderColor("#D9D9D9")
                                        .Padding(5).Text((clase.First().Codigo + "." + grupo.First().Codigo 
                                        + "." + cuenta.First().Codigo + "." + subcuenta.First().Codigo)).FontSize(10).FontColor("#607080");


                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text(subcuenta.First().Descricion).FontColor("#607080").Bold().FontSize(10);

                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text(asiento.First().Concepto).FontColor("#607080").Bold().FontSize(10);

                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                       .Padding(5).Text(asiento.First().Monto.ToString("N")).FontColor("#607080").Bold().FontSize(10);

                                        tabla.Cell().Border(0.5f).BorderColor("#D9D9D9").AlignMiddle()
                                        .Padding(5).Text("").FontColor("#607080").Bold().FontSize(10);
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
                    .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
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
                    
                                if(indexPagosVM.PagosEmitidos != null && indexPagosVM.PagosEmitidos.Count() >0)
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
                                                        .Padding(5).Text((item.Monto.ToString("N")) +" "+
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
                    .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
         .GeneratePdf();
            return data;
        }
    }
}
