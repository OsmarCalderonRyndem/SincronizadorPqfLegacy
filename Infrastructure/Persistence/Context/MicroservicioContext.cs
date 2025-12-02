using Infrastructure.Persistence.PConnectProquifaDotNet.Entities;
using Microsoft.EntityFrameworkCore;
using SincronizadorPqfLegacy.Infrastructure.Models;

namespace SincronizadorPqfLegacy.Infrastructure.Persistence.Context;

public partial class MicroservicioContext : DbContext
{
    public MicroservicioContext() { }

    public MicroservicioContext(DbContextOptions<MicroservicioContext> options)
        : base(options) { }

    public virtual DbSet<TableDummy> TableDummies { get; set; }
    public virtual DbSet<SyncJobLog> SyncJobLogs { get; set; }
}
