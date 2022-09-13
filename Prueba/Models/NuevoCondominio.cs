using Microsoft.AspNetCore.Http;
namespace Prueba.Models
{
    public class NuevoCondominio
    {
        /*MAS ADELANTE
         * modelar para que se creen condominios
         * con multiples Administradores
         * multiples inmuebles
         * multiples estacionamientos 
         */ 
        public Usuario? Administrador { get; set; }
        public IFormFile? ExcelPropietarios { get; set; }
        public IList<Usuario>? Propietarios { get; set; }
        public Ubicacion? Ubicacion { get; set; }
        public Inmueble? Inmueble { get; set; }
        public IList<Propiedad>? Propiedades { get; set; }
        public Estacionamiento? Estacionamiento { get; set; }
        public IFormFile? ExcelPuestos_Est { get; set; }
        public IList<PuestoE>? Puesto_Est { get; set; }
        public string? Nombre { get; set; }
        public int Rif { get; set; }
    }
}
