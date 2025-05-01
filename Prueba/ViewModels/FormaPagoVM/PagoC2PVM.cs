using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels.FormaPagoVM
{
    public class PagoC2PVM
    {
        public List<SelectListItem> Propiedades { get; set; } = new List<SelectListItem>();
        public int IdPropiedad { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public List<SelectListItem> Bancos { get; set; } = new List<SelectListItem>();
        public int IdBanco { get; set; }
        public List<SelectListItem> Operaciones { get; set; } = new List<SelectListItem>();
        public int IdOperacion { get; set; }
        public List<string> TipoDocs { get; set; } = new List<string>();
        public string NumDocs { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public List<string> BancoOrigen { get; set; } = new List<string>();
        public string ClaveC2P { get; set; } = string.Empty;

        public IList<SelectListItem>? RecibosModel { get; set; }
        public IList<ReciboCobro>? Recibos { get; set; }
        public IList<SelectListItem>? ListRecibos { get; set; } = new List<SelectListItem>();
        public IList<int>? ListRecibosIDs { get; set; } = new List<int>();

        public decimal Saldo { get; set; }
        public decimal Interes { get; set; }
        public decimal Indexacion { get; set; }
        public decimal Credito { get; set; }
        public decimal Deuda { get; set; }
        public decimal Abonado { get; set; }
    }
}
