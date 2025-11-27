using Microsoft.AspNetCore.Mvc;
using SincronizadorPqfLegacy.Application.DTOs;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Application.Services;

namespace SincronizadorPqfLegacy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SincronizarCotizacionController : ControllerBase
    {
        #region Properties and Constructor

        private readonly ISincronizarCotizacion _sincronizarCotizacion;
        private readonly ISincronizacionMultipleService _sincronizacionMultipleService;


        public SincronizarCotizacionController(ISincronizarCotizacion sincronizarCotizacion, ISincronizacionMultipleService sincronizacionMultipleService)
        {
            _sincronizarCotizacion = sincronizarCotizacion;
            _sincronizacionMultipleService = sincronizacionMultipleService;
        }

        #endregion
        [HttpPost("sincronizarCotizacion")]
        public async Task<IActionResult> SincronizarCotizacion(Guid idCotizacion)
        {
            var report = await _sincronizarCotizacion.SincronizarCotizacion(idCotizacion);
            return Ok(report);
        }

        /// <summary>
        /// Sincroniza todas las cotizaciones pendientes de la tabla de control
        /// </summary>
        /// <returns>Resultado con contadores de éxito/error y lista de folios fallidos</returns>
        /// <response code="200">Sincronización completada (puede tener registros fallidos)</response>
        /// <response code="500">Error crítico durante la sincronización</response>
        [HttpPost("sincronizar-pendientes")]
        [ProducesResponseType(typeof(ResultadoSincronizacionMultipleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResultadoSincronizacionMultipleDto>> SincronizarPendientes()
        {
            var resultado = await _sincronizacionMultipleService.SincronizarPendientesAsync();
            // Retornar 200 OK incluso si hubo fallos individuales
            // El cliente puede revisar el resultado.Fallidos y resultado.FoliosFallidos
            return Ok(resultado);
        }
    }
}
