namespace SincronizadorPqfLegacy.Application.Interfaces
{
    public interface ISincronizarCotizacion
    {
        public Task<Guid> SincronizarCotizacion(Guid idCotizacion);
    }
}
