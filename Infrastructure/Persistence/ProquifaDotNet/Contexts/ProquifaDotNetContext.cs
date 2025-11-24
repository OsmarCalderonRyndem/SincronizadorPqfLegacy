using System;
using System.Collections.Generic;
using Infrastructure.Persistence.ProquifaDotNet.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ProquifaDotNet.Contexts;

public partial class ProquifaDotNetContext : DbContext
{
    public ProquifaDotNetContext(DbContextOptions<ProquifaDotNetContext> options)
        : base(options)
    {
    }

    public virtual DbSet<cotCotizacion> cotCotizacions { get; set; }

    public virtual DbSet<vCotizacionesTransformadasETL> vCotizacionesTransformadasETLs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<cotCotizacion>(entity =>
        {
            entity.ToTable("cotCotizacion", tb =>
                {
                    tb.HasTrigger("Trigger_AfterInsert_CotCotizacion_Archivo");
                    tb.HasTrigger("Trigger_AfterUpdate_CotCotizacion_Archivo");
                });

            entity.Property(e => e.IdCotCotizacion).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Caducada).HasDefaultValue(false);
            entity.Property(e => e.CotizacionDeInvestigacion).HasDefaultValueSql("('0')");
            entity.Property(e => e.EnviadaConInvestigacion).HasDefaultValueSql("('0')");
            entity.Property(e => e.Enviado).HasDefaultValue(false);
            entity.Property(e => e.FleteDesglosado).HasDefaultValue(true);
            entity.Property(e => e.SeGuardanPartidasInvestigacion).HasDefaultValueSql("('0')");
            entity.Property(e => e.TipoCambioEsDOF).HasDefaultValueSql("('0')");

            entity.HasOne(d => d.IdCotCotizacionOriginalNavigation).WithMany(p => p.InverseIdCotCotizacionOriginalNavigation).HasConstraintName("FK_cotCotizacion_cotCotizacion");
        });

        modelBuilder.Entity<vCotizacionesTransformadasETL>(entity =>
        {
            entity.ToView("vCotizacionesTransformadasETL");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
