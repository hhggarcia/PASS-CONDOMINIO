namespace Prueba.ViewModels
{
    public class ListaRetencionesIVAVM
    {
        public string Comprobante { get; set; }
        public DateTime Fecha { get; set; }
        public string Proveedor { get; set; }
        public string NumFactura { get; set; }
        public string Concepto { get; set; }
        public decimal TotalFactura { get; set; }
        public decimal Iva { get; set; }
        public decimal Retencones { get; set; }
        public decimal Retenido { get; set; }
    }
}
