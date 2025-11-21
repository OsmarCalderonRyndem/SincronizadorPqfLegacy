using Microservicio.Application.Exceptions;
using Microservicio.Application.Interfaces.Validators;
using Microservicio.Domain.Models;
using Serilog;

namespace Microservicio.Application.Validators
{
    /// <summary>
    /// Provides validation logic for <see cref="DocumentTemplate"/> instances.
    /// </summary>
    /// <remarks>This validator ensures that a <see cref="DocumentTemplate"/> instance meets the required
    /// conditions for its properties. Specifically, it checks for the presence of required file names when
    /// corresponding template flags are set. If any validation rule is violated, an exception is thrown.</remarks>
    public class TableDummyValidator : IObjectValidator<TableDummyDomain>
    {

        public Task ValidateAsync(TableDummyDomain? entity)
        {
            if (entity == null)
            {
                Log.Error("The model DocumentTemplate cannot be null.");
                throw new AppArgumentException("The model DocumentTemplate cannot be null.", nameof(entity));
            }
            return Task.CompletedTask;
        }
    }
}
