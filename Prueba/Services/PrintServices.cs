using ceTe.DynamicPDF.Printing;
using Prueba.Context;

namespace Prueba.Services
{
    public interface IPrintServices
    {
        string PrintCompRetencion(byte[] data, int id);
        string PrintCompRetencionIva(byte[] data, int id);
    }
    public class PrintServices : IPrintServices
    {
        private readonly NuevaAppContext _context;

        public PrintServices(NuevaAppContext context)
        {
            _context = context;
        }

        public string PrintCompRetencion(byte[] data, int id)
        {
            var resultado = string.Empty;

            try
            {
                var impresora = _context.Impresoras.Where(c => c.IdCondominio ==  id).FirstOrDefault();
                InputPdf input = new InputPdf(data);

                PrintJob printJob = new PrintJob(impresora.Nombre, input);

                resultado = printJob.Status.ToString();
                // Imprimir el trabajo
                printJob.Print();

                return resultado;

            }
            catch (Exception ex)
            {
                resultado = ex.Message;
                return resultado;
            }
            

        }

        public string PrintCompRetencionIva(byte[] data, int id)
        {
            var resultado = string.Empty;

            try
            {
                InputPdf input = new InputPdf(data);

                PrintJob printJob = new PrintJob("HP Ink Tank 310 series", input);

                resultado = printJob.Status.ToString();
                // Imprimir el trabajo
                printJob.Print();

                return resultado;
            }
            catch (Exception ex)
            {

                resultado = ex.Message;
                return resultado;
            }
            
        }
    }
}
