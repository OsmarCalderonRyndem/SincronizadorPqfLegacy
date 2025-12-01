using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

/// <summary>
/// Repositorio para acceso a cotizaciones del sistema origen (ProquifaDotNet)
/// </summary>
public interface ICotizacionOrigenRepository
{
    /// <summary>
    /// Obtiene una cotización transformada por su ID
    /// </summary>
    /// <param name="idCotizacion">ID de la cotización</param>
    /// <returns>Cotización DTO o null si no existe</returns>
    Task<CotizacionOrigenDto?> ObtenerPorIdAsync(Guid idCotizacion);

}