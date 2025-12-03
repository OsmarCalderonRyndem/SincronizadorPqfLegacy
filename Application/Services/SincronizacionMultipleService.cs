using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Application.DTOs;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;
using SincronizadorPqfLegacy.Domain.Models;

namespace SincronizadorPqfLegacy.Application.Services;

/// <summary>
/// Implementación del servicio de sincronización masiva
/// </summary>
public class SincronizacionMultipleService : ISincronizacionMultipleService
{
    private readonly ICotizacionOrigenRepository _origenRepo;
    private readonly ICotizacionLegacyRepository _legacyRepo;
    private readonly ICotizacionControlRepository _controlRepo;
    private readonly ISincronizarCotizacion _sincronizarCotizacionService;
    private readonly IExceptionClassifier _exceptionClassifier;
    private readonly ISyncLogService _syncLogService;
    private readonly ILogger<SincronizacionMultipleService> _logger;

    public SincronizacionMultipleService(
        ICotizacionOrigenRepository origenRepo,
        ICotizacionLegacyRepository legacyRepo,
        ICotizacionControlRepository controlRepo,
        ISincronizarCotizacion sincronizarCotizacionService,
        IExceptionClassifier exceptionClassifier,
        ISyncLogService syncLogService,
        ILogger<SincronizacionMultipleService> logger)
    {
        _origenRepo = origenRepo;
        _legacyRepo = legacyRepo;
        _controlRepo = controlRepo;
        _sincronizarCotizacionService = sincronizarCotizacionService;
        _exceptionClassifier = exceptionClassifier;
        _syncLogService = syncLogService;
        _logger = logger;
    }

    public async Task<List<Guid>> SincronizarCotizacionesPendientes(PerformContext? context = null)
    {
        try
        {
            context?.WriteLine("Iniciando sincronización de cotizaciones pendientes...");
            _logger.LogInformation("Iniciando sincronización de cotizaciones pendientes...");

            var pendientes = await _origenRepo.SincronizarCotizacionesPQF2Pendientes();

            context?.WriteLine($"Se encontraron {pendientes.Count} cotizaciones pendientes. Iniciando procesamiento...");
            _logger.LogInformation("Se encontraron {Count} cotizaciones pendientes. Iniciando procesamiento...", pendientes.Count);

            // Crear barra de progreso
            var progressBar = context?.WriteProgressBar();
            progressBar?.SetValue(0);

            int contador = 0;
            int exitosos = 0;
            int fallidos = 0;
            int totalPendientes = pendientes.Count;

            foreach (var idCotizacion in pendientes)
            {
                contador++;
                try
                {
                    context?.WriteLine($"[{contador}/{totalPendientes}] Procesando cotización: {idCotizacion}");
                    _logger.LogInformation("Procesando cotización {Actual}/{Total}: {Id}", contador, totalPendientes, idCotizacion);

                    // Ejecutar sincronización individual
                    await _sincronizarCotizacionService.SincronizarCotizacion(idCotizacion);

                    // Registrar éxito
                    await _syncLogService.LogSuccessAsync("Cotizacion", idCotizacion);

                    exitosos++;

                    // Actualizar barra de progreso
                    var progreso = (int)((double)contador / totalPendientes * 100);
                    progressBar?.SetValue(progreso);

                    context?.WriteLine($"✓ [{contador}/{totalPendientes}] Cotización sincronizada exitosamente: {idCotizacion}", ConsoleTextColor.Green);
                    _logger.LogInformation("✓ Cotización {Actual}/{Total} sincronizada exitosamente: {Id}", contador, totalPendientes, idCotizacion);
                }
                catch (Exception ex)
                {
                    // Actualizar barra de progreso incluso en error
                    var progreso = (int)((double)contador / totalPendientes * 100);
                    progressBar?.SetValue(progreso);

                    context?.WriteLine($"✗ [{contador}/{totalPendientes}] Error al sincronizar cotización: {idCotizacion} - {ex.Message}", ConsoleTextColor.Red);

                    // Manejar error según clasificación
                    await ManejarErrorIndividual(ex, idCotizacion);
                    fallidos++;
                }
            }

            // Asegurar que el progreso esté al 100%
            progressBar?.SetValue(100);

            context?.WriteLine("");
            context?.WriteLine("═══════════════════════════════════════════════════════");
            context?.WriteLine($"Sincronización completada. Total: {totalPendientes} | Exitosos: {exitosos} | Fallidos: {fallidos}");
            context?.WriteLine("═══════════════════════════════════════════════════════");

            _logger.LogInformation("Sincronización completada. Total: {Total} | Exitosos: {Exitosos} | Fallidos: {Fallidos}",
                totalPendientes, exitosos, fallidos);

            return pendientes;
        }
        catch (Exception ex)
        {
            context?.WriteLine($"ERROR CRÍTICO: {ex.Message}", ConsoleTextColor.Red);
            _logger.LogError(ex, "Error crítico al sincronizar cotizaciones pendientes");
            throw;
        }
    }

    public async Task<ResultadoSincronizacionMultipleDto> SincronizarPendientesAsync(PerformContext? context = null)
    {
        // Inicializar resultado
        var resultado = new ResultadoSincronizacionMultipleDto
        {
            FechaInicio = DateTime.Now
        };

        try
        {
            context?.WriteLine("Iniciando sincronización masiva de pendientes...");
            _logger.LogInformation("INICIANDO SINCRONIZACIÓN MASIVA DE PENDIENTES");

            // PASO 1: Obtener registros pendientes
            context?.WriteLine("Consultando registros pendientes...");
            var pendientes = await ObtenerPendientesAsync();
            resultado.TotalPendientes = pendientes.Count();

            context?.WriteLine($"Total de registros pendientes encontrados: {resultado.TotalPendientes}");
            _logger.LogInformation("Total de registros pendientes encontrados: {Total}",
                resultado.TotalPendientes);

            if (resultado.TotalPendientes == 0)
            {
                context?.WriteLine("No hay registros pendientes para sincronizar", ConsoleTextColor.Yellow);
                _logger.LogInformation("No hay registros pendientes para sincronizar");
                resultado.FechaFin = DateTime.Now;
                return resultado;
            }

            // PASO 2: Procesar cada registro pendiente
            await ProcesarPendientesAsync(pendientes, resultado, context);

            // Finalizar
            resultado.FechaFin = DateTime.Now;

            context?.WriteLine("");
            context?.WriteLine("SINCRONIZACIÓN MASIVA COMPLETADA", ConsoleTextColor.Cyan);
            _logger.LogInformation("SINCRONIZACIÓN MASIVA COMPLETADA");
            _logger.LogInformation("Total: {Total} | Exitosos: {Exitosos} | Fallidos: {Fallidos} | Tiempo: {Tiempo}",
                resultado.TotalPendientes,
                resultado.Exitosos,
                resultado.Fallidos,
                resultado.TiempoTotal);

            return resultado;
        }
        catch (Exception ex)
        {
            context?.WriteLine($"ERROR CRÍTICO: {ex.Message}", ConsoleTextColor.Red);
            _logger.LogError(ex, "Error crítico en sincronización masiva");
            resultado.FechaFin = DateTime.Now;
            throw;
        }
    }

    /// <summary>
    /// Maneja errores individuales según clasificación (transient vs permanent).
    /// Similar a ManejarError en SincronizacionJobService pero para procesamiento batch.
    /// </summary>
    private async Task ManejarErrorIndividual(Exception ex, Guid idCotizacion)
    {
        var category = _exceptionClassifier.Classify(ex);

        if (category == ErrorCategory.Transient)
        {
            // Error transitorio: Loguear como warning pero continuar con el siguiente
            _logger.LogWarning(ex, "Error transitorio en cotización {Id}. Se continuará con el siguiente registro.", idCotizacion);
            // En procesamiento batch, NO relanzamos para continuar con los demás
        }
        else // Permanent
        {
            // Error permanente: Registrar en la tabla de logs
            _logger.LogWarning(ex, "Error permanente en cotización {Id}. Se registrará y continuará.", idCotizacion);
            await _syncLogService.LogPermanentFailureAsync("Cotizacion", idCotizacion, ex);
        }
    }

    /// <summary>
    /// Obtiene todos los registros pendientes de sincronización
    /// </summary>
    private async Task<IEnumerable<CotizacionControlDto>> ObtenerPendientesAsync()
    {
        try
        {
            _logger.LogDebug("Consultando tabla de control para obtener pendientes...");

            // Llamar al repositorio
            var pendientes = await _controlRepo.ObtenerPendientesSincronizacionAsync();

            _logger.LogDebug("Registros pendientes obtenidos: {Count}", pendientes.Count());

            return pendientes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registros pendientes de la tabla de control");
            throw;
        }
    }

    /// <summary>
    /// Procesa cada registro pendiente uno por uno
    /// </summary>
    private async Task ProcesarPendientesAsync(
        IEnumerable<CotizacionControlDto> pendientes,
        ResultadoSincronizacionMultipleDto resultado,
        PerformContext? context = null)
    {
        context?.WriteLine($"Iniciando procesamiento de {pendientes.Count()} registros pendientes...");
        _logger.LogInformation("Iniciando procesamiento de {Count} registros pendientes",
            pendientes.Count());

        // Crear barra de progreso
        var progressBar = context?.WriteProgressBar();
        progressBar?.SetValue(0);

        int contador = 0;
        int totalPendientes = pendientes.Count();

        foreach (var cotizacionControl in pendientes)
        {
            contador++;

            context?.WriteLine($"[{contador}/{totalPendientes}] Procesando: Folio={cotizacionControl.Folio}, ID={cotizacionControl.CotizacionPQF}");
            _logger.LogInformation("Procesando registro {Actual}/{Total}: Folio={Folio}, CotizacionPQF={Id}",
                contador,
                totalPendientes,
                cotizacionControl.Folio,
                cotizacionControl.CotizacionPQF);

            try
            {
                // Sincronizar este registro individual
                await SincronizarRegistroIndividualAsync(cotizacionControl.CotizacionPQF);

                // Incrementar contador de exitosos
                resultado.Exitosos++;

                // Actualizar barra de progreso
                var progreso = (int)((double)contador / totalPendientes * 100);
                progressBar?.SetValue(progreso);

                context?.WriteLine($"✓ [{contador}/{totalPendientes}] Sincronizado exitosamente: {cotizacionControl.Folio}", ConsoleTextColor.Green);
                _logger.LogInformation("✓ Registro {Actual}/{Total} sincronizado exitosamente: {Folio}",
                    contador,
                    totalPendientes,
                    cotizacionControl.Folio);
            }
            catch (Exception ex)
            {
                // Incrementar contador de fallidos
                resultado.Fallidos++;

                // Agregar el folio a la lista de fallidos
                resultado.FoliosFallidos.Add(cotizacionControl.Folio);

                // Actualizar barra de progreso incluso en error
                var progreso = (int)((double)contador / totalPendientes * 100);
                progressBar?.SetValue(progreso);

                context?.WriteLine($"✗ [{contador}/{totalPendientes}] Error al sincronizar: {cotizacionControl.Folio} - {ex.Message}", ConsoleTextColor.Red);
                _logger.LogError(ex, "✗ Error al sincronizar registro {Actual}/{Total}: {Folio}",
                    contador,
                    totalPendientes,
                    cotizacionControl.Folio);

                // Manejar error según clasificación
                await ManejarErrorIndividual(ex, cotizacionControl.CotizacionPQF);
            }
        }

        // Asegurar que el progreso esté al 100%
        progressBar?.SetValue(100);

        context?.WriteLine("");
        context?.WriteLine($"Procesamiento completado: {resultado.Exitosos} exitosos, {resultado.Fallidos} fallidos");
        _logger.LogInformation("Procesamiento completado: {Exitosos} exitosos, {Fallidos} fallidos",
            resultado.Exitosos,
            resultado.Fallidos);
    }

    /// <summary>
    /// Sincroniza un registro individual de cotización
    /// Reutiliza el servicio de sincronización individual existente
    /// </summary>
    private async Task SincronizarRegistroIndividualAsync(Guid idCotizacion)
    {
        try
        {
            _logger.LogDebug("Iniciando sincronización individual para: {Id}", idCotizacion);

            // Llamar al servicio de sincronización individual
            //await _sincronizacionJobService.EjecutarSincronizacion(TipoProcesoEtl.Cotizacion, idCotizacion);
            await _sincronizarCotizacionService.SincronizarCotizacion(idCotizacion);
            await _syncLogService.LogSuccessAsync("Cotizacion", idCotizacion);
            _logger.LogDebug("Sincronización individual completada para: {Id}", idCotizacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en sincronización individual para: {Id}", idCotizacion);
            throw;
        }
    }
}