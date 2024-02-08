using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Prueba.Models;

public partial class PagoFactura
{
    public int IdPagoEmitido { get; set; }

    public int IdFactura { get; set; }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; } 

    public virtual Factura IdFacturaNavigation { get; set; } = null!;

    public virtual PagoEmitido IdPagoEmitidoNavigation { get; set; } = null!;
}
