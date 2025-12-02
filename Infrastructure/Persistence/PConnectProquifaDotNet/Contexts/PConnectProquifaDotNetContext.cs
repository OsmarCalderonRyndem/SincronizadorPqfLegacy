using System;
using System.Collections.Generic;
using SincronizadorPqfLegacy.Domain.Models;
using Infrastructure.Persistence.PConnectProquifaDotNet.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;

public partial class PConnectProquifaDotNetContext : DbContext
{
    public PConnectProquifaDotNetContext(DbContextOptions<PConnectProquifaDotNetContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cotizacione> Cotizaciones { get; set; }

    public virtual DbSet<SyncJobLog> SyncJobLogs { get; set; }

    public virtual DbSet<catEstadoTransferencium> catEstadoTransferencia { get; set; }

    public virtual DbSet<vETLCotizacionesPendiete> vETLCotizacionesPendietes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Cotizacione>(entity =>
        {
            entity.Property(e => e.IdMapeoProquifaLegacy).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<SyncJobLog>(entity =>
        {
            entity.Property(e => e.IdSyncJobLog).HasDefaultValueSql("(newid())");
            entity.Property(e => e.FechaProcesamiento).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<catEstadoTransferencium>(entity =>
        {
            entity.HasKey(e => e.IdCatEstadoTransferencia).HasName("PK__catEstad__7A509A95A587B83D");

            entity.Property(e => e.IdCatEstadoTransferencia).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<vETLCotizacionesPendiete>(entity =>
        {
            entity.ToView("vETLCotizacionesPendietes");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
