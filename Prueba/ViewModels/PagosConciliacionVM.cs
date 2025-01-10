namespace Prueba.ViewModels
{
    public class PagosConciliacionVM
    {
        public int Id { get; set; }
        public int IdCondominio { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public bool FormaPago { get; set; }
        public string? Concepto { get; set; } = string.Empty;
        public bool TipoOperacion { get; set; }
        public bool Activo { get; set; }

    }
}
