using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public IEnumerable<SelectListItem>? Propietarios { get; set; }
        public string? IdPropietario { get; set; }
        public Ubicacion? Ubicacion { get; set; }
        public Condominio? Condominio { get; set; }
        public Inmueble? Inmueble { get; set; }
        public IList<Propiedad>? Propiedades { get; set; }
        public IEnumerable<SelectListItem>? SelectPropiedades { get; set; }
        public Propiedad? Propiedad { get; set; }
        public Estacionamiento? Estacionamiento { get; set; }
        public IFormFile? ExcelPuestos_Est { get; set; }
        public IList<PuestoE>? Puesto_Est { get; set; }
        public string? Nombre { get; set; }
        public int Rif { get; set; }
    }
}
