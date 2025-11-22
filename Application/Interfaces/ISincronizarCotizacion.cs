using SincronizadorPqfLegacy.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SincronizadorPqfLegacy.Application.Interfaces
{
    public interface ISincronizarCotizacion
    {
        public Task<Guid> SincronizarCotizacion(Guid idCotizacion);
    }
}
