using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

namespace SincronizadorPqfLegacy.Application.Services;

/// <summary>
/// Servicio para registro de logs de sincronización
/// Usa DTOs para todas las operaciones
/// </summary>
public class SyncLogService : ISyncLogService
{
    private readonly ISyncJobLogRepository _repository;
    private readonly ILogger<SyncLogService> _logger;

    public SyncLogService(
        ISyncJobLogRepository repository,
        ILogger<SyncLogService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registra un fallo permanente (no reintentar)
    /// </summary>
    public async Task LogPermanentFailureAsync(string entityName, Guid recordIdentifier, Exception ex)
    {
        try
        {
            _logger.LogWarning(ex,
                "Registrando fallo permanente: Entidad={Entidad}, ID={Id}",
                entityName, recordIdentifier);

            var logDto = new SyncJobLogDto
            {
                IdSyncJobLog = Guid.NewGuid(),
                NombreEntidad = entityName,
                IdentificadorRegistro = recordIdentifier,
                Estado = "Fallo persistente",
                MensajeError = $"{ex.GetType().Name}: {ex.Message}",
                FechaProcesamiento = DateTime.Now,
                FechaRegistro = DateTime.Now
            };

            await _repository.InsertarAsync(logDto);

            _logger.LogInformation(
                "Fallo permanente registrado: Entidad={Entidad}, ID={Id}",
                entityName, recordIdentifier);
        }
        catch (Exception logEx)
        {
            // No lanzar excepción para no interrumpir el flujo principal
            _logger.LogError(logEx,
                "Error al registrar fallo permanente: Entidad={Entidad}, ID={Id}",
                entityName, recordIdentifier);
        }
    }

    /// <summary>
    /// Registra una sincronización exitosa
    /// </summary>
    public async Task LogSuccessAsync(string entityName, Guid recordIdentifier)
    {
        try
        {
            _logger.LogDebug(
                "Registrando sincronización exitosa: Entidad={Entidad}, ID={Id}",
                entityName, recordIdentifier);

            var logDto = new SyncJobLogDto
            {
                IdSyncJobLog = Guid.NewGuid(),
                NombreEntidad = entityName,
                IdentificadorRegistro = recordIdentifier,
                Estado = "Sincronizado",
                MensajeError = string.Empty,
                FechaProcesamiento = DateTime.Now,
                FechaRegistro = DateTime.Now
            };

            await _repository.InsertarAsync(logDto);

            _logger.LogDebug(
                "Sincronización exitosa registrada: Entidad={Entidad}, ID={Id}",
                entityName, recordIdentifier);
        }
        catch (Exception logEx)
        {
            // No lanzar excepción para no interrumpir el flujo principal
            _logger.LogError(logEx,
                "Error al registrar sincronización exitosa: Entidad={Entidad}, ID={Id}",
                entityName, recordIdentifier);
        }
    }
}
