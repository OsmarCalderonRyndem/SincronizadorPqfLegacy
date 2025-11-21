using Microservicio.Domain.Interfaces;
using Microservicio.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microservicio.Infrastructure.Persistence
{
    /// <summary>
    /// Provides a mechanism for managing and coordinating repositories and saving changes to the underlying data store.
    /// </summary>
    /// <remarks>The <see cref="UnitOfWork"/> class implements the Unit of Work design pattern, which ensures
    /// that multiple operations performed on repositories are treated as a single transaction. This class provides
    /// access to specific repositories and a method to persist changes asynchronously.</remarks>
    public class UnitOfWork(MicroservicioContext context) : IUnitOfWork
    {
        private readonly MicroservicioContext _context = context;
        private IDbContextTransaction? _transaction; // Marked as nullable to avoid nullability warnings.

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null) // Added null check to avoid CS8602.
            {
                await _transaction.CommitAsync();
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null) // Added null check to avoid CS8602.
            {
                await _transaction.RollbackAsync();
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task Dispose()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }

            if (_context != null)
            {
                await _context.DisposeAsync();
            }
        }
    }
}
