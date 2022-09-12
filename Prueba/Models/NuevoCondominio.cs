using Microsoft.AspNetCore.Http;
namespace Prueba.Models
{
    public class NuevoCondominio
    {
        public Usuario? Administrador { get; set; }
        public IFormFile? ExcelPropietarios { get; set; }
        public IEnumerable<Usuario>? Propietarios { get; set; }
        public Ubicacion? Ubicacion { get; set; }
        public Inmueble? Inmueble { get; set; }
        public IEnumerable<Propiedad>? Propiedades { get; set; }
        public IEnumerable<Estacionamiento>? Estacionamientos { get; set; }
        public IEnumerable<PuestoE>? Puestos_Est { get; set; }
        public string? Nombre { get; set; }
        public int Rif { get; set; }
    }
}
