using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Parroquia
    {
        public Parroquia()
        {
            Zonas = new HashSet<Zona>();
        }

        public int IdParroquia { get; set; }
        public int IdMunicipio { get; set; }
        public string Parroquia1 { get; set; } = null!;
        public short Urbana { get; set; }

        public virtual Municipio IdMunicipioNavigation { get; set; } = null!;
        public virtual ICollection<Zona> Zonas { get; set; }
    }
}
