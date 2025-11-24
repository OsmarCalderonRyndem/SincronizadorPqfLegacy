using Infrastructure.Persistence.PConnect.Contexts;
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
                //if (cotizacionOrigen == null)
                //{
                //    resultado.Exitoso = false;
                //    resultado.Mensaje = $"No se encontró la cotización con folio {folio} en ProquifaDotNet";
                //    _logger.LogWarning(resultado.Mensaje);
                //    return resultado;
                //}

                // PASO 2: TRANSFORM - Convertir a entidad destino
                //var cotizacionDestino = await TransformarCotizacionAsync(cotizacionOrigen);

                // PASO 3: LOAD - Guardar en destino
                //await CargarCotizacionDestinoAsync(cotizacionDestino);

                // Commit
                //await _pConnectContext.SaveChangesAsync();

                //resultado.Exitoso = true;
                //resultado.RegistrosSincronizados = 1;
                //resultado.Mensaje = $"Cotización {folio} sincronizada exitosamente";

                _logger.LogInformation("Cotización {Folio} sincronizada correctamente", idCotizacion);
                return cotizacionOrigen.IdCotCotizacion;
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
        private async Task<vCotCotizacion?> ExtraerCotizacionOrigenAsync(Guid IdCotCotizacion)
        {
            _logger.LogDebug("Extrayendo cotización con folio: {Folio}", IdCotCotizacion);

            // Buscamos por el campo Folio en la tabla cotCotizacion
            var cotizacion = await _proquifaDotNetContext.vCotCotizacions
                .Where(c => c.IdCotCotizacion == IdCotCotizacion && c.Activo) // Solo activas
                .FirstOrDefaultAsync();

            if (cotizacion != null)
            {
                _logger.LogDebug("Cotización encontrada: {Id}", cotizacion.IdCotCotizacion);
            }

            return cotizacion;
        }

        #endregion
    }
}
