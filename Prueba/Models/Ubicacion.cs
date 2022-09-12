using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.Models
{
    public class Ubicacion
    {
        public IQueryable<SelectListItem>? Paises { get; set; }
        public int IdPais { get; set; }
        public IQueryable<SelectListItem>? Estados { get; set; }
        public int IdEstado { get; set; }

        public IQueryable<SelectListItem>? Municipios { get; set; }
        public int IdMunicipio { get; set; }

        public IQueryable<SelectListItem>? Parroquias { get; set; }
        public int IdParroquia { get; set; }

        public IQueryable<SelectListItem>? Zonas { get; set; }

        public int IdZona { get; set; }

    }
}
