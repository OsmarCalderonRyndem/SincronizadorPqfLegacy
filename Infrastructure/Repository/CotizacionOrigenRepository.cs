using AutoMapper;
using Infrastructure.Persistence.ProquifaDotNet.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

namespace SincronizadorPqfLegacy.Infrastructure.Repositories;

/// <summary>
/// Repositorio para acceso a cotizaciones del sistema origen (ProquifaDotNet)
/// Implementa operaciones de solo lectura
/// </summary>
public class CotizacionOrigenRepository : ICotizacionOrigenRepository
{
    private readonly ProquifaDotNetContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CotizacionOrigenRepository> _logger;

    public CotizacionOrigenRepository(
        ProquifaDotNetContext context,
        IMapper mapper,
        ILogger<CotizacionOrigenRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una cotización transformada por su ID
    /// </summary>
    public async Task<CotizacionOrigenDto?> ObtenerPorIdAsync(Guid idCotizacion)
    {
        try
        {
            _logger.LogDebug("Buscando cotización origen: {Id}", idCotizacion);

            // Query a la vista
            var entidad = await _context.vCotizacionesTransformadasETLs
                .AsNoTracking() // Solo lectura, no tracking
                .Where(c => c.IdCotCotizacion == idCotizacion)
                .FirstOrDefaultAsync();

            if (entidad == null)
            {
                _logger.LogWarning("Cotización origen no encontrada: {Id}", idCotizacion);
                return null;
            }

            // Mapear Entity → DTO
            var dto = _mapper.Map<CotizacionOrigenDto>(entidad);

            _logger.LogDebug("Cotización origen encontrada: {Clave}", dto.Clave);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización origen: {Id}", idCotizacion);
            throw;
        }
    }
}