using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Prueba.Models;

public partial class CondominioContext : DbContext
{
    public CondominioContext()
    {
    }

    public CondominioContext(DbContextOptions<CondominioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CuotasEspeciale> CuotasEspeciales { get; set; }

    public virtual DbSet<PagoReciboCuota> PagoReciboCuota { get; set; }

    public virtual DbSet<PagosCuotasRecibido> PagosCuotasRecibidos { get; set; }

    public virtual DbSet<ReciboCuota> ReciboCuotas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-1GR3ADJ;Database=Condominio;Integrated Security=True;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuotasEspeciale>(entity =>
        {
            entity.HasKey(e => e.IdCuotaEspecial).HasName("PK__CuotasEs__58ADC9EC8D59A7C0");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FechaFin).HasColumnType("date");
            entity.Property(e => e.FechaInicio).HasColumnType("date");
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
        });

        modelBuilder.Entity<PagoReciboCuota>(entity =>
        {
            entity.HasKey(e => e.IdPagoRecibido).HasName("PK__PagoReci__A9AD5F26DD8EF760");

            entity.Property(e => e.IdPagoRecibido).HasColumnName("Id_PagoRecibido");
            entity.Property(e => e.Concepto)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Confirmado).HasColumnName("confirmado");
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.FormaPago).HasColumnName("Forma_pago");
            entity.Property(e => e.IdCuota).HasColumnName("Id_Cuota");
            entity.Property(e => e.IdPropiedad).HasColumnName("Id_Propiedad");
            entity.Property(e => e.IdSubCuenta).HasColumnName("idSubCuenta");
            entity.Property(e => e.Monto).HasColumnType("money");
            entity.Property(e => e.MontoRef).HasColumnType("money");
            entity.Property(e => e.SimboloMoneda)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.SimboloRef)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.ValorDolar).HasColumnType("money");

            entity.HasOne(d => d.IdCuotaNavigation).WithMany(p => p.PagoReciboCuota)
                .HasForeignKey(d => d.IdCuota)
                .HasConstraintName("Fk_Relacion_PagoCuotas_PagoRecibido");
        });

        modelBuilder.Entity<PagosCuotasRecibido>(entity =>
        {
            entity.HasKey(e => e.IdPagoRecibido).HasName("PK__PagosCuo__520DB5BF279635E2");

            entity.HasOne(d => d.IdPagoNavigation).WithMany(p => p.PagosCuotasRecibidos)
                .HasForeignKey(d => d.IdPago)
                .HasConstraintName("Fk_Relacion_PagosCuotasRecibidos_PagoReciboCuota");

            entity.HasOne(d => d.IdRecibidoNavigation).WithMany(p => p.PagosCuotasRecibidos)
                .HasForeignKey(d => d.IdRecibido)
                .HasConstraintName("Fk_Relacion_PagosCuotas_RecibosCoutas");
        });

        modelBuilder.Entity<ReciboCuota>(entity =>
        {
            entity.HasKey(e => e.IdReciboCuotas).HasName("PK__ReciboCu__19AE8F411CBD3B43");

            entity.Property(e => e.Abonado).HasColumnType("money");
            entity.Property(e => e.Fecha).HasColumnType("date");
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
                .HasConstraintName("Fk_Recibo_Cuota");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
