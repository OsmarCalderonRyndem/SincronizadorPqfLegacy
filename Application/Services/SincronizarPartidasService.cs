using AutoMapper;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces;
using SincronizadorPqfLegacy.Domain.Interfaces.Repository;

namespace SincronizadorPqfLegacy.Application.Services;

public class SincronizarPartidasService : ISincronizarPartidasService
{
    private readonly IPartidaCotizacionOrigenRepository _origenRepo;
    private readonly IPartidaCotizacionLegacyRepository _legacyRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<SincronizarPartidasService> _logger;

    public SincronizarPartidasService(
        IPartidaCotizacionOrigenRepository origenRepo,
        IPartidaCotizacionLegacyRepository legacyRepo,
        IMapper mapper,
        ILogger<SincronizarPartidasService> logger)
    {
        _origenRepo = origenRepo;
        _legacyRepo = legacyRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task SincronizarPartidasAsync(Guid idCotizacion, int pkFolio)
    {
        try
        {
            _logger.LogInformation("Iniciando sincronizacion de partidas: IdCotizacion={Id}, PK_Folio={PK}",
                idCotizacion,
                pkFolio);

            var partidasOrigen = await ObtenerPartidasOrigenAsync(idCotizacion);

            _logger.LogInformation("Partidas encontradas en origen: {Count}", partidasOrigen.Count());

            if (!partidasOrigen.Any())
            {
                _logger.LogInformation("No hay partidas para sincronizar");
                return;
            }

            await ProcesarPartidasAsync(partidasOrigen, pkFolio);

            _logger.LogInformation("Sincronizacion de partidas completada: {Count} partidas procesadas",
                partidasOrigen.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sincronizar partidas: IdCotizacion={Id}, PK_Folio={PK}",
                idCotizacion,
                pkFolio);
            throw;
        }
    }

    private async Task<IEnumerable<PartidaCotizacionOrigenDto>> ObtenerPartidasOrigenAsync(Guid idCotizacion)
    {
        try
        {
            _logger.LogDebug("Obteniendo partidas de origen: {Id}", idCotizacion);

            var partidas = await _origenRepo.ObtenerPorCotizacionAsync(idCotizacion);

            _logger.LogDebug("Partidas obtenidas de origen: {Count}", partidas.Count());

            return partidas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas de origen: {Id}", idCotizacion);
            throw;
        }
    }

    private async Task ProcesarPartidasAsync(
        IEnumerable<PartidaCotizacionOrigenDto> partidasOrigen,
        int pkFolio)
    {
        _logger.LogDebug("Procesando {Count} partidas", partidasOrigen.Count());

        int contador = 0;

        foreach (var partidaOrigen in partidasOrigen)
        {
            contador++;

            try
            {
                _logger.LogDebug("Procesando partida {Actual}/{Total}: Partida={NumPartida}",
                    contador,
                    partidasOrigen.Count(),
                    partidaOrigen.Partida);

                await SincronizarPartidaIndividualAsync(partidaOrigen, pkFolio);

                _logger.LogDebug("Partida {Actual}/{Total} sincronizada exitosamente",
                    contador,
                    partidasOrigen.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar partida {Actual}/{Total}: Partida={NumPartida}",
                    contador,
                    partidasOrigen.Count(),
                    partidaOrigen.Partida);
                throw;
            }
        }

        _logger.LogDebug("Todas las partidas procesadas: {Count}", contador);
    }

    private async Task SincronizarPartidaIndividualAsync(
        PartidaCotizacionOrigenDto partidaOrigen,
        int pkFolio)
    {
        var partidaLegacy = _mapper.Map<PartidaCotizacionLegacyDto>(partidaOrigen);

        partidaLegacy.FK02_Cotiza = pkFolio;

        var partidaExistente = await BuscarPartidaExistenteAsync(pkFolio, partidaOrigen.Partida);

        if (partidaExistente != null)
        {
            _logger.LogDebug("Partida existente encontrada, actualizando: idPCotiza={Id}",
                partidaExistente.idPCotiza);

            partidaLegacy.idPCotiza = partidaExistente.idPCotiza;
            await _legacyRepo.ActualizarAsync(partidaLegacy);
        }
        else
        {
            _logger.LogDebug("Partida no existe, insertando nueva");

            await _legacyRepo.InsertarAsync(partidaLegacy);
        }
    }

    private async Task<PartidaCotizacionLegacyDto?> BuscarPartidaExistenteAsync(
        int pkFolio,
        int? numeroPartida)
    {
        if (!numeroPartida.HasValue)
        {
            return null;
        }

        var partidasExistentes = await _legacyRepo.ObtenerPorFolioAsync(pkFolio);

        return partidasExistentes.FirstOrDefault(p => p.Partida == numeroPartida);
    }
}