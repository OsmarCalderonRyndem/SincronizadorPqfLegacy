using System;
using System.Collections.Generic;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Cotizacione>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("TR_Cotizaciones_PreventErvapharma"));

            entity.Property(e => e.IdMapeoProquifaLegacy).HasDefaultValueSql("(newid())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
