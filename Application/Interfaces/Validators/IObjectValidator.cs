namespace Microservicio.Application.Interfaces.Validators
{
    /// <summary>
    /// Defines a contract for validating objects of a specified type.
    /// </summary>
    /// <remarks>Implementations of this interface should provide validation logic for objects of type
    /// <typeparamref name="T"/>. Validation may include checking for required properties, ensuring data integrity, or
    /// enforcing business rules.</remarks>
    /// <typeparam name="T">The type of object to validate. Must be a reference type.</typeparam>
    public interface IObjectValidator<T> where T : class
    {
        Task ValidateAsync(T entity);
    }
}
