using AutoMapper;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

namespace SincronizadorPqfLegacy.Application.Services
{
    public class SincronizarCotizacionService : ISincronizarCotizacion
    {
        private readonly ICotizacionOrigenRepository _cotizacionOrigenRepo;
        private readonly ICotizacionLegacyRepository _cotizacionLegacyRepo;
        private readonly ICotizacionControlRepository _cotizacionControlRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<SincronizarCotizacionService> _logger;

        public SincronizarCotizacionService(
            ICotizacionOrigenRepository cotizacionOrigenRepo,
            ICotizacionLegacyRepository cotizacionLegacyRepo,
            ICotizacionControlRepository cotizacionControlRepo,
            IMapper mapper,
            ILogger<SincronizarCotizacionService> logger)
        {
            _cotizacionOrigenRepo = cotizacionOrigenRepo;
            _cotizacionLegacyRepo = cotizacionLegacyRepo;
            _cotizacionControlRepo = cotizacionControlRepo;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<Guid> SincronizarCotizacion(Guid idCotizacion)
        {
            try
            {
                _logger.LogInformation("Iniciando sincronización de cotización: {Id}", idCotizacion);

                // ==========================================
                // PASO 1: EXTRACT - Obtener de origen
                // ==========================================
                var cotizacionOrigenDto = await ExtraerCotizacionOrigenAsync(idCotizacion);

                if (cotizacionOrigenDto == null)
                {
                    throw new InvalidOperationException(
                        $"Cotización {idCotizacion} no encontrada en sistema origen");
                }

                // ==========================================
                // PASO 1.5: Registrar inicio en tabla control
                // ==========================================
                var controlDto = await RegistrarInicioEnControlAsync(cotizacionOrigenDto);

                // ==========================================
                // PASO 2: TRANSFORM - DTO Origen → DTO Legacy
                // ==========================================
                var cotizacionLegacyDto = TransformarCotizacion(cotizacionOrigenDto);

                // ==========================================
                // PASO 3: LOAD - Guardar en Legacy
                // ==========================================
                var cotizaInsertada = await CargarCotizacAsync(cotizacionLegacyDto);

                // ==========================================
                // PASO 4: Actualizar tabla de control con PK
                // ==========================================
                await ActualizarControlConPKAsync(controlDto, cotizaInsertada);

                _logger.LogInformation("Cotización {Id} sincronizada correctamente. PK Legacy: {PK}",
                    idCotizacion,
                    cotizaInsertada.PK_Folio);

                return cotizacionOrigenDto.IdCotCotizacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar cotización {Folio}", idCotizacion);
                throw;
            }
        }

        #region EXTRACT - Extraer datos del origen
        /// <summary>
        /// PASO 1: EXTRACT
        /// Obtiene la cotización desde ProquifaDotNet (origen)
        /// </summary>
        private async Task<CotizacionOrigenDto?> ExtraerCotizacionOrigenAsync(Guid idCotCotizacion)
        {
            _logger.LogDebug("Extrayendo cotización con folio: {Folio}", idCotCotizacion);

            var cotizacion = await _cotizacionOrigenRepo.ObtenerPorIdAsync(idCotCotizacion);

            if (cotizacion != null)
            {
                _logger.LogDebug("Cotización encontrada: {Id}", cotizacion.IdCotCotizacion);
            }

            return cotizacion;
        }
        #endregion


        #region TRANSFORM - Transformar datos
        /// <summary>
        /// PASO 2: TRANSFORM
        /// Convierte los datos de la vista a la entidad Legacy (Cotiza)
        /// 
        /// Responsabilidades:
        /// - Mapeo campo a campo
        /// - Aplicar reglas de negocio específicas
        /// - Manejar valores null/default
        /// - Validaciones de datos
        /// </summary>
        /// <param name="vista">Datos de la vista con lookups ya resueltos</param>
        /// <returns>Entidad lista para insertar en PConnect</returns>
        private CotizacionLegacyDto TransformarCotizacion(CotizacionOrigenDto vista)
        {
            _logger.LogDebug("Transformando cotización: {Clave}", vista.Clave);

            var cotiza = _mapper.Map<CotizacionLegacyDto>(vista);

            _logger.LogDebug("Transformación completada: {Clave}", cotiza.Clave);
            return cotiza;
        }
        #endregion

        #region LOAD - Cargar en destino
        private async Task<CotizacionLegacyDto> CargarCotizacAsync(CotizacionLegacyDto cotizacion)
        {
            _logger.LogDebug("Cargando cotización en Legacy: {Clave}", cotizacion.Clave);

            // Verificar si ya existe
            var existente = await _cotizacionLegacyRepo.ObtenerPorClaveAsync(cotizacion.Clave!);

            CotizacionLegacyDto resultado;

            if (existente != null)
            {
                // ACTUALIZAR
                _logger.LogInformation("Actualizando cotización existente: {Clave}, PK: {PK}",
                    cotizacion.Clave,
                    existente.PK_Folio);

                cotizacion.PK_Folio = existente.PK_Folio; // Mantener la PK
                resultado = await _cotizacionLegacyRepo.ActualizarAsync(cotizacion);
            }
            else
            {
                // INSERTAR
                _logger.LogInformation("Insertando nueva cotización: {Clave}", cotizacion.Clave);

                resultado = await _cotizacionLegacyRepo.InsertarAsync(cotizacion);
            }

            _logger.LogDebug("Cotización cargada en Legacy: {Clave}, PK: {PK}",
                resultado.Clave,
                resultado.PK_Folio);

            return resultado;
        }
        #endregion

        #region CONTROL

        /// <summary>
        /// Registra el inicio del proceso en la tabla de control
        /// </summary>
        private async Task<CotizacionControlDto> RegistrarInicioEnControlAsync(CotizacionOrigenDto origen)
        {
            _logger.LogDebug("Registrando inicio en tabla de control: {Id}", origen.IdCotCotizacion);

            // Verificar si ya existe
            var existente = await _cotizacionControlRepo.ObtenerPorIdAsync(origen.IdCotCotizacion);

            if (existente != null)
            {
                _logger.LogDebug("Registro de control ya existe: {Id}", origen.IdCotCotizacion);
                return existente;
            }

            // Crear nuevo registro
            var controlDto = new CotizacionControlDto
            {
                CotizacionPQF = origen.IdCotCotizacion,
                Folio = origen.Clave,
                Insertado = true,
                Actualizado = false,
                RegistroCompleto = false,
                FechaRegistro = DateTime.Now,
                FechaUltimaActualizacion = DateTime.Now
            };

            var insertado = await _cotizacionControlRepo.InsertarAsync(controlDto);

            _logger.LogDebug("Registro de control creado: {Id}", origen.IdCotCotizacion);

            return insertado;
        }

        /// <summary>
        /// Actualiza la tabla de control con el PK de Legacy
        /// </summary>
        private async Task ActualizarControlConPKAsync(
            CotizacionControlDto control,
            CotizacionLegacyDto legacy)
        {
            _logger.LogDebug("Actualizando tabla de control con PK Legacy: {PK}", legacy.PK_Folio);

            control.CotizacionLegacy = legacy.PK_Folio;
            control.PK_Folio = legacy.PK_Folio;
            control.RegistroCompleto = true;
            control.Actualizado = true;
            control.FechaUltimaActualizacion = DateTime.Now;
            control.FechaUltimaActualizacionLegacy = DateTime.Now;

            await _cotizacionControlRepo.ActualizarAsync(control);

            _logger.LogDebug("Tabla de control actualizada: {Id}", control.CotizacionPQF);
        }

        #endregion
    }
}
