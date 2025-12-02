using Hangfire.Server;
using SincronizadorPqfLegacy.Application.DTOs;

namespace SincronizadorPqfLegacy.Application.Interfaces;

/// <summary>
/// Servicio para sincronización masiva de cotizaciones
/// </summary>
public interface ISincronizacionMultipleService
{
    /// <summary>
    /// Sincroniza todas las cotizaciones pendientes de la tabla de control
    /// </summary>
    /// <returns>Resultado con contadores de éxito/error</returns>
    Task<ResultadoSincronizacionMultipleDto> SincronizarPendientesAsync(PerformContext? context = null);

    Task<List<Guid>> SincronizarCotizacionesPendientes(PerformContext? context = null);
}