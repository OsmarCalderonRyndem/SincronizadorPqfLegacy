using Microservicio.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Infrastructure.Persistence.Context;

public partial class MicroservicioContext : DbContext
{
    public MicroservicioContext() { }

    public MicroservicioContext(DbContextOptions<MicroservicioContext> options)
        : base(options) { }

    public virtual DbSet<TableDummy> TableDummies { get; set; }


}
