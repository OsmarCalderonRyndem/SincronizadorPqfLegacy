using AutoMapper;
using Infrastructure.Persistence.PConnect.Contexts;
using Infrastructure.Persistence.PConnect.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces;


namespace SincronizadorPqfLegacy.Infrastructure.Repository;

public class PartidaCotizacionLegacyRepository : IPartidaCotizacionLegacyRepository
{
    private readonly PConnectContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PartidaCotizacionLegacyRepository> _logger;

    public PartidaCotizacionLegacyRepository(
        PConnectContext context,
        IMapper mapper,
        ILogger<PartidaCotizacionLegacyRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PartidaCotizacionLegacyDto>> ObtenerPorFolioAsync(int pkFolio)
    {
        try
        {
            _logger.LogDebug("Obteniendo partidas de folio legacy: {Folio}", pkFolio);

            var entidades = await _context.PCotizas
                .AsNoTracking()
                .Where(p => p.FK02_Cotiza == pkFolio)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<PartidaCotizacionLegacyDto>>(entidades);

            _logger.LogDebug("Partidas legacy obtenidas: {Count}", dtos.Count());

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas de folio: {Folio}", pkFolio);
            throw;
        }
    }

    public async Task<PartidaCotizacionLegacyDto> InsertarAsync(PartidaCotizacionLegacyDto partida)
    {
        try
        {
            _logger.LogDebug("Insertando partida: Folio={Folio}, Partida={Partida}",
                partida.Folio,
                partida.Partida);

            var entidad = _mapper.Map<PCotiza>(partida);

            await _context.PCotizas.AddAsync(entidad);
            await _context.SaveChangesAsync();

            var dtoInsertado = _mapper.Map<PartidaCotizacionLegacyDto>(entidad);

            _logger.LogDebug("Partida insertada: idPCotiza={Id}", dtoInsertado.idPCotiza);

            return dtoInsertado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar partida: Folio={Folio}, Partida={Partida}",
                partida.Folio,
                partida.Partida);
            throw;
        }
    }

    public async Task<PartidaCotizacionLegacyDto> ActualizarAsync(PartidaCotizacionLegacyDto partida)
    {
        try
        {
            _logger.LogDebug("Actualizando partida: idPCotiza={Id}", partida.idPCotiza);

            var entidadExistente = await _context.PCotizas
                .Where(p => p.idPCotiza == partida.idPCotiza)
                .FirstOrDefaultAsync();

            if (entidadExistente == null)
            {
                throw new InvalidOperationException(
                    $"No se puede actualizar. Partida con idPCotiza {partida.idPCotiza} no existe.");
            }

            _mapper.Map(partida, entidadExistente);

            _context.PCotizas.Update(entidadExistente);
            await _context.SaveChangesAsync();

            var dtoActualizado = _mapper.Map<PartidaCotizacionLegacyDto>(entidadExistente);

            _logger.LogDebug("Partida actualizada: idPCotiza={Id}", dtoActualizado.idPCotiza);

            return dtoActualizado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar partida: idPCotiza={Id}", partida.idPCotiza);
            throw;
        }
    }
}