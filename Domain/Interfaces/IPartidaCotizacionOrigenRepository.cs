using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

public interface IPartidaCotizacionOrigenRepository
{
    Task<IEnumerable<PartidaCotizacionOrigenDto>> ObtenerPorCotizacionAsync(Guid idCotizacion);
}