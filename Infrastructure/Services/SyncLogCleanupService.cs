using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

namespace SincronizadorPqfLegacy.Infrastructure.Services;

/// <summary>
/// Servicio para limpiar logs antiguos de SyncJobLogs de manera configurable.
/// Usa el repositorio con DTOs.
/// </summary>
public class SyncLogCleanupService
{
    private readonly ISyncJobLogRepository _repository;
    private readonly ILogger<SyncLogCleanupService> _logger;
    private readonly SyncLogCleanupOptions _options;

    public SyncLogCleanupService(
        ISyncJobLogRepository repository,
        ILogger<SyncLogCleanupService> logger,
        IOptions<SyncLogCleanupOptions> options)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Elimina logs más antiguos que el número de días configurado.
    /// </summary>
    public async Task LimpiarLogsAntiguos()
    {
        try
        {
            _logger.LogInformation(
                "Iniciando limpieza de logs con retención de {Dias} días",
                _options.DiasRetencion);

            var cantidadEliminados = await _repository.EliminarAntiguosAsync(_options.DiasRetencion);

            if (cantidadEliminados > 0)
            {
                _logger.LogInformation(
                    "Limpieza completada. {Count} logs eliminados.",
                    cantidadEliminados);
            }
            else
            {
                _logger.LogInformation("No hay logs antiguos para eliminar.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la limpieza de logs antiguos");
            throw;
        }
    }
}

/// <summary>
/// Opciones de configuración para la limpieza de logs.
/// </summary>
public class SyncLogCleanupOptions
{
    public const string SectionName = "SyncLogCleanup";

    /// <summary>
    /// Número de días de retención de logs. Por defecto: 30 días.
    /// </summary>
    public int DiasRetencion { get; set; } = 30;

    /// <summary>
    /// Indica si el proceso de limpieza de logs está habilitado. Por defecto: false.
    /// </summary>
    public bool Habilitado { get; set; } = false;

    /// <summary>
    /// Expresión Cron para la frecuencia de ejecución. Por defecto: diario a las 2 AM.
    /// </summary>
    public string CronExpression { get; set; } = "0 2 * * *"; // Diario a las 2 AM
}
