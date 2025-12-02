using Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;
using Infrastructure.Persistence.PConnectProquifaDotNet.Entities;
using Microsoft.EntityFrameworkCore;
using SincronizadorPqfLegacy.Domain.Interfaces;

namespace SincronizadorPqfLegacy.Infrastructure.Repository
{
    public class SyncJobLogRepository : IGenericRepository<SyncJobLog>
    {
        private readonly PConnectProquifaDotNetContext _context;
        private readonly DbSet<SyncJobLog> _set;

        public SyncJobLogRepository(PConnectProquifaDotNetContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _set = _context.Set<SyncJobLog>();
        }

        public async Task<Guid> AddOrUpdate(SyncJobLog entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            if (entity.IdSyncJobLog == Guid.Empty)
            {
                entity.IdSyncJobLog = Guid.NewGuid();
                await _set.AddAsync(entity);
                await _context.SaveChangesAsync(); // Save immediately as per requirement for logs
                return entity.IdSyncJobLog;
            }

            var existing = await _set.FindAsync(entity.IdSyncJobLog);
            if (existing == null)
            {
                await _set.AddAsync(entity);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync();
            return entity.IdSyncJobLog;
        }

        public async Task<bool> Delete(Guid id)
        {
            var existing = await _set.FindAsync(id);
            if (existing == null) return false;

            _set.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Exists(Guid id)
        {
            return await _set.AsNoTracking().AnyAsync(e => e.IdSyncJobLog == id);
        }

        public async Task<SyncJobLog?> GetById(Guid id)
        {
            return await _set.FindAsync(id);
        }

        public IQueryable<SyncJobLog> Query(bool asNoTracking = true)
        {
            var query = _set.AsQueryable();
            return asNoTracking ? query.AsNoTracking() : query;
        }
    }
}
