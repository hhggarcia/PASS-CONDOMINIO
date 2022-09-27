using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Prueba.Models;

namespace Prueba.Context
{
    public partial class PruebaContext : DbContext
    {
        public PruebaContext()
        {
        }

        public PruebaContext(DbContextOptions<PruebaContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Activo> Activos { get; set; } = null!;
        public virtual DbSet<AreaComun> AreaComuns { get; set; } = null!;
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; } = null!;
        public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; } = null!;
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; } = null!;
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; } = null!;
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; } = null!;
        public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; } = null!;
        public virtual DbSet<BalanceComprobacion> BalanceComprobacions { get; set; } = null!;
        public virtual DbSet<CambioSueldo> CambioSueldos { get; set; } = null!;
        public virtual DbSet<Clase> Clases { get; set; } = null!;
        public virtual DbSet<CodigoCuentasGlobal> CodigoCuentasGlobals { get; set; } = null!;
        public virtual DbSet<Condominio> Condominios { get; set; } = null!;
        public virtual DbSet<Cuenta> Cuenta { get; set; } = null!;
        public virtual DbSet<Empleado> Empleados { get; set; } = null!;
        public virtual DbSet<Estacionamiento> Estacionamientos { get; set; } = null!;
        public virtual DbSet<Estado> Estados { get; set; } = null!;
        public virtual DbSet<EstadoResultado> EstadoResultados { get; set; } = null!;
        public virtual DbSet<EstadoSituacion> EstadoSituacions { get; set; } = null!;
        public virtual DbSet<Factura> Facturas { get; set; } = null!;
        public virtual DbSet<Gasto> Gastos { get; set; } = null!;
        public virtual DbSet<Grupo> Grupos { get; set; } = null!;
        public virtual DbSet<Ingreso> Ingresos { get; set; } = null!;
        public virtual DbSet<Inmueble> Inmuebles { get; set; } = null!;
        public virtual DbSet<LdiarioGlobal> LdiarioGlobals { get; set; } = null!;
        public virtual DbSet<Municipio> Municipios { get; set; } = null!;
        public virtual DbSet<PagoEmitido> PagoEmitidos { get; set; } = null!;
        public virtual DbSet<PagoRecibido> PagoRecibidos { get; set; } = null!;
        public virtual DbSet<Pais> Pais { get; set; } = null!;
        public virtual DbSet<Parroquia> Parroquias { get; set; } = null!;
        public virtual DbSet<Pasivo> Pasivos { get; set; } = null!;
        public virtual DbSet<Patrimonio> Patrimonios { get; set; } = null!;
        public virtual DbSet<Propiedad> Propiedads { get; set; } = null!;
        public virtual DbSet<Proveedor> Proveedors { get; set; } = null!;
        public virtual DbSet<PuestoE> PuestoEs { get; set; } = null!;
        public virtual DbSet<ReciboCobro> ReciboCobros { get; set; } = null!;
        public virtual DbSet<ReciboNomina> ReciboNominas { get; set; } = null!;
        public virtual DbSet<ReferenciasPe> ReferenciasPes { get; set; } = null!;
        public virtual DbSet<ReferenciasPr> ReferenciasPrs { get; set; } = null!;
        public virtual DbSet<RegistroNomina> RegistroNominas { get; set; } = null!;
        public virtual DbSet<RelacionGasto> RelacionGastos { get; set; } = null!;
        public virtual DbSet<SubCuenta> SubCuenta { get; set; } = null!;
        public virtual DbSet<Zona> Zonas { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server= DESKTOP-NF5DL1U\\SQLEXPRESS01; Database= Prueba;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Activo>(entity =>
            {
                entity.HasKey(e => e.IdActivo);

                entity.Property(e => e.IdActivo).HasColumnName("id_activo");

                entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

                entity.HasOne(d => d.IdAsientoNavigation)
                    .WithMany(p => p.Activos)
                    .HasForeignKey(d => d.IdAsiento)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Activos_LDiario_Global");
            });

            modelBuilder.Entity<AreaComun>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Area_Comun");

                entity.Property(e => e.Disponible).HasColumnName("disponible");

                entity.Property(e => e.IdAcomun).HasColumnName("id_acomun");

                entity.Property(e => e.IdInmueble).HasColumnName("id_inmueble");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(30)
                    .HasColumnName("nombre");
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

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
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

                entity.HasMany(d => d.Roles)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "AspNetUserRole",
                        l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                        r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
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

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserToken>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<BalanceComprobacion>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Balance_Comprobacion");

                entity.Property(e => e.Diferencia)
                    .HasColumnType("money")
                    .HasColumnName("diferencia");

                entity.Property(e => e.Fecha)
                    .HasColumnType("money")
                    .HasColumnName("fecha");

                entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

                entity.Property(e => e.IdBalanceC).HasColumnName("id_balanceC");

                entity.Property(e => e.SaldoFinal)
                    .HasColumnType("money")
                    .HasColumnName("saldo_final");

                entity.Property(e => e.SaldoInicial)
                    .HasColumnType("money")
                    .HasColumnName("saldo_inicial");
            });

            modelBuilder.Entity<CambioSueldo>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Cambio_Sueldo");

                entity.Property(e => e.Cargo)
                    .HasMaxLength(30)
                    .HasColumnName("cargo");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.IdCsueldo).HasColumnName("id_csueldo");

                entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");

                entity.Property(e => e.Salario)
                    .HasColumnType("money")
                    .HasColumnName("salario");
            });

            modelBuilder.Entity<Clase>(entity =>
            {
                entity.ToTable("Clase");

                entity.Property(e => e.Codigo).HasMaxLength(1);

                entity.Property(e => e.Descripcion).HasMaxLength(10);
            });

            modelBuilder.Entity<CodigoCuentasGlobal>(entity =>
            {
                entity.HasKey(e => e.IdCodCuenta);

                entity.ToTable("CodigoCuentas_Global");

                entity.Property(e => e.IdCodCuenta).HasColumnName("id_codCuenta");

                entity.Property(e => e.IdCodigo).HasColumnName("id_Codigo");

                entity.Property(e => e.IdCondominio).HasColumnName("id_Condominio");

                entity.HasOne(d => d.IdCodigoNavigation)
                    .WithMany(p => p.CodigoCuentasGlobals)
                    .HasForeignKey(d => d.IdCodigo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CodigoCuentas_Global_SubCuenta");

                entity.HasOne(d => d.IdCondominioNavigation)
                    .WithMany(p => p.CodigoCuentasGlobals)
                    .HasForeignKey(d => d.IdCondominio)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CodigoCuentas_Global_Condominio1");
            });

            modelBuilder.Entity<Condominio>(entity =>
            {
                entity.HasKey(e => e.IdCondominio);

                entity.ToTable("Condominio");

                entity.HasIndex(e => e.IdAdministrador, "IX_Condominio")
                    .IsUnique();

                entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");

                entity.Property(e => e.IdAdministrador).HasColumnName("id_administrador");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(50)
                    .HasColumnName("nombre");

                entity.Property(e => e.Rif)
                    .HasMaxLength(20)
                    .HasColumnName("rif");

                entity.Property(e => e.Tipo)
                    .HasMaxLength(30)
                    .HasColumnName("tipo");

                entity.HasOne(d => d.IdAdministradorNavigation)
                    .WithOne(p => p.Condominio)
                    .HasForeignKey<Condominio>(d => d.IdAdministrador)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Condominio_AspNetUsers");
            });

            modelBuilder.Entity<Cuenta>(entity =>
            {
                entity.Property(e => e.Codigo).HasMaxLength(2);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(50)
                    .IsFixedLength();

                entity.HasOne(d => d.IdGrupoNavigation)
                    .WithMany(p => p.Cuenta)
                    .HasForeignKey(d => d.IdGrupo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cuenta_Grupo");
            });

            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Empleado");

                entity.Property(e => e.Apellido)
                    .HasMaxLength(30)
                    .HasColumnName("apellido");

                entity.Property(e => e.Cedula).HasColumnName("cedula");

                entity.Property(e => e.Estado).HasColumnName("estado");

                entity.Property(e => e.FechaIngreso)
                    .HasColumnType("date")
                    .HasColumnName("fecha_ingreso");

                entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");

                entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(30)
                    .HasColumnName("nombre");
            });

            modelBuilder.Entity<Estacionamiento>(entity =>
            {
                entity.HasKey(e => e.IdEstacionamiento);

                entity.ToTable("Estacionamiento");

                entity.Property(e => e.IdEstacionamiento).HasColumnName("id_estacionamiento");

                entity.Property(e => e.IdInmueble).HasColumnName("id_inmueble");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(30)
                    .HasColumnName("nombre");

                entity.Property(e => e.NumPuestos).HasColumnName("num_puestos");

                entity.HasOne(d => d.IdInmuebleNavigation)
                    .WithMany(p => p.Estacionamientos)
                    .HasForeignKey(d => d.IdInmueble)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Estacionamiento_Inmueble");
            });

            modelBuilder.Entity<Estado>(entity =>
            {
                entity.HasKey(e => e.IdEstado);

                entity.ToTable("Estado");

                entity.Property(e => e.IdEstado).HasColumnName("id_estado");

                entity.Property(e => e.IdPais).HasColumnName("id_pais");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(30)
                    .HasColumnName("nombre");

                entity.HasOne(d => d.IdPaisNavigation)
                    .WithMany(p => p.Estados)
                    .HasForeignKey(d => d.IdPais)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Estado_Pais");
            });

            modelBuilder.Entity<EstadoResultado>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Estado_Resultado");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.IdEstResultado).HasColumnName("id_estResultado");

                entity.Property(e => e.IdGasto).HasColumnName("id_gasto");

                entity.Property(e => e.IdIngreso).HasColumnName("id_ingreso");

                entity.Property(e => e.TotalGastos)
                    .HasColumnType("money")
                    .HasColumnName("total_gastos");

                entity.Property(e => e.TotalIngresos)
                    .HasColumnType("money")
                    .HasColumnName("total_ingresos");
            });

            modelBuilder.Entity<EstadoSituacion>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Estado_Situacion");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.IdActivo).HasColumnName("id_activo");

                entity.Property(e => e.IdEstSituacion).HasColumnName("id_estSituacion");

                entity.Property(e => e.IdPasivo).HasColumnName("id_pasivo");

                entity.Property(e => e.IdPatrimonio).HasColumnName("id_patrimonio");

                entity.Property(e => e.TotalAct)
                    .HasColumnType("money")
                    .HasColumnName("total_act");

                entity.Property(e => e.TotalPas)
                    .HasColumnType("money")
                    .HasColumnName("total_pas");

                entity.Property(e => e.TotalPat)
                    .HasColumnType("money")
                    .HasColumnName("total_pat");
            });

            modelBuilder.Entity<Factura>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Factura");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");

                entity.Property(e => e.IdFactura).HasColumnName("id_factura");

                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");

                entity.Property(e => e.NumFactura).HasColumnName("num_factura");

                entity.Property(e => e.RazonSocial)
                    .HasMaxLength(30)
                    .HasColumnName("razon_social");

                entity.Property(e => e.Total)
                    .HasColumnType("money")
                    .HasColumnName("total");
            });

            modelBuilder.Entity<Gasto>(entity =>
            {
                entity.HasKey(e => e.IdGasto);

                entity.Property(e => e.IdGasto).HasColumnName("id_gasto");

                entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

                entity.HasOne(d => d.IdAsientoNavigation)
                    .WithMany(p => p.Gastos)
                    .HasForeignKey(d => d.IdAsiento)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Gastos_LDiario_Global");
            });

            modelBuilder.Entity<Grupo>(entity =>
            {
                entity.ToTable("Grupo");

                entity.Property(e => e.Codigo).HasMaxLength(1);

                entity.Property(e => e.Descripcion).HasMaxLength(50);

                entity.HasOne(d => d.IdClaseNavigation)
                    .WithMany(p => p.Grupos)
                    .HasForeignKey(d => d.IdClase)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Grupo_Clase");
            });

            modelBuilder.Entity<Ingreso>(entity =>
            {
                entity.HasKey(e => e.IdIngreso);

                entity.Property(e => e.IdIngreso).HasColumnName("id_ingreso");

                entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

                entity.HasOne(d => d.IdAsientoNavigation)
                    .WithMany(p => p.Ingresos)
                    .HasForeignKey(d => d.IdAsiento)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ingresos_LDiario_Global");
            });

            modelBuilder.Entity<Inmueble>(entity =>
            {
                entity.HasKey(e => e.IdInmueble);

                entity.ToTable("Inmueble");

                entity.Property(e => e.IdInmueble)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id_inmueble");

                entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");

                entity.Property(e => e.IdZona).HasColumnName("id_zona");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(30)
                    .HasColumnName("nombre");

                entity.Property(e => e.TotalPropiedad).HasColumnName("total_propiedad");

                entity.HasOne(d => d.IdCondominioNavigation)
                    .WithMany(p => p.Inmuebles)
                    .HasForeignKey(d => d.IdCondominio)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Inmueble_Condominio");

                entity.HasOne(d => d.IdInmuebleNavigation)
                    .WithOne(p => p.Inmueble)
                    .HasForeignKey<Inmueble>(d => d.IdInmueble)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Inmueble_Zonas");
            });

            modelBuilder.Entity<LdiarioGlobal>(entity =>
            {
                entity.HasKey(e => e.IdAsiento);

                entity.ToTable("LDiario_Global");

                entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

                entity.Property(e => e.Concepto)
                    .HasMaxLength(30)
                    .HasColumnName("concepto");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(30)
                    .HasColumnName("descripcion");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.IdCodCuenta).HasColumnName("id_codCuenta");

                entity.Property(e => e.Monto)
                    .HasColumnType("money")
                    .HasColumnName("monto");

                entity.Property(e => e.NumAsiento).HasColumnName("num_Asiento");

                entity.Property(e => e.TipoOperacion).HasColumnName("tipo_operacion");

                entity.HasOne(d => d.IdCodCuentaNavigation)
                    .WithMany(p => p.LdiarioGlobals)
                    .HasForeignKey(d => d.IdCodCuenta)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LDiario_Global_CodigoCuentas_Global");
            });

            modelBuilder.Entity<Municipio>(entity =>
            {
                entity.HasKey(e => e.IdMunicipio);

                entity.Property(e => e.IdMunicipio).HasColumnName("id_municipio");

                entity.Property(e => e.IdEstado).HasColumnName("id_estado");

                entity.Property(e => e.Municipio1)
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .HasColumnName("municipio");

                entity.HasOne(d => d.IdEstadoNavigation)
                    .WithMany(p => p.Municipios)
                    .HasForeignKey(d => d.IdEstado)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Municipios_Estado");
            });

            modelBuilder.Entity<PagoEmitido>(entity =>
            {
                entity.HasKey(e => e.IdPagoEmitido);

                entity.ToTable("Pago_Emitido");

                entity.Property(e => e.IdPagoEmitido).HasColumnName("id_pagoEmitido");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.FormaPago).HasColumnName("forma_pago");

                entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");

                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");

                entity.Property(e => e.Monto)
                    .HasColumnType("money")
                    .HasColumnName("monto");

                entity.HasOne(d => d.IdCondominioNavigation)
                    .WithMany(p => p.PagoEmitidos)
                    .HasForeignKey(d => d.IdCondominio)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Pago_Emitido_Condominio");
            });

            modelBuilder.Entity<PagoRecibido>(entity =>
            {
                entity.HasKey(e => e.IdPagoRecibido);

                entity.ToTable("Pago_Recibido");

                entity.Property(e => e.IdPagoRecibido).HasColumnName("id_pagoRecibido");

                entity.Property(e => e.Conciliado).HasColumnName("conciliado");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.FormaPago).HasColumnName("forma_pago");

                entity.Property(e => e.IdPropiedad).HasColumnName("id_propiedad");

                entity.Property(e => e.Monto)
                    .HasColumnType("money")
                    .HasColumnName("monto");

                entity.HasOne(d => d.IdPropiedadNavigation)
                    .WithMany(p => p.PagoRecibidos)
                    .HasForeignKey(d => d.IdPropiedad)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Pago_Recibido_Propiedad");
            });

            modelBuilder.Entity<Pais>(entity =>
            {
                entity.HasKey(e => e.IdPais);

                entity.Property(e => e.IdPais).HasColumnName("id_pais");

                entity.Property(e => e.Abreviatura)
                    .HasMaxLength(2)
                    .HasColumnName("abreviatura");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(50)
                    .HasColumnName("nombre");
            });

            modelBuilder.Entity<Parroquia>(entity =>
            {
                entity.HasKey(e => e.IdParroquia);

                entity.Property(e => e.IdParroquia).HasColumnName("id_parroquia");

                entity.Property(e => e.IdMunicipio).HasColumnName("id_municipio");

                entity.Property(e => e.Parroquia1)
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .HasColumnName("parroquia");

                entity.Property(e => e.Urbana).HasColumnName("urbana");

                entity.HasOne(d => d.IdMunicipioNavigation)
                    .WithMany(p => p.Parroquia)
                    .HasForeignKey(d => d.IdMunicipio)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Parroquias_Municipios");
            });

            modelBuilder.Entity<Pasivo>(entity =>
            {
                entity.HasKey(e => e.IdPasivo);

                entity.Property(e => e.IdPasivo).HasColumnName("id_pasivo");

                entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");

                entity.HasOne(d => d.IdAsientoNavigation)
                    .WithMany(p => p.Pasivos)
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

                entity.HasOne(d => d.IdAsientoNavigation)
                    .WithMany(p => p.Patrimonios)
                    .HasForeignKey(d => d.IdAsiento)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Patrimonio_LDiario_Global");
            });

            modelBuilder.Entity<Propiedad>(entity =>
            {
                entity.HasKey(e => e.IdPropiedad);

                entity.ToTable("Propiedad");

                entity.Property(e => e.IdPropiedad).HasColumnName("id_propiedad");

                entity.Property(e => e.Alicuota)
                    .HasColumnType("decimal(18, 2)")
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

                entity.Property(e => e.IdInmueble).HasColumnName("id_inmueble");

                entity.Property(e => e.IdUsuario)
                    .HasMaxLength(450)
                    .HasColumnName("id_usuario");

                entity.Property(e => e.Saldo)
                    .HasColumnType("money")
                    .HasColumnName("saldo");

                entity.Property(e => e.Solvencia)
                    .HasColumnType("money")
                    .HasColumnName("solvencia");

                entity.HasOne(d => d.IdInmuebleNavigation)
                    .WithMany(p => p.Propiedads)
                    .HasForeignKey(d => d.IdInmueble)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Propiedad_Inmueble");

                entity.HasOne(d => d.IdUsuarioNavigation)
                    .WithMany(p => p.Propiedads)
                    .HasForeignKey(d => d.IdUsuario)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Propiedad_AspNetUsers");
            });

            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Proveedor");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(30)
                    .HasColumnName("descripcion");

                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(30)
                    .HasColumnName("nombre");

                entity.Property(e => e.Rif)
                    .HasMaxLength(20)
                    .HasColumnName("rif");

                entity.Property(e => e.Telefono).HasColumnName("telefono");
            });

            modelBuilder.Entity<PuestoE>(entity =>
            {
                entity.HasKey(e => e.IdPuestoE)
                    .HasName("PK_Puesto_E");

                entity.ToTable("PuestoE");

                entity.Property(e => e.IdPuestoE).HasColumnName("id_puestoE");

                entity.Property(e => e.Codigo)
                    .HasMaxLength(10)
                    .HasColumnName("codigo");

                entity.Property(e => e.IdEstacionamiento).HasColumnName("id_estacionamiento");

                entity.Property(e => e.IdPropiedad).HasColumnName("id_propiedad");

                entity.HasOne(d => d.IdEstacionamientoNavigation)
                    .WithMany(p => p.PuestoEs)
                    .HasForeignKey(d => d.IdEstacionamiento)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Puesto_E_Estacionamiento");

                entity.HasOne(d => d.IdPropiedadNavigation)
                    .WithMany(p => p.PuestoEs)
                    .HasForeignKey(d => d.IdPropiedad)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Puesto_E_Propiedad");
            });

            modelBuilder.Entity<ReciboCobro>(entity =>
            {
                entity.HasKey(e => e.IdReciboCobro);

                entity.ToTable("Recibo_Cobro");

                entity.Property(e => e.IdReciboCobro).HasColumnName("id_reciboCobro");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.IdPropiedad).HasColumnName("id_propiedad");

                entity.Property(e => e.IdRgastos).HasColumnName("id_rgastos");

                entity.Property(e => e.Monto)
                    .HasColumnType("money")
                    .HasColumnName("monto");

                entity.HasOne(d => d.IdPropiedadNavigation)
                    .WithMany(p => p.ReciboCobros)
                    .HasForeignKey(d => d.IdPropiedad)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Recibo_Cobro_Propiedad");

                entity.HasOne(d => d.IdRgastosNavigation)
                    .WithMany(p => p.ReciboCobros)
                    .HasForeignKey(d => d.IdRgastos)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Recibo_Cobro_Relacion_Gastos");
            });

            modelBuilder.Entity<ReciboNomina>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Recibo_Nomina");

                entity.Property(e => e.Entregado).HasColumnName("entregado");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");

                entity.Property(e => e.IdReciboNomina).HasColumnName("id_reciboNomina");
            });

            modelBuilder.Entity<ReferenciasPe>(entity =>
            {
                entity.HasKey(e => e.IdPagoEmitido)
                    .HasName("PK_Referencias_PE_1");

                entity.ToTable("Referencias_PE");

                entity.Property(e => e.IdPagoEmitido)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id_pagoEmitido");

                entity.Property(e => e.Banco)
                    .HasMaxLength(30)
                    .HasColumnName("banco");

                entity.Property(e => e.NumReferencia).HasColumnName("num_referencia");

                entity.HasOne(d => d.IdPagoEmitidoNavigation)
                    .WithOne(p => p.ReferenciasPe)
                    .HasForeignKey<ReferenciasPe>(d => d.IdPagoEmitido)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Referencias_PE_Pago_Emitido");
            });

            modelBuilder.Entity<ReferenciasPr>(entity =>
            {
                entity.HasKey(e => e.IdPagoRecibido);

                entity.ToTable("Referencias_PR");

                entity.Property(e => e.IdPagoRecibido)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id_pagoRecibido");

                entity.Property(e => e.Banco)
                    .HasMaxLength(30)
                    .HasColumnName("banco");

                entity.Property(e => e.NumReferencia).HasColumnName("num_referencia");

                entity.HasOne(d => d.IdPagoRecibidoNavigation)
                    .WithOne(p => p.ReferenciasPr)
                    .HasForeignKey<ReferenciasPr>(d => d.IdPagoRecibido)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Referencias_PR_Pago_Recibido");
            });

            modelBuilder.Entity<RegistroNomina>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Registro_Nomina");

                entity.Property(e => e.Asignaciones)
                    .HasColumnType("money")
                    .HasColumnName("asignaciones");

                entity.Property(e => e.Deducciones)
                    .HasColumnType("money")
                    .HasColumnName("deducciones");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(30)
                    .HasColumnName("descripcion");

                entity.Property(e => e.Dias).HasColumnName("dias");

                entity.Property(e => e.Horas).HasColumnName("horas");

                entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");

                entity.Property(e => e.IdReciboNomina).HasColumnName("id_reciboNomina");

                entity.Property(e => e.IdRegistroNomina).HasColumnName("id_registroNomina");
            });

            modelBuilder.Entity<RelacionGasto>(entity =>
            {
                entity.HasKey(e => e.IdRgastos);

                entity.ToTable("Relacion_Gastos");

                entity.Property(e => e.IdRgastos).HasColumnName("id_rgastos");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.GastosNomina)
                    .HasColumnType("money")
                    .HasColumnName("gastos_nomina");

                entity.Property(e => e.GastosPatrimonio)
                    .HasColumnType("money")
                    .HasColumnName("gastos_patrimonio");

                entity.Property(e => e.GastosServicios)
                    .HasColumnType("money")
                    .HasColumnName("gastos_servicios");

                entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");

                entity.Property(e => e.IdFactura).HasColumnName("id_factura");

                entity.Property(e => e.IdRegistroNomina).HasColumnName("id_registroNomina");

                entity.Property(e => e.SubTotal)
                    .HasColumnType("money")
                    .HasColumnName("sub_total");

                entity.Property(e => e.TotalMensual)
                    .HasColumnType("money")
                    .HasColumnName("total_mensual");

                entity.HasOne(d => d.IdCondominioNavigation)
                    .WithMany(p => p.RelacionGastos)
                    .HasForeignKey(d => d.IdCondominio)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Relacion_Gastos_Condominio");
            });

            modelBuilder.Entity<SubCuenta>(entity =>
            {
                entity.Property(e => e.Codigo).HasMaxLength(2);

                entity.Property(e => e.Descricion).HasMaxLength(70);

                entity.HasOne(d => d.IdCuentaNavigation)
                    .WithMany(p => p.SubCuenta)
                    .HasForeignKey(d => d.IdCuenta)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubCuenta_Cuenta");
            });

            modelBuilder.Entity<Zona>(entity =>
            {
                entity.HasKey(e => e.IdZona);

                entity.Property(e => e.IdZona).HasColumnName("id_zona");

                entity.Property(e => e.CodigoPostal).HasColumnName("codigo_postal");

                entity.Property(e => e.IdParroquia).HasColumnName("id_parroquia");

                entity.Property(e => e.Zona1)
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .HasColumnName("zona");

                entity.HasOne(d => d.IdParroquiaNavigation)
                    .WithMany(p => p.Zonas)
                    .HasForeignKey(d => d.IdParroquia)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Zonas_Parroquias");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
