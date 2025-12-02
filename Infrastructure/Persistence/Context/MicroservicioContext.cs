using Microsoft.EntityFrameworkCore;

namespace SincronizadorPqfLegacy.Infrastructure.Persistence.Context;

/// <summary>
/// Contexto de base de datos para el microservicio
/// Base de datos: DocumentBuilder
/// </summary>
public partial class MicroservicioContext : DbContext
{
    public MicroservicioContext() { }

    public MicroservicioContext(DbContextOptions<MicroservicioContext> options)
        : base(options) { }

    // Este contexto está reservado para futuras entidades del microservicio
    // Los logs de sincronización están en PConnectProquifaDotNetContext

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
