using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Municipio
    {
        public Municipio()
        {
            Parroquia = new HashSet<Parroquia>();
        }

        public int IdMunicipio { get; set; }
        public int IdEstado { get; set; }
        public string Municipio1 { get; set; } = null!;

        public virtual Estado IdEstadoNavigation { get; set; } = null!;
        public virtual ICollection<Parroquia> Parroquia { get; set; }
    }
}
