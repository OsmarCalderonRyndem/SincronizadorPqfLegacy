using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SincronizadorPqfLegacy.Application.DTOs;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Application.Services;
using SincronizadorPqfLegacy.Domain.Enums;
using SincronizadorPqfLegacy.Domain.Models;

namespace SincronizadorPqfLegacy.API.Controllers
{
    /// <summary>
    /// Controlador genérico para la ejecución de procesos ETL (Extract, Transform, Load).
    /// Permite iniciar sincronizaciones de diferentes entidades mediante un identificador y el tipo de proceso.
    /// </summary>
    /// <remarks>
    /// Este controlador maneja todos los procesos ETL del sistema de forma centralizada.
    /// Los jobs se ejecutan en segundo plano mediante Hangfire, permitiendo escalar y monitorear
    /// fácilmente cualquier proceso de sincronización.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class EtlController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ISincronizacionMultipleService _sincronizacionMultipleService;
        private readonly ProcesoEtlMetadataService _metadataService;
        private readonly ILogger<EtlController> _logger;

        /// <summary>
        /// Constructor del controlador ETL.
        /// </summary>
        public EtlController(
            IBackgroundJobClient backgroundJobClient,
            ISincronizacionMultipleService sincronizacionMultipleService,
            ProcesoEtlMetadataService metadataService,
            ILogger<EtlController> logger)
        {
            _backgroundJobClient = backgroundJobClient;
            _sincronizacionMultipleService = sincronizacionMultipleService;
            _metadataService = metadataService;
            _logger = logger;
        }

        /// <summary>
        /// Encola un trabajo de sincronización ETL para una entidad específica.
        /// </summary>
        /// <param name="tipoProceso">Tipo de proceso ETL (Cotizacion, Partida, etc.)</param>
        /// <param name="recordId">Identificador único del registro principal a sincronizar</param>
        /// <param name="parametrosAdicionales">Parámetros adicionales en formato JSON (opcional, por ejemplo: {"pkFolio": 123})</param>
        /// <returns>ID del trabajo encolado en Hangfire</returns>
        /// <response code="202">Trabajo encolado exitosamente</response>
        /// <response code="400">Si el recordId es inválido</response>
        /// <remarks>
        /// Ejemplos de uso:
        ///
        /// **Sincronizar una cotización:**
        /// ```
        /// POST /api/etl/sincronizar?tipoProceso=Cotizacion&amp;recordId={guid}
        /// ```
        ///
        /// **Sincronizar partidas de una cotización:**
        /// ```
        /// POST /api/etl/sincronizar?tipoProceso=Partida&amp;recordId={guid}&amp;parametrosAdicionales={"pkFolio":123}
        /// ```
        /// </remarks>
        [HttpPost("sincronizar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Sincronizar(
            [FromQuery] TipoProcesoEtl tipoProceso,
            [FromQuery] Guid recordId,
            [FromQuery] string? parametrosAdicionales = null)
        {
            if (recordId == Guid.Empty)
            {
                return BadRequest(new { error = "El recordId no puede ser un GUID vacío" });
            }

            // Encolar el job en Hangfire (el PerformContext es inyectado automáticamente por Hangfire)
            var jobId = _backgroundJobClient.Enqueue<SincronizacionJobService>(
                service => service.EjecutarSincronizacion(tipoProceso, recordId, parametrosAdicionales, null));

            _logger.LogInformation(
                "Job ETL encolado: Tipo={TipoProceso}, RecordId={RecordId}, JobId={JobId}, Parametros={Parametros}",
                tipoProceso, recordId, jobId, parametrosAdicionales ?? "ninguno");

            return Accepted(new
            {
                message = $"Proceso ETL '{tipoProceso}' encolado exitosamente",
                jobId,
                tipoProceso = tipoProceso.ToString(),
                recordId,
                parametrosAdicionales,
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

        /// <summary>
        /// Obtiene el catálogo completo de procesos ETL disponibles.
        /// </summary>
        /// <returns>Lista de procesos ETL con sus metadatos (clave, nombre, descripción, ejemplos).</returns>
        /// <response code="200">Catálogo obtenido exitosamente.</response>
        /// <remarks>
        /// Este endpoint es útil para:
        /// - Mostrar una lista de procesos disponibles en una UI
        /// - Conocer qué parámetros adicionales requiere cada proceso
        /// - Obtener ejemplos de uso para cada proceso
        ///
        /// Ejemplo de respuesta:
        /// ```json
        /// [
        ///   {
        ///     "clave": 1,
        ///     "nombre": "Cotizacion",
        ///     "descripcion": "Sincroniza una cotización...",
        ///     "requiereParametrosAdicionales": false,
        ///     "ejemploParametros": null,
        ///     "ejemploLlamada": "POST /api/etl/sincronizar?tipoProceso=Cotizacion&amp;recordId={guid}"
        ///   }
        /// ]
        /// ```
        /// </remarks>
        [HttpGet("catalogo")]
        [ProducesResponseType(typeof(IEnumerable<ProcesoEtlMetadata>), StatusCodes.Status200OK)]
        public IActionResult ObtenerCatalogo()
        {
            var catalogo = _metadataService.ObtenerCatalogo();
            return Ok(catalogo);
        }

        /// <summary>
        /// Obtiene el catálogo de procesos ETL en formato diccionario (Clave -> Metadata).
        /// </summary>
        /// <returns>Diccionario donde la clave es el número del proceso y el valor son sus metadatos.</returns>
        /// <response code="200">Catálogo obtenido exitosamente.</response>
        /// <remarks>
        /// Este formato es útil cuando necesitas acceder directamente a un proceso por su clave numérica.
        ///
        /// Ejemplo de respuesta:
        /// ```json
        /// {
        ///   "1": {
        ///     "clave": 1,
        ///     "nombre": "Cotizacion",
        ///     "descripcion": "Sincroniza una cotización...",
        ///     ...
        ///   },
        ///   "2": {
        ///     "clave": 2,
        ///     "nombre": "Partida",
        ///     ...
        ///   }
        /// }
        /// ```
        /// </remarks>
        [HttpGet("catalogo/diccionario")]
        [ProducesResponseType(typeof(Dictionary<int, ProcesoEtlMetadata>), StatusCodes.Status200OK)]
        public IActionResult ObtenerCatalogoDiccionario()
        {
            var catalogo = _metadataService.ObtenerCatalogoDiccionario();
            return Ok(catalogo);
        }

        /// <summary>
        /// Obtiene los metadatos de un proceso ETL específico.
        /// </summary>
        /// <param name="tipoProceso">Tipo de proceso ETL.</param>
        /// <returns>Metadatos del proceso solicitado.</returns>
        /// <response code="200">Metadatos obtenidos exitosamente.</response>
        /// <response code="404">Proceso ETL no encontrado.</response>
        [HttpGet("catalogo/{tipoProceso}")]
        [ProducesResponseType(typeof(ProcesoEtlMetadata), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerMetadataProceso(TipoProcesoEtl tipoProceso)
        {
            try
            {
                var metadata = _metadataService.ObtenerMetadata(tipoProceso);
                return Ok(metadata);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"No se encontró el proceso ETL: {tipoProceso}" });
            }
        }
    }
}
