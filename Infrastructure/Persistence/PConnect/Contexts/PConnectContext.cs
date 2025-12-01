using System;
using System.Collections.Generic;
using Infrastructure.Persistence.PConnect.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.PConnect.Contexts;

public partial class PConnectContext : DbContext
{
    public PConnectContext(DbContextOptions<PConnectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cotiza> Cotizas { get; set; }

    public virtual DbSet<PCotiza> PCotizas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cotiza>(entity =>
        {
            entity.HasIndex(e => e.IMoneda, "IMonedaPesos").HasFilter("([IMoneda]='Pesos')");

            entity.Property(e => e.Abierto).HasDefaultValue(true);
            entity.Property(e => e.CPago).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Clave).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Cliente).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Confirmo).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Contacto).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Cotizo).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Estado).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.FK02_DoctosR).HasDefaultValue(1);
            entity.Property(e => e.FS).HasDefaultValue(false);
            entity.Property(e => e.Factura).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.HEntrada).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.HSalida).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.IMoneda).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.InfoFacturacion).HasDefaultValue(false);
            entity.Property(e => e.Lugar).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.MEntrada).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.MSalida).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Moneda).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Observa).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.ObservaC).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Parciales).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Vendedor).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Vigencia).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Zona).UseCollation("SQL_Latin1_General_CP1_CI_AS");
        });

        modelBuilder.Entity<PCotiza>(entity =>
        {
            entity.HasIndex(e => e.Estado, "indexPrueba5").HasFilter("([PCotizas].[estado]<>'Recotizada')");

            entity.Property(e => e.Clasif)
                .IsFixedLength()
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Clave).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Codigo).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Concepto).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Destino).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Estado).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.FK01_PCotizaOrigen).HasDefaultValue(0);
            entity.Property(e => e.Fabrica).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.HEnvio).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.MEnvio)
                .IsFixedLength()
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Nota).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.ObservaE).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Presentacion).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.TEntrega).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Unidades).UseCollation("SQL_Latin1_General_CP1_CI_AS");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
