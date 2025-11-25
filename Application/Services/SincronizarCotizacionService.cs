using AutoMapper;
using Infrastructure.Persistence.PConnect.Contexts;
using Infrastructure.Persistence.PConnect.Entities;
using Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;
using Infrastructure.Persistence.PConnectProquifaDotNet.Entities;
using Infrastructure.Persistence.ProquifaDotNet.Contexts;
using Infrastructure.Persistence.ProquifaDotNet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Application.Interfaces;

namespace SincronizadorPqfLegacy.Application.Services
{
    public class SincronizarCotizacionService : ISincronizarCotizacion
    {
        private readonly ProquifaDotNetContext _proquifaDotNetContext;
        private readonly PConnectContext _pConnectContext;
        private readonly PConnectProquifaDotNetContext _pConnectProquifaDotNetContext;
        private readonly ILogger<SincronizarCotizacionService> _logger;
        private readonly IMapper _mapper;

        public SincronizarCotizacionService(
            ProquifaDotNetContext proquifaDotNetContext,
            PConnectContext pConnectContext,
            PConnectProquifaDotNetContext pConnectProquifaDotNetContext,
            ILogger<SincronizarCotizacionService> logger,
            IMapper mapper)
        {
            _proquifaDotNetContext = proquifaDotNetContext;
            _pConnectContext = pConnectContext;
            _pConnectProquifaDotNetContext = pConnectProquifaDotNetContext;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<Guid> SincronizarCotizacion(Guid idCotizacion)
        {
            try
            {
                _logger.LogInformation("Iniciando sincronización de cotización con folio: {Folio}", idCotizacion);

                // PASO 1: EXTRACT - Obtener cotización de origen
                var cotizacionOrigen = await ExtraerCotizacionOrigenAsync(idCotizacion);

                //TODO: AGREGAR VALIDACIONES

                // Cargar en tabla de control
                var cotizacionTablaControl = await CargarCotizacionTablaDeControl(idCotizacion, false, cotizacionOrigen);

                // PASO 2: TRANSFORM - Convertir a entidad destino
                var cotizacionTransformada = TransformarCotizacion(cotizacionOrigen!);

                // PASO 3: LOAD - Guardar en destino
                var cotiza = await CargarCotizacAsync(cotizacionTransformada);

                // Actualizar tabla de control con PK de cotización legacy
                cotizacionTablaControl = await CargarCotizacionTablaDeControl(idCotizacion, false, cotizacionOrigen, cotiza);              

                _logger.LogInformation("Cotización {Folio} sincronizada correctamente", idCotizacion);
                return (Guid)cotizacionOrigen.IdCotCotizacion;
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
        private async Task<vCotizacionesTransformadasETL?> ExtraerCotizacionOrigenAsync(Guid IdCotCotizacion)
        {
            _logger.LogDebug("Extrayendo cotización con folio: {Folio}", IdCotCotizacion);

            // Buscamos por el campo Folio en la tabla cotCotizacion
            var cotizacion = await _proquifaDotNetContext.vCotizacionesTransformadasETLs
                .Where(c => c.IdCotCotizacion == IdCotCotizacion) // Solo activas
                .FirstOrDefaultAsync();

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
        private Cotiza TransformarCotizacion(vCotizacionesTransformadasETL vista)
        {
            _logger.LogDebug("Transformando cotización: {Clave}", vista.Clave);

            var cotiza = _mapper.Map<Cotiza>(vista);

            _logger.LogDebug("Transformación completada: {Clave}", cotiza.Clave);
            return cotiza;
        }
        #endregion

        #region LOAD - Cargar en destino
        private async Task<Cotiza> CargarCotizacAsync(Cotiza cotizacion)
        {
            _logger.LogDebug("Cargando cotización en PConnect: {Clave}", cotizacion.Clave);

            var cotizacionExistente = await _pConnectContext.Cotizas
                .Where(c => c.Clave == cotizacion.Clave)
                .FirstOrDefaultAsync();

            if (cotizacionExistente != null)
            {
                // ACTUALIZAR
                _logger.LogInformation("Actualizando cotización existente: {Clave} (PK: {PK})",
                    cotizacion.Clave,
                    cotizacionExistente.PK_Folio);

                cotizacionExistente = cotizacion;
                _pConnectContext.Cotizas.Update(cotizacionExistente);
            }
            else
            {
                // INSERTAR
                _logger.LogInformation("Insertando nueva cotización: {Clave}", cotizacion.Clave);
                await _pConnectContext.Cotizas.AddAsync(cotizacion);
            }

            // Commit
            await _pConnectContext.SaveChangesAsync();
            return cotizacion;
        }
        #endregion

        #region LOAD - Mapear e insertar en tabla de control
        private async Task<Cotizacione> CargarCotizacionTablaDeControl(Guid idCotizacion, bool sincronizada, vCotizacionesTransformadasETL cotCotizacion, Cotiza? cotiza = null)
        {
            _logger.LogDebug("Cargando cotización en PConnectProquifaDotNet: {CotizacionPQF}", idCotizacion);
            var nuevaCotizacionEnTablaControl = new Cotizacione();

            var cotizacionEnTablaControl = await _pConnectProquifaDotNetContext.Cotizaciones
                .Where(c => c.CotizacionPQF == idCotizacion)
                .FirstOrDefaultAsync();

            if (cotizacionEnTablaControl != null)
            {
                // ACTUALIZAR
                _logger.LogInformation("Actualizando cotización existente: {Folio}.",cotCotizacion.Clave);
                cotizacionEnTablaControl.RegistroCompleto = true;
                cotizacionEnTablaControl.CotizacionLegacy = cotiza?.PK_Folio;
                cotizacionEnTablaControl.FechaUltimaActualizacionLegacy = DateTime.Now;
                _pConnectProquifaDotNetContext.Cotizaciones.Update(cotizacionEnTablaControl);
                nuevaCotizacionEnTablaControl = cotizacionEnTablaControl;
            }
            else
            {
                // INSERTAR
                _logger.LogInformation("Insertando nueva cotización: {Folio}", cotCotizacion.Clave);
                nuevaCotizacionEnTablaControl = new Cotizacione()
                {
                    CotizacionPQF = idCotizacion,
                    Folio = cotCotizacion.Clave ?? "",
                    Insertado = true,
                    FechaRegistro = DateTime.Now,
                    Actualizado = false,
                    FechaUltimaActualizacion = DateTime.Now,
                    RegistroCompleto = true,
                };
                await _pConnectProquifaDotNetContext.Cotizaciones.AddAsync(nuevaCotizacionEnTablaControl);
            }
            await _pConnectProquifaDotNetContext.SaveChangesAsync();
            return nuevaCotizacionEnTablaControl;
        }
        #endregion
    }
}
