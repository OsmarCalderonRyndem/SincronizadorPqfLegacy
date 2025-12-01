using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Domain.Interfaces.Repository;

public interface IPartidaCotizacionOrigenRepository
{
    Task<IEnumerable<PartidaCotizacionOrigenDto>> ObtenerPorCotizacionAsync(Guid idCotizacion);
}