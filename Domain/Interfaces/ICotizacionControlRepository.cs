using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

/// <summary>
/// Repositorio para la tabla de control de sincronización
/// </summary>
public interface ICotizacionControlRepository
{
    /// <summary>
    /// Obtiene un registro de control por ID de cotización PQF
    /// </summary>
    /// <param name="idCotizacion">ID de la cotización en ProquifaDotNet</param>
    /// <returns>Registro DTO de control o null si no existe</returns>
    Task<CotizacionControlDto?> ObtenerPorIdAsync(Guid idCotizacion);

    /// <summary>
    /// Inserta un nuevo registro de control
    /// </summary>
    /// <param name="cotizacion">Registro DTO a insertar</param>
    /// <returns>Registro DTO insertado</returns>
    Task<CotizacionControlDto> InsertarAsync(CotizacionControlDto cotizacion);

    /// <summary>
    /// Actualiza un registro de control existente
    /// </summary>
    /// <param name="cotizacion">Registro DTO con datos actualizados</param>
    /// <returns>Registro DTO actualizado</returns>
    Task<CotizacionControlDto> ActualizarAsync(CotizacionControlDto cotizacion);

    /// <summary>
    /// Obtiene cotizaciones pendientes de sincronizar
    /// </summary>
    /// <returns>Lista de registros DTO pendientes</returns>
    Task<IEnumerable<CotizacionControlDto>> ObtenerPendientesSincronizacionAsync();

}