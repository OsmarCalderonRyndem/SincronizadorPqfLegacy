using Infrastructure.Persistence.PConnect.Contexts;
using Infrastructure.Persistence.PConnect.Entities;
using Infrastructure.Persistence.ProquifaDotNet.Contexts;
using Infrastructure.Persistence.ProquifaDotNet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SincronizadorPqfLegacy.Application.Services
{
    public class SincronizarCotizacionService : ISincronizarCotizacion
    {
        private readonly ProquifaDotNetContext _proquifaDotNetContext;
        private readonly PConnectContext _pConnectContext;
        private readonly ILogger<SincronizarCotizacionService> _logger;

        public SincronizarCotizacionService(
            ProquifaDotNetContext proquifaDotNetContext,
            PConnectContext pConnectContext,
            ILogger<SincronizarCotizacionService> logger)
        {
            _proquifaDotNetContext = proquifaDotNetContext;
            _pConnectContext = pConnectContext;
            _logger = logger;
        }
        public async Task<Guid> SincronizarCotizacion(Guid idCotizacion)
        {
            try
            {
                _logger.LogInformation("Iniciando sincronización de cotización con folio: {Folio}", idCotizacion);

                // PASO 1: EXTRACT - Obtener cotización de origen
                var cotizacionOrigen = await ExtraerCotizacionOrigenAsync(idCotizacion);


                //TODO: AGREGAR VALIDACIONES
  

                // PASO 2: TRANSFORM - Convertir a entidad destino
                var cotiza = TransformarCotizacion(cotizacionOrigen!);

                // PASO 3: LOAD - Guardar en destino
                //await CargarCotizacionDestinoAsync(cotizacionDestino);

                // Commit
                //await _pConnectContext.SaveChangesAsync();

                //resultado.Exitoso = true;
                //resultado.RegistrosSincronizados = 1;
                //resultado.Mensaje = $"Cotización {folio} sincronizada exitosamente";

                _logger.LogInformation("Cotización {Folio} sincronizada correctamente", idCotizacion);
                return (Guid)cotizacionOrigen.IdCotCotizacion;
            }
            catch (Exception ex)
            {
                //resultado.Exitoso = false;
                //resultado.Mensaje = $"Error al sincronizar cotización: {ex.Message}";
                //resultado.Errores.Add(ex.ToString());
                //resultado.RegistrosConError = 1;

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
            _logger.LogDebug("Transformando cotización: {Clave}", vista.Param_0);

            var cotiza = new Cotiza
            {
                // Orden según UPDATE de BD
                Clave = vista.Param_0,
                Cliente = vista.Param_1,
                Contacto = vista.Param_2,
                Vendedor = vista.Param_3,
                Moneda = vista.Param_4,
                Parciales = vista.Param_5,
                CPago = vista.Param_6,
                Zona = vista.Param_7,
                Estado = vista.Param_8,
                FEnvio = vista.Param_9,
                IMoneda = vista.Param_10,
                Cotizo = vista.Param_11,
                Factura = vista.Param_12,
                HEntrada = vista.Param_13,
                MEntrada = vista.Param_14,
                MSalida = vista.Param_15,
                FechaClasif = vista.Param_16,
                FechaCierre = vista.Param_17,
                InfoFacturacion = vista.Param_18,
                Abierto = vista.Param_19,
                FS = vista.Param_20,
                GravaIVA = vista.Param_21,
                Generada = vista.Param_22,
                Tipo = vista.Param_23,
                DeSistema = vista.Param_24,
                Nombre = vista.Param_25,
                HSalida = vista.Param_26,
                idContacto = vista.Param_27,

                //// Campos adicionales
                //Fecha = vista.Fecha,
                //Vigencia = vista.Vigencia,
                //Observa = vista.Observa,
                //ObservaC = vista.ObservaC,
                //Confirmo = vista.Confirmo,
                //CanceladaDesde = vista.CanceladaDesde,
                //Lugar = vista.Lugar,
                //Orden = vista.Orden,

                //// FKs
                //FK01_idCliente = null,
                //FK02_DoctosR = null,
                //FK03_idVisita = null
            };

            // Validaciones
            if (string.IsNullOrEmpty(cotiza.Vigencia))
                cotiza.Vigencia = "30 días";

            if (string.IsNullOrWhiteSpace(cotiza.Cliente))
                cotiza.Cliente = "CLIENTE NO ESPECIFICADO";

            if (string.IsNullOrWhiteSpace(cotiza.Moneda))
                cotiza.Moneda = "MXN";

            if (string.IsNullOrWhiteSpace(cotiza.Estado))
                cotiza.Estado = "NUEVA";

            _logger.LogDebug("Transformación completada: {Clave}", cotiza.Clave);
            return cotiza;
        }

        #endregion
    }
}
