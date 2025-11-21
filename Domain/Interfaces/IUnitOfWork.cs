namespace Microservicio.Domain.Interfaces
{
    /// <summary>
    /// Defines a contract for a unit of work that manages repositories and coordinates the saving of changes to the
    /// data store.
    /// </summary>
    /// <remarks>This interface provides access to repositories and ensures that changes made through those
    /// repositories are persisted as a single atomic operation. It also implements <see cref="IDisposable"/> to allow
    /// proper resource cleanup.</remarks>
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<bool> SaveChangesAsync();
        Task Dispose();
    }
}
