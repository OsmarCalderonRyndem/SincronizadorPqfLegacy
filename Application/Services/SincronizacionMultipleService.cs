using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Application.DTOs;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Enums;
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

    public async Task<List<Guid>> SincronizarCotizacionesPendientes()
    {
        try
        {
            _logger.LogInformation("Iniciando sincronización de cotizaciones pendientes...");
            var pendientes = await _origenRepo.SincronizarCotizacionesPQF2Pendientes();
            
            _logger.LogInformation("Se encontraron {Count} cotizaciones pendientes. Iniciando procesamiento...", pendientes.Count);

            int contador = 0;
            int exitosos = 0;
            int fallidos = 0;

            foreach (var idCotizacion in pendientes)
            {
                contador++;
                try
                {
                    _logger.LogInformation("Procesando cotización {Actual}/{Total}: {Id}", contador, pendientes.Count, idCotizacion);
                    
                    // Ejecutar sincronización individual
                    await _sincronizarCotizacionService.SincronizarCotizacion(idCotizacion);
                    
                    // Registrar éxito
                    await _syncLogService.LogSuccessAsync("Cotizacion", idCotizacion.ToString());
                    
                    exitosos++;
                    _logger.LogInformation("✓ Cotización {Actual}/{Total} sincronizada exitosamente: {Id}", contador, pendientes.Count, idCotizacion);
                }
                catch (Exception ex)
                {
                    // Manejar error según clasificación
                    await ManejarErrorIndividual(ex, idCotizacion);
                    fallidos++;
                }
            }

            _logger.LogInformation("Sincronización completada. Total: {Total} | Exitosos: {Exitosos} | Fallidos: {Fallidos}",
                pendientes.Count, exitosos, fallidos);

            return pendientes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al sincronizar cotizaciones pendientes");
            throw;
        }
    }

    public async Task<ResultadoSincronizacionMultipleDto> SincronizarPendientesAsync()
    {
        // Inicializar resultado
        var resultado = new ResultadoSincronizacionMultipleDto
        {
            FechaInicio = DateTime.Now
        };

        try
        {
            _logger.LogInformation("INICIANDO SINCRONIZACIÓN MASIVA DE PENDIENTES");

            // PASO 1: Obtener registros pendientes
            var pendientes = await ObtenerPendientesAsync();
            resultado.TotalPendientes = pendientes.Count();

            _logger.LogInformation("Total de registros pendientes encontrados: {Total}",
                resultado.TotalPendientes);

            if (resultado.TotalPendientes == 0)
            {
                _logger.LogInformation("No hay registros pendientes para sincronizar");
                resultado.FechaFin = DateTime.Now;
                return resultado;
            }

            // PASO 2: Procesar cada registro pendiente
            await ProcesarPendientesAsync(pendientes, resultado);

            // Finalizar
            resultado.FechaFin = DateTime.Now;

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
            await _syncLogService.LogPermanentFailureAsync("Cotizacion", idCotizacion.ToString(), ex);
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
        ResultadoSincronizacionMultipleDto resultado)
    {
        _logger.LogInformation("Iniciando procesamiento de {Count} registros pendientes",
            pendientes.Count());

        int contador = 0;

        foreach (var cotizacionControl in pendientes)
        {
            contador++;

            _logger.LogInformation("Procesando registro {Actual}/{Total}: Folio={Folio}, CotizacionPQF={Id}",
                contador,
                pendientes.Count(),
                cotizacionControl.Folio,
                cotizacionControl.CotizacionPQF);

            try
            {
                // Sincronizar este registro individual
                await SincronizarRegistroIndividualAsync(cotizacionControl.CotizacionPQF);

                // Incrementar contador de exitosos
                resultado.Exitosos++;

                _logger.LogInformation("✓ Registro {Actual}/{Total} sincronizado exitosamente: {Folio}",
                    contador,
                    pendientes.Count(),
                    cotizacionControl.Folio);
            }
            catch (Exception ex)
            {
                // Incrementar contador de fallidos
                resultado.Fallidos++;

                // Agregar el folio a la lista de fallidos
                resultado.FoliosFallidos.Add(cotizacionControl.Folio);

                _logger.LogError(ex, "✗ Error al sincronizar registro {Actual}/{Total}: {Folio}",
                    contador,
                    pendientes.Count(),
                    cotizacionControl.Folio);

                // Manejar error según clasificación
                await ManejarErrorIndividual(ex, cotizacionControl.CotizacionPQF);
            }
        }

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
            await _syncLogService.LogSuccessAsync("Cotizacion", idCotizacion.ToString());
            _logger.LogDebug("Sincronización individual completada para: {Id}", idCotizacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en sincronización individual para: {Id}", idCotizacion);
            throw;
        }
    }
}