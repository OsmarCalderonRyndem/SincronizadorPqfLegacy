using SincronizadorPqfLegacy.Domain.Models;
using Microsoft.EntityFrameworkCore;


namespace SincronizadorPqfLegacy.Infrastructure.Persistence.Context;

public partial class MicroservicioContext : DbContext
{
    public MicroservicioContext() { }

    public MicroservicioContext(DbContextOptions<MicroservicioContext> options)
        : base(options) { }


    public virtual DbSet<SyncJobLog> SyncJobLogs { get; set; }
}
