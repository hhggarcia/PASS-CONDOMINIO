using Prueba.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Prueba.ViewModels
{
    public class AsignarCobroTransitoReciboVM
    {
        [Required]
        public int IdPropiedad { get; set; }
        [Required]
        public int IdCobroTransito { get; set; }
        public IList<ReciboCobro> Recibos { get; set; } = new List<ReciboCobro>();
        public IList<SelectListItem> RecibosModel { get; set; } = new List<SelectListItem>(); 
        public IList<int> IdsRecibos { get; set; } = new List<int>();
        [Required]
        public IList<SelectListItem> ListRecibos { get; set; } = new List<SelectListItem>();

    }
}
