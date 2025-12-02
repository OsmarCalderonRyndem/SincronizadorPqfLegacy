using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Domain.Interfaces;
using SincronizadorPqfLegacy.Domain.Models;
using SincronizadorPqfLegacy.Domain.Enums;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;

namespace SincronizadorPqfLegacy.Application.Services
{
    /// <summary>
    /// Servicio wrapper genérico para ejecutar jobs de sincronización ETL con manejo de errores clasificado.
    /// Este servicio es llamado por Hangfire y maneja la lógica de reintentos vs errores permanentes.
    /// Soporta múltiples tipos de procesos ETL mediante un switch basado en TipoProcesoEtl.
    /// </summary>
    public class SincronizacionJobService(
        ISincronizarCotizacion sincronizarCotizacion,
        ISincronizarPartidasService sincronizarPartidas,
        IExceptionClassifier exceptionClassifier,
        ISyncLogService syncLogService,
        ILogger<SincronizacionJobService> logger)
    {
        private readonly ISincronizarCotizacion _sincronizarCotizacion = sincronizarCotizacion;
        private readonly ISincronizarPartidasService _sincronizarPartidas = sincronizarPartidas;
        private readonly IExceptionClassifier _exceptionClassifier = exceptionClassifier;
        private readonly ISyncLogService _syncLogService = syncLogService;
        private readonly ILogger<SincronizacionJobService> _logger = logger;

        /// <summary>
        /// Ejecuta un proceso de sincronización ETL basado en el tipo de proceso.
        /// </summary>
        /// <param name="tipoProceso">Tipo de proceso ETL (enum)</param>
        /// <param name="recordId">ID del registro principal a sincronizar</param>
        /// <param name="parametrosAdicionales">Parámetros adicionales en formato JSON (opcional, para casos como partidas que necesitan pkFolio)</param>
        /// <param name="context">Contexto de Hangfire para reportar progreso (opcional)</param>
        public async Task EjecutarSincronizacion(TipoProcesoEtl tipoProceso, Guid recordId, string? parametrosAdicionales = null, PerformContext? context = null)
        {
            var nombreProceso = tipoProceso.ToString();

            try
            {
                context?.WriteLine($"Iniciando proceso ETL: {nombreProceso} para ID: {recordId}");
                _logger.LogInformation("Iniciando proceso ETL: {TipoProceso} para ID: {RecordId}", nombreProceso, recordId);

                // Reportar progreso si el contexto está disponible
                var progressBar = context?.WriteProgressBar();

                // Fase 1: Preparación
                progressBar?.SetValue(10);
                context?.WriteLine("Preparando ejecución...");

                switch (tipoProceso)
                {
                    case TipoProcesoEtl.Cotizacion:
                        await EjecutarSincronizacionCotizacionInterno(recordId, context, progressBar);
                        break;

                    case TipoProcesoEtl.Partida:
                        await EjecutarSincronizacionPartidaInterno(recordId, parametrosAdicionales, context, progressBar);
                        break;

                    default:
                        throw new ArgumentException($"Proceso ETL no soportado: {tipoProceso}");
                }

                _logger.LogInformation("Proceso ETL {TipoProceso} completado exitosamente para ID {RecordId}", nombreProceso, recordId);
            }
            catch (Exception ex)
            {
                await ManejarError(ex, nombreProceso, recordId, context);
            }
        }

        private async Task EjecutarSincronizacionCotizacionInterno(Guid idCotizacion, PerformContext? context, IProgressBar? progressBar)
        {
            context?.WriteLine("Preparando datos de cotización...");

            // Fase 2: Ejecución de sincronización
            progressBar?.SetValue(40);
            context?.WriteLine("Ejecutando sincronización de cotización...");

            await _sincronizarCotizacion.SincronizarCotizacion(idCotizacion);

            // Fase 3: Finalización
            progressBar?.SetValue(80);
            context?.WriteLine("Guardando resultados...");

            await _syncLogService.LogSuccessAsync("Cotizacion", idCotizacion.ToString());

            progressBar?.SetValue(100);
            context?.WriteLine("Sincronización de cotización completada exitosamente", ConsoleTextColor.Green);
        }

        private async Task EjecutarSincronizacionPartidaInterno(Guid idCotizacion, string? parametrosAdicionales, PerformContext? context, IProgressBar? progressBar)
        {
            context?.WriteLine("Preparando datos de partidas...");

            // Extraer pkFolio de los parámetros adicionales (puede ser JSON)
            int pkFolio = 0;
            if (!string.IsNullOrEmpty(parametrosAdicionales))
            {
                try
                {
                    var parametros = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(parametrosAdicionales);
                    if (parametros != null && parametros.ContainsKey("pkFolio"))
                    {
                        pkFolio = Convert.ToInt32(parametros["pkFolio"]);
                    }
                }
                catch
                {
                    // Si falla el parseo JSON, intentar como entero directo
                    int.TryParse(parametrosAdicionales, out pkFolio);
                }
            }

            // Fase 2: Ejecución de sincronización
            progressBar?.SetValue(40);
            context?.WriteLine($"Ejecutando sincronización de partidas (pkFolio: {pkFolio})...");

            await _sincronizarPartidas.SincronizarPartidasAsync(idCotizacion, pkFolio);

            // Fase 3: Finalización
            progressBar?.SetValue(80);
            context?.WriteLine("Guardando resultados...");

            await _syncLogService.LogSuccessAsync("Partida", $"{idCotizacion}|{pkFolio}");

            progressBar?.SetValue(100);
            context?.WriteLine("Sincronización de partidas completada exitosamente", ConsoleTextColor.Green);
        }

        private async Task ManejarError(Exception ex, string processName, Guid recordId, PerformContext? context)
        {
            var category = _exceptionClassifier.Classify(ex);

            if (category == ErrorCategory.Transient)
            {
                // Error transitorio: Lanzar excepción para que Hangfire reintente
                context?.WriteLine($"Error transitorio: {ex.Message}", ConsoleTextColor.Yellow);
                _logger.LogWarning(ex, "Error transitorio en proceso {ProcessName} ID {RecordId}. Hangfire reintentará.", processName, recordId);
                throw ex; // Relanzar la excepción para que Hangfire reintente
            }
            else // Permanent
            {
                // Error permanente: Loguear y NO reintentar
                var errorMessage = $"Error permanente en proceso {processName} ID {recordId}: {ex.Message}";

                context?.WriteLine($"{errorMessage}", ConsoleTextColor.Red);
                _logger.LogWarning(ex, "Error permanente en proceso {ProcessName} ID {RecordId}. No se reintentará.", processName, recordId);
                await _syncLogService.LogPermanentFailureAsync(processName, recordId.ToString(), ex);

                // Escribir en consola estándar también para compatibilidad
                Console.WriteLine($"{errorMessage}");

                // NO lanzar excepción: el job terminará como "Succeeded",
                // pero el mensaje aparecerá en el log del dashboard
            }
        }
    }
}
