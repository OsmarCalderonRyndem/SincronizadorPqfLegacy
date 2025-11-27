using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Domain.Interfaces;

public interface IPartidaCotizacionLegacyRepository
{
    Task<IEnumerable<PartidaCotizacionLegacyDto>> ObtenerPorFolioAsync(int pkFolio);

    Task<PartidaCotizacionLegacyDto> InsertarAsync(PartidaCotizacionLegacyDto partida);

    Task<PartidaCotizacionLegacyDto> ActualizarAsync(PartidaCotizacionLegacyDto partida);
}