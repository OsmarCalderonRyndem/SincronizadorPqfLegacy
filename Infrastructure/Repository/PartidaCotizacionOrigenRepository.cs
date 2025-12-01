using AutoMapper;
using Infrastructure.Persistence.ProquifaDotNet.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces.Repository;

namespace SincronizadorPqfLegacy.Infrastructure.Repository;

public class PartidaCotizacionOrigenRepository : IPartidaCotizacionOrigenRepository
{
    private readonly ProquifaDotNetContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PartidaCotizacionOrigenRepository> _logger;

    public PartidaCotizacionOrigenRepository(
        ProquifaDotNetContext context,
        IMapper mapper,
        ILogger<PartidaCotizacionOrigenRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PartidaCotizacionOrigenDto>> ObtenerPorCotizacionAsync(Guid idCotizacion)
    {
        try
        {
            _logger.LogDebug("Obteniendo partidas de cotizacion: {Id}", idCotizacion);

            var entidades = await _context.vPartidasCotizacionTransformadasETLs
                .AsNoTracking()
                .Where(p => p.IdCotCotizacion == idCotizacion)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<PartidaCotizacionOrigenDto>>(entidades);

            _logger.LogDebug("Partidas obtenidas: {Count}", dtos.Count());

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas de cotizacion: {Id}", idCotizacion);
            throw;
        }
    }
}