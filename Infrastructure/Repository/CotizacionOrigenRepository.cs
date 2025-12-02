using AutoMapper;
using Infrastructure.Persistence.PConnect.Contexts;
using Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;
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
    private readonly ProquifaDotNetContext _proquifaContext;
    private readonly PConnectProquifaDotNetContext _pconnectProquifaContext;
    private readonly IMapper _mapper;
    private readonly ILogger<CotizacionOrigenRepository> _logger;

    public CotizacionOrigenRepository(
        ProquifaDotNetContext context,
        PConnectProquifaDotNetContext pcpcontext,
        IMapper mapper,
        ILogger<CotizacionOrigenRepository> logger)
    {
        _proquifaContext = context;
        _pconnectProquifaContext = pcpcontext;
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
            var entidad = await _proquifaContext.vCotizacionesTransformadasETLs
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

    public async Task<List<Guid>> SincronizarCotizacionesPQF2Pendientes()
    {
        try
        {
            var cotizaionesPendientes = _pconnectProquifaContext.vETLCotizacionesPendietes
                .AsNoTracking()
                .Select(x=>x.IdCotCotizacion)
                .ToList();
            
            return cotizaionesPendientes ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones pendientes en ProquifaNet 2");
            throw;
        }
    }
}