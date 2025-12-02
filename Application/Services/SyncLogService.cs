using Infrastructure.Persistence.PConnectProquifaDotNet.Entities;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Domain.Interfaces;

namespace SincronizadorPqfLegacy.Application.Services
{
    public class SyncLogService(IGenericRepository<SyncJobLog> repository) : ISyncLogService
    {
        private readonly IGenericRepository<SyncJobLog> _repository = repository;

        public async Task LogPermanentFailureAsync(string entityName, Guid recordIdentifier, Exception ex)
        {
            var log = new SyncJobLog
            {
                NombreEntidad = entityName,
                IdentificadorRegistro = recordIdentifier,
                Estado = "Fallo persistente",
                MensajeError = ex.Message,
                FechaProcesamiento = DateTime.Now,
                FechaRegistro = DateTime.Now
            };

            await _repository.AddOrUpdate(log);
        }

        public async Task LogSuccessAsync(string entityName, Guid recordIdentifier)
        {
            var log = new SyncJobLog
            {
                NombreEntidad = entityName,
                IdentificadorRegistro = recordIdentifier,
                Estado = "Sincronizado",
                MensajeError = string.Empty,
                FechaProcesamiento = DateTime.Now,
                FechaRegistro = DateTime.Now
            };

            await _repository.AddOrUpdate(log);
        }
    }
}
