using Prueba.Models;

namespace Prueba.ViewModels
{
    public class PagoNominaVM : PagosNomina
    {
        public int IdEmpleado { get; set; }
        public IList<Deduccion> Deducciones { get; set; } = new List<Deduccion>();
        public IList<Percepcion> Percepciones { get; set; } = new List<Percepcion>();
    }
}
