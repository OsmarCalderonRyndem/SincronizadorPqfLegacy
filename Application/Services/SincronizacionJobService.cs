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
        ISincronizacionMultipleService sincronizacionMultipleService,
        IExceptionClassifier exceptionClassifier,
        ISyncLogService syncLogService,
        ILogger<SincronizacionJobService> logger) : ISincronizacionJobService
    {
        private readonly ISincronizarCotizacion _sincronizarCotizacion = sincronizarCotizacion;
        private readonly ISincronizacionMultipleService _sincronizacionMultipleService = sincronizacionMultipleService;
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
        public async Task EjecutarSincronizacion(TipoProcesoEtl tipoProceso, Guid recordId, PerformContext? context = null)
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

        /// <summary>
        /// Job recurrente para sincronizar todas las cotizaciones pendientes.
        /// Este método se ejecuta automáticamente cada cierto intervalo configurado en Hangfire.
        /// </summary>
        /// <param name="context">Contexto de Hangfire para reportar progreso (opcional)</param>
        public async Task EjecutarSincronizacionPendientesRecurrente(PerformContext? context = null)
        {
            try
            {
                context?.WriteLine("═══════════════════════════════════════════════════════");
                context?.WriteLine("  SINCRONIZACIÓN AUTOMÁTICA DE PENDIENTES", ConsoleTextColor.Cyan);
                context?.WriteLine("═══════════════════════════════════════════════════════");
                context?.WriteLine($"Hora de ejecución: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                _logger.LogInformation("Iniciando sincronización automática de pendientes");

                var progressBar = context?.WriteProgressBar();
                progressBar?.SetValue(0);

                // Ejecutar la sincronización
                var resultado = await _sincronizacionMultipleService.SincronizarPendientesAsync();

                // Reportar resultados
                progressBar?.SetValue(100);

                context?.WriteLine("");
                context?.WriteLine("───────────────────────────────────────────────────────");
                context?.WriteLine("  RESULTADOS", ConsoleTextColor.White);
                context?.WriteLine("───────────────────────────────────────────────────────");
                context?.WriteLine($"Total pendientes: {resultado.TotalPendientes}");

                if (resultado.Exitosos > 0)
                {
                    context?.WriteLine($"✓ Exitosos: {resultado.Exitosos}", ConsoleTextColor.Green);
                }

                if (resultado.Fallidos > 0)
                {
                    context?.WriteLine($"✗ Fallidos: {resultado.Fallidos}", ConsoleTextColor.Yellow);
                    if (resultado.FoliosFallidos.Any())
                    {
                        context?.WriteLine($"  Folios fallidos: {string.Join(", ", resultado.FoliosFallidos)}");
                    }
                }

                context?.WriteLine($"Tiempo total: {resultado.TiempoTotal}");
                context?.WriteLine("═══════════════════════════════════════════════════════");

                _logger.LogInformation(
                    "Sincronización automática completada: Total={Total}, Exitosos={Exitosos}, Fallidos={Fallidos}",
                    resultado.TotalPendientes, resultado.Exitosos, resultado.Fallidos);
            }
            catch (Exception ex)
            {
                context?.WriteLine($"ERROR CRÍTICO: {ex.Message}", ConsoleTextColor.Red);
                _logger.LogError(ex, "Error crítico en sincronización automática de pendientes");
                throw;
            }
        }

        /// <summary>
        /// Job para procesar registros sin detonación inicial a ETL.
        /// Estos son procesos que no pudieron comunicarse con el API de ETL en su momento.
        /// </summary>
        /// <param name="context">Contexto de Hangfire para reportar progreso (opcional)</param>
        /// <remarks>
        /// TODO: Implementar la lógica específica según reglas de negocio.
        /// Por ahora es un placeholder para definir las reglas posteriormente.
        /// </remarks>
        public async Task EjecutarProcesosSinDetonacionInicial(PerformContext? context = null)
        {
            try
            {
                context?.WriteLine("═══════════════════════════════════════════════════════");
                context?.WriteLine("  PROCESOS SIN DETONACIÓN INICIAL", ConsoleTextColor.Cyan);
                context?.WriteLine("═══════════════════════════════════════════════════════");
                context?.WriteLine($"Hora de ejecución: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                context?.WriteLine("");
                context?.WriteLine("⚠️  PLACEHOLDER - Pendiente de implementación", ConsoleTextColor.Yellow);
                context?.WriteLine("");
                context?.WriteLine("Este job está reservado para procesar registros que:");
                context?.WriteLine("• No pudieron comunicarse con el API de ETL inicialmente");
                context?.WriteLine("• Requieren lógica especial de recuperación");
                context?.WriteLine("• Deben ser procesados según reglas de negocio específicas");
                context?.WriteLine("");
                context?.WriteLine("Las reglas de negocio se definirán posteriormente.");
                context?.WriteLine("═══════════════════════════════════════════════════════");

                _logger.LogInformation("Job de procesos sin detonación inicial ejecutado (placeholder)");


                var progressBar = context?.WriteProgressBar();
                progressBar?.SetValue(0);

                // Ejecutar la sincronización
                var resultado = await _sincronizacionMultipleService.SincronizarCotizacionesPendientes();

                // Reportar resultados
                progressBar?.SetValue(100);

                context?.WriteLine($"Total de cotizaciones sincronizadas: {resultado.Count}");
                _logger.LogInformation("Procesos sin detonación inicial completados. Total sincronizadas: {Count}", resultado.Count);

                
            }
            catch (Exception ex)
            {
                context?.WriteLine($"ERROR: {ex.Message}", ConsoleTextColor.Red);
                _logger.LogError(ex, "Error en job de procesos sin detonación inicial");
                throw;
            }
        }
    }
}
