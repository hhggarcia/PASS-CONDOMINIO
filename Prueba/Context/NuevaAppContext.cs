using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Prueba.Models;

namespace Prueba.Context;

public partial class NuevaAppContext : DbContext
{
    public NuevaAppContext()
    {
    }

    public NuevaAppContext(DbContextOptions<NuevaAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activo> Activos { get; set; }

    public virtual DbSet<Anticipo> Anticipos { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<BalanceComprobacion> BalanceComprobacions { get; set; }

    public virtual DbSet<Clase> Clases { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<CodigoCuentasGlobal> CodigoCuentasGlobals { get; set; }

    public virtual DbSet<ComprobanteRetencion> ComprobanteRetencions { get; set; }

    public virtual DbSet<ComprobanteRetencionCliente> ComprobanteRetencionClientes { get; set; }

    public virtual DbSet<Condominio> Condominios { get; set; }

    public virtual DbSet<CondominioNomina> CondominioNominas { get; set; }

    public virtual DbSet<CuentasCobrar> CuentasCobrars { get; set; }

    public virtual DbSet<CuentasGrupo> CuentasGrupos { get; set; }

    public virtual DbSet<CuentasPagar> CuentasPagars { get; set; }

    public virtual DbSet<Cuenta> Cuenta { get; set; }

    public virtual DbSet<CuotasEspeciale> CuotasEspeciales { get; set; }

    public virtual DbSet<Deduccion> Deducciones { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<EstadoResultado> EstadoResultados { get; set; }

    public virtual DbSet<EstadoSituacion> EstadoSituacions { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<FacturaEmitida> FacturaEmitida { get; set; }

    public virtual DbSet<Fondo> Fondos { get; set; }

    public virtual DbSet<Gasto> Gastos { get; set; }

    public virtual DbSet<Grupo> Grupos { get; set; }

    public virtual DbSet<GrupoGasto> GrupoGastos { get; set; }

    public virtual DbSet<Ingreso> Ingresos { get; set; }

    public virtual DbSet<Islr> Islrs { get; set; }

    public virtual DbSet<Iva> Ivas { get; set; }

    public virtual DbSet<LdiarioGlobal> LdiarioGlobals { get; set; }

    public virtual DbSet<LibroCompra> LibroCompras { get; set; }

    public virtual DbSet<LibroVenta> LibroVentas { get; set; }

    public virtual DbSet<MonedaCond> MonedaConds { get; set; }

    public virtual DbSet<MonedaCuenta> MonedaCuenta { get; set; }

    public virtual DbSet<Moneda> Moneda { get; set; }

    public virtual DbSet<PagoAnticipo> PagoAnticipos { get; set; }

    public virtual DbSet<PagoEmitido> PagoEmitidos { get; set; }

    public virtual DbSet<PagoFactura> PagoFacturas { get; set; }

    public virtual DbSet<PagoFacturaEmitida> PagoFacturaEmitida { get; set; }

    public virtual DbSet<PagoRecibido> PagoRecibidos { get; set; }

    public virtual DbSet<PagoReserva> PagoReservas { get; set; }

    public virtual DbSet<PagosCuota> PagosCuotas { get; set; }

    public virtual DbSet<PagosNomina> PagosNominas { get; set; }

    public virtual DbSet<PagosRecibo> PagosRecibos { get; set; }

    public virtual DbSet<Pasivo> Pasivos { get; set; }

    public virtual DbSet<Patrimonio> Patrimonios { get; set; }

    public virtual DbSet<Percepcion> Percepciones { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Propiedad> Propiedads { get; set; }

    public virtual DbSet<PropiedadesGrupo> PropiedadesGrupos { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Provision> Provisiones { get; set; }

    public virtual DbSet<ReciboCobro> ReciboCobros { get; set; }

    public virtual DbSet<ReciboCuota> ReciboCuotas { get; set; }

    public virtual DbSet<ReciboNomina> ReciboNominas { get; set; }

    public virtual DbSet<ReciboReserva> ReciboReservas { get; set; }

    public virtual DbSet<ReferenciasPe> ReferenciasPes { get; set; }

    public virtual DbSet<ReferenciasPr> ReferenciasPrs { get; set; }

    public virtual DbSet<RelacionGasto> RelacionGastos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<SubCuenta> SubCuenta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-1GR3ADJ;Database=nuevaApp;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Modern_Spanish_CI_AS");

        modelBuilder.Entity<Activo>(entity =>
        {
            entity.HasKey(e => e.IdActivo);

            entity.Property(e => e.IdActivo).HasColumnName("id_activo");
            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

            entity.HasOne(d => d.IdAsientoNavigation).WithMany(p => p.Activos)
                .HasForeignKey(d => d.IdAsiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Activos_LDiario_Global");
        });

        modelBuilder.Entity<Anticipo>(entity =>
        {
            entity.HasKey(e => e.IdAnticipo);

            entity.ToTable("Anticipo");

            entity.Property(e => e.Detalle).HasMaxLength(50);
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.Saldo).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCodCuentaNavigation).WithMany(p => p.Anticipos)
                .HasForeignKey(d => d.IdCodCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Anticipo_CodigoCuentas_Global");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Anticipos)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Anticipo_Proveedor");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<BalanceComprobacion>(entity =>
        {
            entity.HasKey(e => e.IdBalanceC);

            entity.ToTable("Balance_Comprobacion");

            entity.Property(e => e.IdBalanceC).HasColumnName("id_balanceC");
            entity.Property(e => e.Diferencia)
                .HasColumnType("money")
                .HasColumnName("diferencia");
            entity.Property(e => e.Fecha)
                .HasColumnType("money")
                .HasColumnName("fecha");
            entity.Property(e => e.SaldoFinal)
                .HasColumnType("money")
                .HasColumnName("saldo_final");
            entity.Property(e => e.SaldoInicial)
                .HasColumnType("money")
                .HasColumnName("saldo_inicial");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.BalanceComprobacions)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Balance_Comprobacion_Condominio");
        });

        modelBuilder.Entity<Clase>(entity =>
        {
            entity.ToTable("Clase");

            entity.Property(e => e.Codigo).HasMaxLength(1);
            entity.Property(e => e.Descripcion).HasMaxLength(10);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);

            entity.ToTable("Cliente");

            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Representante).HasMaxLength(50);
            entity.Property(e => e.Rif).HasMaxLength(15);
            entity.Property(e => e.Saldo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Telefono).HasMaxLength(50);

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_Condominio");

            entity.HasOne(d => d.IdRetencionIslrNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdRetencionIslr)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_islr");

            entity.HasOne(d => d.IdRetencionIvaNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdRetencionIva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_iva");
        });

        modelBuilder.Entity<CodigoCuentasGlobal>(entity =>
        {
            entity.HasKey(e => e.IdCodCuenta);

            entity.ToTable("CodigoCuentas_Global");

            entity.Property(e => e.IdCodCuenta).HasColumnName("id_codCuenta");
            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .HasColumnName("codigo");
            entity.Property(e => e.IdSubCuenta).HasColumnName("id_SubCuenta");
            entity.Property(e => e.Saldo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SaldoInicial).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdClaseNavigation).WithMany(p => p.CodigoCuentasGlobals)
                .HasForeignKey(d => d.IdClase)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CodigoCuentas_Global_Clase");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.CodigoCuentasGlobals)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CodigoCuentas_Global_Condominio");

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.CodigoCuentasGlobals)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CodigoCuentas_Global_Cuenta");

            entity.HasOne(d => d.IdGrupoNavigation).WithMany(p => p.CodigoCuentasGlobals)
                .HasForeignKey(d => d.IdGrupo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CodigoCuentas_Global_Grupo");

            entity.HasOne(d => d.IdSubCuentaNavigation).WithMany(p => p.CodigoCuentasGlobals)
                .HasForeignKey(d => d.IdSubCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CodigoCuentas_Global_SubCuenta");
        });

        modelBuilder.Entity<ComprobanteRetencion>(entity =>
        {
            entity.HasKey(e => e.IdComprobante);

            entity.ToTable("ComprobanteRetencion");

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.FechaEmision).HasColumnType("datetime");
            entity.Property(e => e.Retencion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sustraendo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalImpuesto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ValorRetencion).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.ComprobanteRetencions)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComprobanteRetencion_Factura");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.ComprobanteRetencions)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComprobanteRetencion_Proveedor");
        });

        modelBuilder.Entity<ComprobanteRetencionCliente>(entity =>
        {
            entity.HasKey(e => e.IdComprobanteCliente);

            entity.ToTable("ComprobanteRetencionCliente");

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.FechaEmision).HasColumnType("datetime");
            entity.Property(e => e.Retencion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sustraendo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalImpuesto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ValorRetencion).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ComprobanteRetencionClientes)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComprobanteRetencionCliente_Cliente");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.ComprobanteRetencionClientes)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ComprobanteRetencionCliente_FacturaEmitida");
        });

        modelBuilder.Entity<Condominio>(entity =>
        {
            entity.HasKey(e => e.IdCondominio);

            entity.ToTable("Condominio");

            entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.IdAdministrador)
                .HasMaxLength(450)
                .HasColumnName("id_administrador");
            entity.Property(e => e.InteresMora).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Rif)
                .HasMaxLength(20)
                .HasColumnName("rif");
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdAdministradorNavigation).WithMany(p => p.Condominios)
                .HasForeignKey(d => d.IdAdministrador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Condominio_AspNetUsers");
        });

        modelBuilder.Entity<CondominioNomina>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CondominioNomina");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany()
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CondominioNomina_Condominio");

            entity.HasOne(d => d.IdReciboNominaNavigation).WithMany()
                .HasForeignKey(d => d.IdReciboNomina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CondominioNomina_Recibo_Nomina");
        });

        modelBuilder.Entity<CuentasCobrar>(entity =>
        {
            entity.ToTable("CuentasCobrar");

            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.CuentasCobrars)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CuentasCobrar_Condominio");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.CuentasCobrars)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CuentasCobrar_Factura");
        });

        modelBuilder.Entity<CuentasGrupo>(entity =>
        {
            entity.HasKey(e => e.IdCuentaGrupos);

            entity.HasOne(d => d.IdCodCuentaNavigation).WithMany(p => p.CuentasGrupos)
                .HasForeignKey(d => d.IdCodCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CuentasGrupos_CodigoCuentas_Global");

            entity.HasOne(d => d.IdGrupoGastoNavigation).WithMany(p => p.CuentasGrupos)
                .HasForeignKey(d => d.IdGrupoGasto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CuentasGrupos_GrupoGastos");
        });

        modelBuilder.Entity<CuentasPagar>(entity =>
        {
            entity.ToTable("CuentasPagar");

            entity.Property(e => e.Monto)
                .HasComment("Monto a pagar agregando retenciones")
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.CuentasPagars)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CuentasPagar_Condominio");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.CuentasPagars)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CuentasPagar_Factura");
        });

        modelBuilder.Entity<Cuenta>(entity =>
        {
            entity.Property(e => e.Codigo).HasMaxLength(2);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsFixedLength();

            entity.HasOne(d => d.IdGrupoNavigation).WithMany(p => p.Cuenta)
                .HasForeignKey(d => d.IdGrupo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cuenta_Grupo");
        });

        modelBuilder.Entity<CuotasEspeciale>(entity =>
        {
            entity.HasKey(e => e.IdCuotaEspecial);

            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.MontoMensual).HasColumnType("money");
            entity.Property(e => e.MontoTotal).HasColumnType("money");
            entity.Property(e => e.SimboloMoneda)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SimboloRef)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SubCuotas).HasColumnType("money");
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.CuotasEspeciales)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CuotasEspeciales_Condominio");
        });

        modelBuilder.Entity<Deduccion>(entity =>
        {
            entity.HasKey(e => e.IdDeduccion);

            entity.Property(e => e.Concepto).HasMaxLength(50);
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefMonto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Deducciones)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deducciones_Empleado");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.IdEmpleado);

            entity.ToTable("Empleado");

            entity.Property(e => e.Apellido).HasMaxLength(30);
            entity.Property(e => e.BaseSueldo).HasColumnType("money");
            entity.Property(e => e.FechaIngreso).HasColumnName("Fecha_ingreso");
            entity.Property(e => e.Nombre).HasMaxLength(30);
            entity.Property(e => e.RefMonto).HasColumnType("money");
        });

        modelBuilder.Entity<EstadoResultado>(entity =>
        {
            entity.HasKey(e => e.IdEstResultado);

            entity.ToTable("Estado_Resultado");

            entity.Property(e => e.IdEstResultado).HasColumnName("id_estResultado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.MontoRef).HasColumnType("money");
            entity.Property(e => e.SimboloMoneda).HasMaxLength(2);
            entity.Property(e => e.SimboloRef).HasMaxLength(2);
            entity.Property(e => e.TotalGastos)
                .HasColumnType("money")
                .HasColumnName("total_gastos");
            entity.Property(e => e.TotalIngresos)
                .HasColumnType("money")
                .HasColumnName("total_ingresos");
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.EstadoResultados)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Estado_Resultado_Condominio");
        });

        modelBuilder.Entity<EstadoSituacion>(entity =>
        {
            entity.HasKey(e => e.IdEstSituacion);

            entity.ToTable("Estado_Situacion");

            entity.Property(e => e.IdEstSituacion).HasColumnName("id_estSituacion");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.TotalAct)
                .HasColumnType("money")
                .HasColumnName("total_act");
            entity.Property(e => e.TotalPas)
                .HasColumnType("money")
                .HasColumnName("total_pas");
            entity.Property(e => e.TotalPat)
                .HasColumnType("money")
                .HasColumnName("total_pat");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.EstadoSituacions)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Estado_Situacion_Condominio");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.IdFactura);

            entity.ToTable("Factura");

            entity.Property(e => e.Abonado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.FechaEmision).HasColumnType("datetime");
            entity.Property(e => e.FechaVencimiento).HasColumnType("datetime");
            entity.Property(e => e.Iva)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("iva");
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NumControl).HasMaxLength(20);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCodCuentaNavigation).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.IdCodCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_CodigoCuentas_Global");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_Proveedor");
        });

        modelBuilder.Entity<FacturaEmitida>(entity =>
        {
            entity.HasKey(e => e.IdFacturaEmitida);

            entity.Property(e => e.Abonado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.FechaEmision).HasColumnType("datetime");
            entity.Property(e => e.FechaVencimiento).HasColumnType("datetime");
            entity.Property(e => e.Iva)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("iva");
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NumControl).HasMaxLength(20);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.FacturaEmitida)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FacturaEmitida_Producto");
        });

        modelBuilder.Entity<Fondo>(entity =>
        {
            entity.HasKey(e => e.IdFondo);

            entity.Property(e => e.IdFondo).HasColumnName("id_Fondo");
            entity.Property(e => e.IdCodCuenta).HasColumnName("id_codCuenta");
            entity.Property(e => e.Porcentaje).HasColumnName("porcentaje");
            entity.Property(e => e.Saldo).HasColumnType("money");

            entity.HasOne(d => d.IdCodCuentaNavigation).WithMany(p => p.Fondos)
                .HasForeignKey(d => d.IdCodCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fondos_CodigoCuentas_Global");
        });

        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.HasKey(e => e.IdGasto);

            entity.Property(e => e.IdGasto).HasColumnName("id_gasto");
            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

            entity.HasOne(d => d.IdAsientoNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdAsiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gastos_LDiario_Global");
        });

        modelBuilder.Entity<Grupo>(entity =>
        {
            entity.ToTable("Grupo");

            entity.Property(e => e.Codigo).HasMaxLength(1);
            entity.Property(e => e.Descripcion).HasMaxLength(50);

            entity.HasOne(d => d.IdClaseNavigation).WithMany(p => p.Grupos)
                .HasForeignKey(d => d.IdClase)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grupo_Clase");
        });

        modelBuilder.Entity<GrupoGasto>(entity =>
        {
            entity.HasKey(e => e.IdGrupoGasto);

            entity.Property(e => e.NombreGrupo).HasMaxLength(50);
        });

        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.HasKey(e => e.IdIngreso);

            entity.Property(e => e.IdIngreso).HasColumnName("id_ingreso");
            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

            entity.HasOne(d => d.IdAsientoNavigation).WithMany(p => p.Ingresos)
                .HasForeignKey(d => d.IdAsiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingresos_LDiario_Global");
        });

        modelBuilder.Entity<Islr>(entity =>
        {
            entity.ToTable("islr");

            entity.Property(e => e.BaseImponible).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Concepto).HasMaxLength(250);
            entity.Property(e => e.Factor).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Modo).HasMaxLength(50);
            entity.Property(e => e.MontoDesde).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MontoHasta).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Pjuridica).HasColumnName("PJuridica");
            entity.Property(e => e.Pnatural).HasColumnName("PNatural");
            entity.Property(e => e.Sustraendo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Tarifa).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnidadTributaria).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Iva>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Retenciones");

            entity.ToTable("iva");

            entity.Property(e => e.Descripcion).HasMaxLength(50);
            entity.Property(e => e.Porcentaje).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<LdiarioGlobal>(entity =>
        {
            entity.HasKey(e => e.IdAsiento);

            entity.ToTable("LDiario_Global");

            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");
            entity.Property(e => e.Concepto)
                .HasMaxLength(30)
                .HasColumnName("concepto");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdCodCuenta).HasColumnName("id_codCuenta");
            entity.Property(e => e.Monto)
                .HasColumnType("smallmoney")
                .HasColumnName("monto");
            entity.Property(e => e.MontoRef).HasColumnType("money");
            entity.Property(e => e.NumAsiento).HasColumnName("num_Asiento");
            entity.Property(e => e.SimboloMoneda).HasMaxLength(2);
            entity.Property(e => e.SimboloRef).HasMaxLength(2);
            entity.Property(e => e.TipoOperacion).HasColumnName("tipo_operacion");
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdCodCuentaNavigation).WithMany(p => p.LdiarioGlobals)
                .HasForeignKey(d => d.IdCodCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LDiario_Global_CodigoCuentas_Global");
        });

        modelBuilder.Entity<LibroCompra>(entity =>
        {
            entity.Property(e => e.BaseImponible).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ExentoIva)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("ExentoIVA");
            entity.Property(e => e.FechaComprobanteRet).HasColumnType("datetime");
            entity.Property(e => e.Igtf)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("IGTF");
            entity.Property(e => e.Iva)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("IVA");
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RetIslr)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("RetISLR");
            entity.Property(e => e.RetIva)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("RetIVA");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.LibroCompras)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LibroCompras_Condominio");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.LibroCompras)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LibroCompras_Factura");
        });

        modelBuilder.Entity<LibroVenta>(entity =>
        {
            entity.Property(e => e.BaseImponible)
                .HasComment("Monto SubTotal de la factura")
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IdCondominio).HasComment("Condominio");
            entity.Property(e => e.IdFactura).HasComment("Factura");
            entity.Property(e => e.Iva)
                .HasComment("IVA de la factura")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("IVA");
            entity.Property(e => e.Monto)
                .HasComment("Monto a cobrar")
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NumComprobanteRet).HasComment("Numero de comprobacion de retencion");
            entity.Property(e => e.RetIslr)
                .HasComment("Retencion islr dependiendo del condominio (tangibles)")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("RetISLR");
            entity.Property(e => e.RetIva)
                .HasComment("Retencion de iva dependiendo del condominio ret")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("RetIVA");
            entity.Property(e => e.Total)
                .HasComment("suma del iva y la base imponible")
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.LibroVenta)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LibroVentas_Condominio");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.LibroVenta)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LibroVentas_Factura");
        });

        modelBuilder.Entity<MonedaCond>(entity =>
        {
            entity.HasKey(e => e.IdMonedaCond);

            entity.ToTable("MonedaCond");

            entity.Property(e => e.Simbolo).HasMaxLength(2);
            entity.Property(e => e.ValorDolar)
                .HasComment("Valor de la moneda respecto al dolar")
                .HasColumnType("money");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.MonedaConds)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonedaCond_Condominio");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.MonedaConds)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonedaCond_Moneda");
        });

        modelBuilder.Entity<MonedaCuenta>(entity =>
        {
            entity.Property(e => e.IdCodCuenta).HasComment("Codigo Sub cuenta del condominio");
            entity.Property(e => e.IdMoneda).HasComment("Moneda asignada");
            entity.Property(e => e.RecibePagos).HasComment("Mostrar cuenta en pago del propietario");
            entity.Property(e => e.SaldoFinal).HasColumnType("money");
            entity.Property(e => e.SaldoInicial).HasColumnType("money");

            entity.HasOne(d => d.IdCodCuentaNavigation).WithMany(p => p.MonedaCuenta)
                .HasForeignKey(d => d.IdCodCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonedaCuenta_CodigoCuentas_Global");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.MonedaCuenta)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonedaCuenta_Moneda");
        });

        modelBuilder.Entity<Moneda>(entity =>
        {
            entity.HasKey(e => e.IdMoneda);

            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Pais).HasMaxLength(150);
        });

        modelBuilder.Entity<PagoAnticipo>(entity =>
        {
            entity.ToTable("PagoAnticipo");

            entity.HasOne(d => d.IdAnticipoNavigation).WithMany(p => p.PagoAnticipos)
                .HasForeignKey(d => d.IdAnticipo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoAnticipo_Anticipo");

            entity.HasOne(d => d.IdPagoEmitidoNavigation).WithMany(p => p.PagoAnticipos)
                .HasForeignKey(d => d.IdPagoEmitido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoAnticipo_Pago_Emitido");
        });

        modelBuilder.Entity<PagoEmitido>(entity =>
        {
            entity.HasKey(e => e.IdPagoEmitido);

            entity.ToTable("Pago_Emitido");

            entity.Property(e => e.IdPagoEmitido).HasColumnName("id_pagoEmitido");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FormaPago).HasColumnName("forma_pago");
            entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");
            entity.Property(e => e.Monto)
                .HasColumnType("money")
                .HasColumnName("monto");
            entity.Property(e => e.MontoRef).HasColumnType("money");
            entity.Property(e => e.SimboloMoneda).HasMaxLength(2);
            entity.Property(e => e.SimboloRef).HasMaxLength(2);
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.PagoEmitidos)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Emitido_Condominio");
        });

        modelBuilder.Entity<PagoFactura>(entity =>
        {
            entity.ToTable("PagoFactura");

            entity.HasOne(d => d.IdAnticipoNavigation).WithMany(p => p.PagoFacturas)
                .HasForeignKey(d => d.IdAnticipo)
                .HasConstraintName("FK__PagoFactura_Anticipo");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.PagoFacturas)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoFactura_Factura");

            entity.HasOne(d => d.IdPagoEmitidoNavigation).WithMany(p => p.PagoFacturas)
                .HasForeignKey(d => d.IdPagoEmitido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoFactura_Pago_Emitido");
        });

        modelBuilder.Entity<PagoFacturaEmitida>(entity =>
        {
            entity.HasKey(e => e.IdPagoFacturaEmitida);

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.PagoFacturaEmitida)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoFacturaEmitida_FacturaEmitida");

            entity.HasOne(d => d.IdPagoRecibidoNavigation).WithMany(p => p.PagoFacturaEmitida)
                .HasForeignKey(d => d.IdPagoRecibido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoFacturaEmitida_Pago_Recibido");
        });

        modelBuilder.Entity<PagoRecibido>(entity =>
        {
            entity.HasKey(e => e.IdPagoRecibido);

            entity.ToTable("Pago_Recibido");

            entity.Property(e => e.IdPagoRecibido).HasColumnName("id_pagoRecibido");
            entity.Property(e => e.Concepto)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("concepto");
            entity.Property(e => e.Confirmado).HasColumnName("confirmado");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.FormaPago).HasColumnName("forma_pago");
            entity.Property(e => e.IdPropiedad).HasColumnName("id_propiedad");
            entity.Property(e => e.IdSubCuenta).HasColumnName("idSubCuenta");
            entity.Property(e => e.Monto)
                .HasColumnType("money")
                .HasColumnName("monto");
            entity.Property(e => e.MontoRef).HasColumnType("money");
            entity.Property(e => e.SimboloMoneda).HasMaxLength(2);
            entity.Property(e => e.SimboloRef).HasMaxLength(2);
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdPropiedadNavigation).WithMany(p => p.PagoRecibidos)
                .HasForeignKey(d => d.IdPropiedad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Recibido_Propiedad");
        });

        modelBuilder.Entity<PagoReserva>(entity =>
        {
            entity.HasKey(e => e.IdPagoReserva);

            entity.ToTable("PagoReserva");

            entity.Property(e => e.IdPagoReserva).ValueGeneratedOnAdd();

            entity.HasOne(d => d.IdPagoReservaNavigation).WithOne(p => p.PagoReserva)
                .HasForeignKey<PagoReserva>(d => d.IdPagoReserva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoReserva_Pago_Recibido");

            entity.HasOne(d => d.IdReciboReservaNavigation).WithMany(p => p.PagoReservas)
                .HasForeignKey(d => d.IdReciboReserva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagoReserva_Recibo_Reserva");
        });

        modelBuilder.Entity<PagosCuota>(entity =>
        {
            entity.HasKey(e => e.IdPagoCuota);

            entity.HasOne(d => d.IdPagoRecibidoNavigation).WithMany(p => p.PagosCuota)
                .HasForeignKey(d => d.IdPagoRecibido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagosCuotas_Pago_Recibido");

            entity.HasOne(d => d.IdReciboCuotaNavigation).WithMany(p => p.PagosCuota)
                .HasForeignKey(d => d.IdReciboCuota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagosCuotas_ReciboCuotas");
        });

        modelBuilder.Entity<PagosNomina>(entity =>
        {
            entity.ToTable("PagosNomina");

            entity.HasOne(d => d.IdPagoEmitidoNavigation).WithMany(p => p.PagosNominas)
                .HasForeignKey(d => d.IdPagoEmitido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagosNomina_Pago_Emitido");

            entity.HasOne(d => d.IdReciboNominaNavigation).WithMany(p => p.PagosNominas)
                .HasForeignKey(d => d.IdReciboNomina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagosNomina_PagosNomina");
        });

        modelBuilder.Entity<PagosRecibo>(entity =>
        {
            entity.HasKey(e => e.IdPagoRecibo);

            entity.ToTable("PagosRecibo");

            entity.HasOne(d => d.IdPagoNavigation).WithMany(p => p.PagosRecibos)
                .HasForeignKey(d => d.IdPago)
                .HasConstraintName("FK_PagosRecibo_Pago_Recibido");

            entity.HasOne(d => d.IdReciboNavigation).WithMany(p => p.PagosRecibos)
                .HasForeignKey(d => d.IdRecibo)
                .HasConstraintName("FK_PagosRecibo_Recibo_Cobro");
        });

        modelBuilder.Entity<Pasivo>(entity =>
        {
            entity.HasKey(e => e.IdPasivo);

            entity.Property(e => e.IdPasivo).HasColumnName("id_pasivo");
            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

            entity.HasOne(d => d.IdAsientoNavigation).WithMany(p => p.Pasivos)
                .HasForeignKey(d => d.IdAsiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pasivos_LDiario_Global");
        });

        modelBuilder.Entity<Patrimonio>(entity =>
        {
            entity.HasKey(e => e.IdPatrimonio);

            entity.ToTable("Patrimonio");

            entity.Property(e => e.IdPatrimonio).HasColumnName("id_patrimonio");
            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

            entity.HasOne(d => d.IdAsientoNavigation).WithMany(p => p.Patrimonios)
                .HasForeignKey(d => d.IdAsiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Patrimonio_LDiario_Global");
        });

        modelBuilder.Entity<Percepcion>(entity =>
        {
            entity.HasKey(e => e.IdPercepcion);

            entity.Property(e => e.Concepto).HasMaxLength(50);
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefMonto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Percepciones)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Percepciones_Empleado");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto);

            entity.ToTable("Producto");

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Precio).HasColumnType("money");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_Condominio");

            entity.HasOne(d => d.IdRetencionIslrNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdRetencionIslr)
                .HasConstraintName("FK_Producto_islr");

            entity.HasOne(d => d.IdRetencionIvaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdRetencionIva)
                .HasConstraintName("FK_Producto_iva");
        });

        modelBuilder.Entity<Propiedad>(entity =>
        {
            entity.HasKey(e => e.IdPropiedad);

            entity.ToTable("Propiedad");

            entity.Property(e => e.IdPropiedad).HasColumnName("id_propiedad");
            entity.Property(e => e.Alicuota)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("alicuota");
            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .HasColumnName("codigo");
            entity.Property(e => e.Deuda)
                .HasColumnType("money")
                .HasColumnName("deuda");
            entity.Property(e => e.Dimensiones)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dimensiones");
            entity.Property(e => e.IdUsuario)
                .HasMaxLength(450)
                .HasColumnName("id_usuario");
            entity.Property(e => e.MontoIntereses).HasColumnType("money");
            entity.Property(e => e.Saldo)
                .HasColumnType("money")
                .HasColumnName("saldo");
            entity.Property(e => e.Solvencia).HasColumnName("solvencia");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.Propiedads)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Propiedad_Condominio");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Propiedads)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Propiedad_AspNetUsers");
        });

        modelBuilder.Entity<PropiedadesGrupo>(entity =>
        {
            entity.HasKey(e => e.IdPropiedadGrupo);

            entity.Property(e => e.Alicuota).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.IdGrupoGastoNavigation).WithMany(p => p.PropiedadesGrupos)
                .HasForeignKey(d => d.IdGrupoGasto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PropiedadesGrupos_GrupoGastos");

            entity.HasOne(d => d.IdPropiedadNavigation).WithMany(p => p.PropiedadesGrupos)
                .HasForeignKey(d => d.IdPropiedad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PropiedadesGrupos_Propiedad");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor);

            entity.ToTable("Proveedor");

            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Representante).HasMaxLength(50);
            entity.Property(e => e.Rif).HasMaxLength(15);
            entity.Property(e => e.Saldo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Telefono).HasMaxLength(50);

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.Proveedors)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Proveedor_Condominio");

            entity.HasOne(d => d.IdRetencionIslrNavigation).WithMany(p => p.Proveedors)
                .HasForeignKey(d => d.IdRetencionIslr)
                .HasConstraintName("FK_Proveedor_islr");

            entity.HasOne(d => d.IdRetencionIvaNavigation).WithMany(p => p.Proveedors)
                .HasForeignKey(d => d.IdRetencionIva)
                .HasConstraintName("FK_Proveedor_iva");
        });

        modelBuilder.Entity<Provision>(entity =>
        {
            entity.HasKey(e => e.IdProvision);

            entity.Property(e => e.IdProvision).HasColumnName("id_provision");
            entity.Property(e => e.IdCodCuenta).HasColumnName("id_codCuenta");
            entity.Property(e => e.IdCodGasto).HasColumnName("id_codGasto");
            entity.Property(e => e.Monto)
                .HasColumnType("smallmoney")
                .HasColumnName("monto");
            entity.Property(e => e.MontoRef).HasColumnType("money");
            entity.Property(e => e.SimboloMoneda).HasMaxLength(2);
            entity.Property(e => e.SimboloRef).HasMaxLength(2);
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdCodCuentaNavigation).WithMany(p => p.ProvisioneIdCodCuentaNavigations)
                .HasForeignKey(d => d.IdCodCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Provisiones_CodigoCuentas_Global");

            entity.HasOne(d => d.IdCodGastoNavigation).WithMany(p => p.ProvisioneIdCodGastoNavigations)
                .HasForeignKey(d => d.IdCodGasto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Provisiones_CodigoCuentas_Global1");
        });

        modelBuilder.Entity<ReciboCobro>(entity =>
        {
            entity.HasKey(e => e.IdReciboCobro);

            entity.ToTable("Recibo_Cobro");

            entity.Property(e => e.IdReciboCobro).HasColumnName("id_reciboCobro");
            entity.Property(e => e.Abonado).HasColumnType("money");
            entity.Property(e => e.EnProceso).HasColumnName("enProceso");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdPropiedad).HasColumnName("id_propiedad");
            entity.Property(e => e.IdRgastos).HasColumnName("id_rgastos");
            entity.Property(e => e.Monto)
                .HasColumnType("money")
                .HasColumnName("monto");
            entity.Property(e => e.MontoRef).HasColumnType("money");
            entity.Property(e => e.Pagado).HasColumnName("pagado");
            entity.Property(e => e.SimboloMoneda).HasMaxLength(2);
            entity.Property(e => e.SimboloRef).HasMaxLength(2);
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdPropiedadNavigation).WithMany(p => p.ReciboCobros)
                .HasForeignKey(d => d.IdPropiedad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recibo_Cobro_Propiedad");

            entity.HasOne(d => d.IdRgastosNavigation).WithMany(p => p.ReciboCobros)
                .HasForeignKey(d => d.IdRgastos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recibo_Cobro_Relacion_Gastos");
        });

        modelBuilder.Entity<ReciboCuota>(entity =>
        {
            entity.HasKey(e => e.IdReciboCuotas);

            entity.Property(e => e.Abonado).HasColumnType("money");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.SimboloMoneda)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.SimboloRef)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.SubCuotas).HasColumnType("money");
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdCuotaEspecialNavigation).WithMany(p => p.ReciboCuota)
                .HasForeignKey(d => d.IdCuotaEspecial)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReciboCuotas_CuotasEspeciales");

            entity.HasOne(d => d.IdPropiedadNavigation).WithMany(p => p.ReciboCuota)
                .HasForeignKey(d => d.IdPropiedad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReciboCuotas_Propiedad");
        });

        modelBuilder.Entity<ReciboNomina>(entity =>
        {
            entity.HasKey(e => e.IdReciboNomina);

            entity.ToTable("Recibo_Nomina");

            entity.Property(e => e.Concepto).HasMaxLength(50);
            entity.Property(e => e.Entregado).HasColumnName("entregado");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");
            entity.Property(e => e.PagoTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefMonto).HasColumnType("money");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.ReciboNominas)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recibo_Nomina_Empleado");
        });

        modelBuilder.Entity<ReciboReserva>(entity =>
        {
            entity.HasKey(e => e.IdReciboReserva);

            entity.ToTable("Recibo_Reserva");

            entity.Property(e => e.Abonado).HasColumnType("money");
            entity.Property(e => e.Monto).HasColumnType("money");
            entity.Property(e => e.MontoRef).HasColumnType("money");
            entity.Property(e => e.SimboloMoneda).HasMaxLength(2);
            entity.Property(e => e.SimboloRef).HasMaxLength(2);
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.ReciboReservas)
                .HasForeignKey(d => d.IdReserva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recibo_Reserva_Reserva");
        });

        modelBuilder.Entity<ReferenciasPe>(entity =>
        {
            entity.HasKey(e => e.IdReferencia);

            entity.ToTable("Referencias_PE");

            entity.Property(e => e.IdReferencia).HasColumnName("id_referencia");
            entity.Property(e => e.Banco)
                .HasMaxLength(30)
                .HasColumnName("banco");
            entity.Property(e => e.IdPagoEmitido).HasColumnName("id_pagoEmitido");
            entity.Property(e => e.NumReferencia).HasColumnName("num_referencia");

            entity.HasOne(d => d.IdPagoEmitidoNavigation).WithMany(p => p.ReferenciasPes)
                .HasForeignKey(d => d.IdPagoEmitido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Referencias_PE_Pago_Emitido");
        });

        modelBuilder.Entity<ReferenciasPr>(entity =>
        {
            entity.HasKey(e => e.IdReferencia).HasName("PK_Referencias_PR_1");

            entity.ToTable("Referencias_PR");

            entity.Property(e => e.IdReferencia).HasColumnName("id_referencia");
            entity.Property(e => e.Banco)
                .HasMaxLength(70)
                .HasColumnName("banco");
            entity.Property(e => e.IdPagoRecibido).HasColumnName("id_pagoRecibido");
            entity.Property(e => e.NumReferencia).HasColumnName("num_referencia");

            entity.HasOne(d => d.IdPagoRecibidoNavigation).WithMany(p => p.ReferenciasPrs)
                .HasForeignKey(d => d.IdPagoRecibido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Referencias_PR_Pago_Recibido");
        });

        modelBuilder.Entity<RelacionGasto>(entity =>
        {
            entity.HasKey(e => e.IdRgastos);

            entity.ToTable("Relacion_Gastos");

            entity.Property(e => e.IdRgastos).HasColumnName("id_rgastos");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");
            entity.Property(e => e.MontoRef).HasColumnType("money");
            entity.Property(e => e.SimboloMoneda).HasMaxLength(2);
            entity.Property(e => e.SimboloRef).HasMaxLength(2);
            entity.Property(e => e.SubTotal)
                .HasColumnType("money")
                .HasColumnName("sub_total");
            entity.Property(e => e.TotalMensual)
                .HasColumnType("money")
                .HasColumnName("total_mensual");
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.RelacionGastos)
                .HasForeignKey(d => d.IdCondominio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Relacion_Gastos_Condominio");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva);

            entity.ToTable("Reserva");

            entity.Property(e => e.IdReserva).ValueGeneratedNever();
            entity.Property(e => e.TipoEvento).HasMaxLength(50);

            entity.HasOne(d => d.IdPropiedadNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdPropiedad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Propiedad");
        });

        modelBuilder.Entity<SubCuenta>(entity =>
        {
            entity.Property(e => e.Codigo).HasMaxLength(2);
            entity.Property(e => e.Descricion).HasMaxLength(70);

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.SubCuenta)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubCuenta_Cuenta");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
