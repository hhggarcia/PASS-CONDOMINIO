using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;
using System.ComponentModel.DataAnnotations;

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

        [Required]
        public string Descripcion { get; set; } = string.Empty;
        public short IdClase { get; set; }
        public short IdGrupo { get; set; }
        [Range(0, 10000, ErrorMessage = "El valor de {0} debe estar entre {1} y {2}.")]
        public short Saldo { get; set; }
        [Range(0, 10000, ErrorMessage = "El valor de {0} debe estar entre {1} y {2}.")]
        public short SaldoInicial { get; set; }
    }
}
