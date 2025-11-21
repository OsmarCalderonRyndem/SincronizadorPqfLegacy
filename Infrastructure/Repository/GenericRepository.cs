using Microservicio.Domain.Interfaces;
using Microservicio.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Microservicio.Infrastructure.Repository
{
    
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly MicroservicioContext _context;
        private readonly DbSet<T> _set;
        private readonly PropertyInfo _keyClrProperty;

        /// <summary>
        /// Initializes a new instance of the repository backed by the provided <see cref="DbContext"/>.
        /// </summary>
        /// <param name="context">An EF Core <see cref="DbContext"/> that tracks <typeparamref name="T"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if EF Core model metadata for <typeparamref name="T"/> cannot be found or lacks a primary key.</exception>
        /// <exception cref="NotSupportedException">Thrown if the entity has a composite key or the PK type is not <see cref="Guid"/>.</exception>
        public GenericRepository(MicroservicioContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _set = _context.Set<T>();

            // Discover EF Core model metadata and cache the PK property.
            var entityType = _context.Model.FindEntityType(typeof(T))
                ?? throw new InvalidOperationException($"Entity type {typeof(T).Name} not found in DbContext model.");

            var pk = entityType.FindPrimaryKey()
                ?? throw new InvalidOperationException($"Entity type {typeof(T).Name} has no primary key configured.");

            if (pk.Properties.Count != 1)
                throw new NotSupportedException($"Only single-column primary keys are supported. {typeof(T).Name} defines {pk.Properties.Count} PK columns.");

            var keyName = pk.Properties[0].Name;

            _keyClrProperty = typeof(T).GetProperty(keyName)
                ?? throw new InvalidOperationException($"Primary key property '{keyName}' not found on CLR type {typeof(T).Name}.");

            if (_keyClrProperty.PropertyType != typeof(Guid))
                throw new NotSupportedException($"Primary key for {typeof(T).Name} must be Guid. Found {_keyClrProperty.PropertyType.Name}.");
        }

        /// <summary>
        /// Adds a new entity or updates an existing one based on its <see cref="Guid"/> primary key.
        /// </summary>
        /// <remarks>
        /// Behavior:
        /// <list type="bullet">
        ///   <item><description>If the entity's Id is <see cref="Guid.Empty"/>, a new <see cref="Guid"/> is generated, assigned to the entity, and the entity is added.</description></item>
        ///   <item><description>If the Id is non-empty but not found in the database, the entity is added (upsert semantics).</description></item>
        ///   <item><description>If the Id exists in the database, the existing tracked entity is updated with the incoming values.</description></item>
        /// </list>
        /// This method does <b>not</b> call <see cref="SaveChanges"/>; invoke it separately to persist changes.
        /// </remarks>
        /// <param name="entity">The entity instance to add or update.</param>
        /// <returns>The entity's (new or existing) <see cref="Guid"/> primary key.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
        public async Task<Guid> AddOrUpdate(T entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            var idObj = _keyClrProperty.GetValue(entity);
            var id = idObj is Guid g ? g : Guid.Empty;

            // INSERT
            if (id == Guid.Empty)
            {
                id = Guid.NewGuid();
                _keyClrProperty.SetValue(entity, id);

                // TIP: If your entity implements IAuditable, you can set audit fields here:
                // if (entity is IAuditable a) { a.RegistrationDate = DateTime.UtcNow; a.LastUpdateDate = DateTime.UtcNow; }

                await _set.AddAsync(entity).ConfigureAwait(false);
                return id;
            }

            // UPDATE (merge that ignores null values)
            var existing = await _set.FindAsync(new object[] { id }).ConfigureAwait(false);

            if (existing is null)
            {
                // Entity does not exist in DB -> treat it as an upsert (insert with id).
                // Make sure ALL required non-null fields are set on 'entity'.
                await _set.AddAsync(entity).ConfigureAwait(false);
                return id;
            }

            var entry = _context.Entry(existing);

            foreach (var prop in entry.Properties)
            {
                // Skip PK
                if (prop.Metadata.IsPrimaryKey()) continue;

                var pi = prop.Metadata.PropertyInfo;
                if (pi == null) continue;

                var newValue = pi.GetValue(entity);

                // Preserve existing value when the new entity property is null
                if (newValue is null)
                {
                    // Optional: if you use auditing and want to set LastUpdateDate, do it outside this loop
                    continue;
                }

                prop.CurrentValue = newValue;
            }

            // TIP: If you use auditing:
            // if (existing is IAuditable ex) ex.LastUpdateDate = DateTime.UtcNow;

            return id;
        }

        /// <summary>
        /// Removes an entity by its <see cref="Guid"/> primary key.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns><c>true</c> if the entity was found and staged for removal; otherwise, <c>false</c>.</returns>
        /// <remarks>This method does <b>not</b> call <see cref="SaveChanges"/>.</remarks>
        public async Task<bool> Delete(Guid id)
        {
            var existing = await _set.FindAsync(id).ConfigureAwait(false);
            if (existing is null) return false;

            _set.Remove(existing);
            return true;
        }

        /// <summary>
        /// Checks whether an entity with the specified <see cref="Guid"/> primary key exists.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns><c>true</c> if an entity exists; otherwise, <c>false</c>.</returns>
        public async Task<bool> Exists(Guid id)
        {
            var entityType = _context.Model.FindEntityType(typeof(T))!;
            var keyName = entityType.FindPrimaryKey()!.Properties[0].Name;

            // Translates to SQL: WHERE EF.Property<Guid>(e, keyName) = @id
            return await _set.AsNoTracking()
                .AnyAsync(e => EF.Property<Guid>(e, keyName) == id)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a single entity by its <see cref="Guid"/> primary key.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The entity instance if found; otherwise, <c>null</c>.</returns>
        public async Task<T?> GetById(Guid id)
        {
            return await _set.FindAsync(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns an <see cref="IQueryable{T}"/> filtered in-memory using the provided predicate.
        /// </summary>
        /// <remarks>
        /// Because the parameter type is <see cref="Func{T, Boolean}"/> (not an expression),
        /// EF Core cannot translate the predicate to SQL. The method enumerates the set in-memory
        /// using <see cref="Enumerable.Where{TSource}(System.Collections.Generic.IEnumerable{TSource}, System.Func{TSource, bool})"/>.
        /// For large datasets, prefer an overload that accepts <c>Expression&lt;Func&lt;T,bool&gt;&gt;</c>
        /// so the filtering can be performed server-side.
        /// </remarks>
        /// <param name="predicate">The in-memory predicate to apply.</param>
        /// <returns>An <see cref="IQueryable{T}"/> representing the filtered sequence.</returns>
        public IQueryable<T> Query(bool asNoTracking = true)
        {
            var query = _context.Set<T>().AsQueryable();
            return asNoTracking ? query.AsNoTracking() : query;
        }

        /// <summary>
        /// Persists all staged changes in the underlying <see cref="DbContext"/>.
        /// </summary>
        /// <returns><c>true</c> if one or more changes were saved; otherwise, <c>false</c>.</returns>
        public async Task<bool> SaveChanges()
        {
            return (await _context.SaveChangesAsync().ConfigureAwait(false)) > 0;
        }
    }
}
