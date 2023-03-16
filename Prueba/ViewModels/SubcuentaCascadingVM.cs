using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class SubcuentaCascadingVM : SubCuenta
    {
        public SubcuentaCascadingVM()
        {
            Clases = new List<SelectListItem>();
            Grupos = new List<SelectListItem>();
            Cuentas = new List<SelectListItem>();
        }
        public List<SelectListItem>? Clases { get; set; }
        public List<SelectListItem>? Grupos { get; set; }
        public List<SelectListItem>? Cuentas { get; set; }

        public string Descripcion { get; set; } = string.Empty;
        public short IdClase { get; set; }
        public short IdGrupo { get; set; }
    }
}
