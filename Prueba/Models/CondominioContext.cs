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

    public virtual DbSet<PagosCuota> PagosCuotas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-1GR3ADJ;Database=Condominio;Integrated Security=True;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PagosCuota>(entity =>
        {
            entity.HasKey(e => e.IdPagoRecibido).HasName("PK__PagosCuo__520DB5BFCBA61A27");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
