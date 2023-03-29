using Microsoft.AspNetCore.Mvc;
using Prueba.Context;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SQLitePCL;

namespace Prueba.Services
{
    public interface IPDFServices
    {
        byte[] ExamplePDF();
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
                            col.Item().AlignCenter().Text("Junta de condominio Residencias Parque Humbold").Bold().FontSize(14);

                        });

                        row.RelativeItem().Column(col =>
                        {
                            string date = DateTime.UtcNow.ToString("dd-MM-yyyy");
                            col.Item().Column(col =>
                            {
                                col.Item().Border(1).BorderColor("#257272").
                                AlignCenter().Text(date);

                                col.Item().Background("#257272").Border(1)
                                .BorderColor("#257272").AlignCenter()
                                .Text("Alicuotas").FontColor("#fff");

                                col.Item().Border(1).BorderColor("#257272").
                                AlignCenter().Text("Costo de alicuota");
                            });
                        });

                    });

                    page.Content().PaddingVertical(10).Column(col1 =>
                    {
                        col1.Item().Column(col2 =>
                        {

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Propietario: ").SemiBold().FontSize(10);
                                txt.Span("Mario mendoza").FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Torre: ").SemiBold().FontSize(10);
                                txt.Span("A").FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Piso: ").SemiBold().FontSize(10);
                                txt.Span("2").FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Apartamento: ").SemiBold().FontSize(10);
                                txt.Span("51-A").FontSize(10);
                            });


                        });

                        col1.Item().LineHorizontal(0.5f);

                        col1.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);

                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background("#257272").Text("Fecha").FontColor("#fff").FontSize(10);

                                header.Cell().Background("#257272").Text("Tipo de Gasto").FontColor("#fff").FontSize(10);

                                header.Cell().Background("#257272").AlignCenter().Text("Descripcion").FontColor("#fff").FontSize(10);

                                header.Cell().Background("#257272").AlignRight().Text("Monto Bs.").FontColor("#fff").FontSize(10);

                                header.Cell().Background("#257272").AlignRight().Text("Alicuota").FontColor("#fff").FontSize(10);

                                header.Cell().Background("#257272").AlignRight().Text("Mto a \r\nPagar Bs.").FontColor("#fff").FontSize(10);
                            });

                            foreach (var item in Enumerable.Range(1, 6))
                            {
                                //Fecha actual
                                string date = DateTime.UtcNow.ToString("dd-MM-yyyy");
                                //precio de mes deudor
                                var precio = Placeholders.Random.Next(5);
                                //Total de la deuda
                                var total = precio;

                                tabla.Cell().Border(1).BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text(date).FontSize(6);
                                //Tipo de gasto
                                tabla.Cell().Border(1).BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text("Ordinario").FontSize(6);

                                tabla.Cell().Border(1).BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text("Descricion").FontSize(6);

                                //LG es para Leyenda de gasto

                                tabla.Cell().Border(1).BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).AlignRight().Text("LG").FontSize(6);

                                tabla.Cell().Border(1).BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).AlignRight().Text("Alicuota").FontSize(6);

                                tabla.Cell().Border(1).BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).AlignRight().Text("LG").FontSize(6);

                            }
                            foreach (var item in Enumerable.Range(1, 6))
                            {

                                tabla.Cell().Border(1).BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text("").FontSize(6);
                                //Tipo de gasto
                                tabla.Cell().Border(1).BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text("").FontSize(6);

                            }

                            col1.Item().AlignRight().Text("Total: ").FontSize(12);

                            col1.Item().Padding(10)
                            .Column(column =>
                            {
                                column.Item().Background(Transparent).AlignCenter().Text("Elaborado por Junta de Condominio Res Parque Humboldt\r\nRealizar los pagos por Transferencias a la cuenta\r\nBANCO MERCANTIL\r\nJunta de Condominio Residencias Parque Humboldt\r\nRif J-30720421-4\r\nCta. Cte. 0105-0021-46802103-9647\r\nEnviar al correo : adm.parque.humboldt@gmail.com\r\nFavor indicar número de Apartamento ").FontSize(8);
                            });

                        });

                        if (1 == 1)
                            col1.Item().Background(Transparent).Padding(10)
                            .Column(column =>
                            {
                                column.Item().Text("Observaciones").FontSize(14);

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

        public void ReciboPDF()
        {

        }

        public void LibroDiarioPDF()
        {

        }

        public void RelacionGastosPDF()
        {

        }

        public void EstadoDeResultadoPDF()
        {

        }

        public void BalanceComprobacionPDF()
        {

        }
    }
}
