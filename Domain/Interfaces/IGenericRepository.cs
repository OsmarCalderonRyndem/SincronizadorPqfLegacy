namespace SincronizadorPqfLegacy.Domain.Interfaces
{

    
    /// <summary>
    /// Defines a generic repository for managing entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>This interface provides methods for common data access operations, such as adding, updating, 
    /// deleting, and querying entities. It is designed to abstract the underlying data storage mechanism,  allowing for
    /// flexibility in implementation.</remarks>
    /// <typeparam name="T">The type of entity managed by the repository. Must be a reference type.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        Task<Guid> AddOrUpdate(T entity);
        Task<bool> Delete(Guid id);
        Task<T?> GetById(Guid id);
        IQueryable<T> Query(bool asNoTracking = true);
        Task<bool> Exists(Guid id);

    }
}
