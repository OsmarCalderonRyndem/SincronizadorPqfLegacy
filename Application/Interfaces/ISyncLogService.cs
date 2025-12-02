using SincronizadorPqfLegacy.Domain.Models;

namespace SincronizadorPqfLegacy.Application.Interfaces
{
    public interface ISyncLogService
    {
        Task LogPermanentFailureAsync(string entityName, string recordIdentifier, Exception ex);
        Task LogSuccessAsync(string entityName, string recordIdentifier);
    }
}
