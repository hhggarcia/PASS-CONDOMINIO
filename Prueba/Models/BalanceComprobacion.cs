using System;
using System.Collections.Generic;

namespace Prueba.Models;

public partial class BalanceComprobacion
{
    public int IdBalanceC { get; set; }

    public decimal Fecha { get; set; }

    public decimal SaldoInicial { get; set; }

    public decimal SaldoFinal { get; set; }

    public decimal Diferencia { get; set; }

    public int IdCondominio { get; set; }

    public virtual Condominio IdCondominioNavigation { get; set; } = null!;
}
