using Microsoft.AspNetCore.Mvc;
using SincronizadorPqfLegacy.Application.Interfaces;

namespace SincronizadorPqfLegacy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SincronizarCotizacionController : ControllerBase
    {
        #region Properties and Constructor

        private readonly ISincronizarCotizacion _sincronizarCotizacion;


        public SincronizarCotizacionController(ISincronizarCotizacion sincronizarCotizacion)
        {
            _sincronizarCotizacion = sincronizarCotizacion;
        }

        #endregion
        [HttpPost("sincronizarCotizacion")]
        public async Task<IActionResult> SincronizarCotizacion(Guid idCotizacion)
        {
            var report = await _sincronizarCotizacion.SincronizarCotizacion(idCotizacion);
            return Ok(report);
        }
    }
}
