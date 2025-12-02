using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Domain.Interfaces;
using SincronizadorPqfLegacy.Domain.Models;

namespace SincronizadorPqfLegacy.Application.Services
{
    /// <summary>
    /// Servicio wrapper para ejecutar jobs de sincronización con manejo de errores clasificado.
    /// Este servicio es llamado por Hangfire y maneja la lógica de reintentos vs errores permanentes.
    /// </summary>
    public class SincronizacionJobService(
        ISincronizarCotizacion sincronizarCotizacion,
        IExceptionClassifier exceptionClassifier,
        ISyncLogService syncLogService,
        ILogger<SincronizacionJobService> logger)
    {
        private readonly ISincronizarCotizacion _sincronizarCotizacion = sincronizarCotizacion;
        private readonly IExceptionClassifier _exceptionClassifier = exceptionClassifier;
        private readonly ISyncLogService _syncLogService = syncLogService;
        private readonly ILogger<SincronizacionJobService> _logger = logger;

        /// <summary>
        /// Ejecuta la sincronización de una cotización con manejo de errores clasificado.
        /// </summary>
        /// <param name="idCotizacion">ID de la cotización a sincronizar</param>
        public async Task EjecutarSincronizacionCotizacion(Guid idCotizacion)
        {
            try
            {
                _logger.LogInformation("Iniciando sincronización de cotización {IdCotizacion}", idCotizacion);
                
                await _sincronizarCotizacion.SincronizarCotizacion(idCotizacion);
                
                // Log de éxito
                await _syncLogService.LogSuccessAsync("Cotizacion", idCotizacion.ToString());
                
                _logger.LogInformation("Sincronización completada exitosamente para cotización {IdCotizacion}", idCotizacion);
            }
            catch (Exception ex)
            {
                var category = _exceptionClassifier.Classify(ex);

                if (category == ErrorCategory.Transient)
                {
                    // Error transitorio: Lanzar excepción para que Hangfire reintente
                    _logger.LogWarning(ex, "Error transitorio en cotización {IdCotizacion}. Hangfire reintentará.", idCotizacion);
                    throw; // Hangfire captura esto y programa un reintento
                }
                else // Permanent
                {
                    // Error permanente: Loguear y NO reintentar
                    var errorMessage = $"Error permanente en cotización {idCotizacion}: {ex.Message}";
                    
                    _logger.LogWarning(ex, "Error permanente en cotización {IdCotizacion}. No se reintentará.", idCotizacion);
                    await _syncLogService.LogPermanentFailureAsync("Cotizacion", idCotizacion.ToString(), ex);
                    
                    // Escribir en consola para que aparezca en el Dashboard de Hangfire
                    Console.WriteLine($"⚠️ {errorMessage}");
                    
                    // NO lanzar excepción: el job terminará como "Succeeded",
                    // pero el mensaje aparecerá en el log del dashboard
                }
            }
        }
    }
}
