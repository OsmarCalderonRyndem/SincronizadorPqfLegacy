using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

/// <summary>
/// Repositorio para acceso a cotizaciones del sistema legacy (PConnect)
/// </summary>
public interface ICotizacionLegacyRepository
{
    /// <summary>
    /// Obtiene una cotización por su clave
    /// </summary>
    /// <param name="clave">Clave de la cotización</param>
    /// <returns>Cotización DTO o null si no existe</returns>
    Task<CotizacionLegacyDto?> ObtenerPorClaveAsync(string clave);

    /// <summary>
    /// Obtiene una cotización por su PK_Folio
    /// </summary>
    /// <param name="pkFolio">Primary key</param>
    /// <returns>Cotización DTO o null si no existe</returns>
    Task<CotizacionLegacyDto?> ObtenerPorPKAsync(int pkFolio);

    /// <summary>
    /// Inserta una nueva cotización en legacy
    /// </summary>
    /// <param name="cotizacion">Cotización DTO a insertar</param>
    /// <returns>Cotización DTO insertada con PK_Folio asignado</returns>
    Task<CotizacionLegacyDto> InsertarAsync(CotizacionLegacyDto cotizacion);

    /// <summary>
    /// Actualiza una cotización existente
    /// </summary>
    /// <param name="cotizacion">Cotización DTO con datos actualizados</param>
    /// <returns>Cotización DTO actualizada</returns>
    Task<CotizacionLegacyDto> ActualizarAsync(CotizacionLegacyDto cotizacion);

}