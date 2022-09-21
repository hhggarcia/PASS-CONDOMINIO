using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Cuenta
    {
        public Cuenta()
        {
            SubCuenta = new HashSet<SubCuenta>();
        }

        public short Id { get; set; }
        public short IdGrupo { get; set; }
        public string Descripcion { get; set; } = null!;
        public string Codigo { get; set; } = null!;

        public virtual Grupo IdGrupoNavigation { get; set; } = null!;
        public virtual ICollection<SubCuenta> SubCuenta { get; set; }
    }
}
