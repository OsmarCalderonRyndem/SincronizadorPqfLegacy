namespace SincronizadorPqfLegacy.Application.Interfaces;

public interface ISincronizarPartidasService
{
    Task SincronizarPartidasAsync(Guid idCotizacion, int pkFolio);
}