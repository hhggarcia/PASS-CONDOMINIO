using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ClienteFacturasPendientesVM
    {
        public Cliente Cliente { get; set; } = null!;
        public IList<FacturaEmitida> FacturasPendientes { get; set; } = new List<FacturaEmitida>();
        public IDictionary<string, PagoRecibido> PagosFacturas { get; set; } = new Dictionary<string, PagoRecibido>();
    }
}
