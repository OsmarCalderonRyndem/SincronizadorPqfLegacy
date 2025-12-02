using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SincronizadorPqfLegacy.Application.DTOs;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Application.Services;

namespace SincronizadorPqfLegacy.API.Controllers
{
    /// <summary>
    /// Provides API endpoints for synchronizing individual and multiple quotations in the background. This controller
    /// enables clients to enqueue synchronization jobs and trigger bulk synchronization of pending quotations.
    /// </summary>
    /// <remarks>Endpoints in this controller use background processing to offload synchronization tasks,
    /// improving responsiveness for clients. Jobs are managed via Hangfire, and results include status information for
    /// tracking success and failures. All endpoints are intended for use by authorized clients requiring quotation data
    /// consistency.</remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class SincronizarCotizacionController : ControllerBase
    {
        #region Properties and Constructor

        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ISincronizacionMultipleService _sincronizacionMultipleService;
        private readonly ILogger<SincronizarCotizacionController> _logger;

        public SincronizarCotizacionController(
            IBackgroundJobClient backgroundJobClient,
            ISincronizacionMultipleService sincronizacionMultipleService,
            ILogger<SincronizarCotizacionController> logger)
        {
            _backgroundJobClient = backgroundJobClient;
            _sincronizacionMultipleService = sincronizacionMultipleService;
            _logger = logger;
        }

        #endregion

        /// <summary>
        /// Encola una cotización para sincronización en segundo plano con Hangfire.
        /// </summary>
        /// <param name="idCotizacion">ID de la cotización a sincronizar</param>
        /// <returns>ID del job en Hangfire</returns>
        /// <response code="202">Job encolado exitosamente</response>
        [HttpPost("sincronizarCotizacion")]
        [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
        public IActionResult SincronizarCotizacion(Guid idCotizacion)
        {
            // Encolar el job en Hangfire
            var jobId = _backgroundJobClient.Enqueue<SincronizacionJobService>(
                service => service.EjecutarSincronizacionCotizacion(idCotizacion));

            _logger.LogInformation("Job encolado para cotización {IdCotizacion} con JobId {JobId}", idCotizacion, jobId);

            return Accepted(new
            {
                message = "Sincronización encolada exitosamente",
                jobId,
                idCotizacion,
                dashboardUrl = $"/hangfire/jobs/details/{jobId}"
            });
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
