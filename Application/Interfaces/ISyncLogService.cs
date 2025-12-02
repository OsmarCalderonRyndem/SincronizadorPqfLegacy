using SincronizadorPqfLegacy.Domain.Models;

namespace SincronizadorPqfLegacy.Application.Interfaces
{
    public interface ISyncLogService
    {
        Task LogPermanentFailureAsync(string entityName, Guid recordIdentifier, Exception ex);
        Task LogSuccessAsync(string entityName, Guid recordIdentifier);
    }
}
