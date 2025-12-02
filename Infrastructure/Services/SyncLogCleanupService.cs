using Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SincronizadorPqfLegacy.Infrastructure.Services
{
    /// <summary>
    /// Servicio para limpiar logs antiguos de SyncJobLogs de manera configurable.
    /// </summary>
    public class SyncLogCleanupService
    {
        private readonly PConnectProquifaDotNetContext _context;
        private readonly ILogger<SyncLogCleanupService> _logger;
        private readonly SyncLogCleanupOptions _options;

        public SyncLogCleanupService(
            PConnectProquifaDotNetContext context,
            ILogger<SyncLogCleanupService> logger,
            IOptions<SyncLogCleanupOptions> options)
        {
            _context = context;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Elimina logs más antiguos que el número de días configurado.
        /// </summary>
        public async Task LimpiarLogsAntiguos()
        {
            try
            {
                var fechaLimite = DateTime.UtcNow.AddDays(-_options.DiasRetencion);

                _logger.LogInformation("Iniciando limpieza de logs anteriores a {FechaLimite}", fechaLimite);

                var logsAEliminar = await _context.SyncJobLogs
                    .Where(log => log.FechaProcesamiento < fechaLimite)
                    .ToListAsync();

                if (logsAEliminar.Any())
                {
                    _context.SyncJobLogs.RemoveRange(logsAEliminar);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Limpieza completada. {Count} logs eliminados.", logsAEliminar.Count);
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
}
