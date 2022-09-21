using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Grupo
    {
        public Grupo()
        {
            Cuenta = new HashSet<Cuenta>();
        }

        public short Id { get; set; }
        public short IdClase { get; set; }
        public string Descripcion { get; set; } = null!;
        public string Codigo { get; set; } = null!;

        public virtual Clase IdClaseNavigation { get; set; } = null!;
        public virtual ICollection<Cuenta> Cuenta { get; set; }
    }
}
