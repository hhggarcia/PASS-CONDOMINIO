﻿using System;
using System.Collections.Generic;

namespace Prueba.Models
{
    public partial class Clase
    {
        public Clase()
        {
            Grupos = new HashSet<Grupo>();
        }

        public short Id { get; set; }
        public string Descripcion { get; set; } = null!;
        public string Codigo { get; set; } = null!;

        public virtual ICollection<Grupo> Grupos { get; set; }
    }
}