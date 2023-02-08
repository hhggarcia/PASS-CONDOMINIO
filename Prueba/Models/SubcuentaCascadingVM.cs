using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.Models
{
    public class SubcuentaCascadingVM
    {
        public SubcuentaCascadingVM()
        {
            this.Clases = new List<SelectListItem>();
            this.Grupos = new List<SelectListItem>();
            this.Cuentas = new List<SelectListItem>();
        }
        public List<SelectListItem>? Clases { get; set; }
        public List<SelectListItem>? Grupos { get; set; }
        public List<SelectListItem>? Cuentas { get; set; }

        public string Descripcion { get; set; }
        public string Codigo { get; set; }
        public short IdClase { get; set; }
        public short IdGrupo { get; set; }
        public short IdCuenta { get; set; }
    }
}
