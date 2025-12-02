using SincronizadorPqfLegacy.Domain.Enums;
using Hangfire.Server;

namespace SincronizadorPqfLegacy.Application.Services
{
    /// <summary>
    /// Interfaz para el servicio wrapper genérico de jobs de sincronización ETL.
    /// Define los métodos públicos para ejecutar procesos de sincronización con manejo de errores clasificado.
    /// </summary>
    public interface ISincronizacionJobService
    {
        /// <summary>
        /// Ejecuta un proceso de sincronización ETL basado en el tipo de proceso.
        /// </summary>
        /// <param name="tipoProceso">Tipo de proceso ETL (enum)</param>
        /// <param name="recordId">ID del registro principal a sincronizar</param>
        /// <param name="context">Contexto de Hangfire para reportar progreso (opcional)</param>
        Task EjecutarSincronizacion(TipoProcesoEtl tipoProceso, Guid recordId, PerformContext? context = null);

        /// <summary>
        /// Job recurrente para sincronizar todas las cotizaciones pendientes.
        /// Este método se ejecuta automáticamente cada cierto intervalo configurado en Hangfire.
        /// </summary>
        /// <param name="context">Contexto de Hangfire para reportar progreso (opcional)</param>
        Task EjecutarSincronizacionPendientesRecurrente(PerformContext? context = null);

        /// <summary>
        /// Job para procesar registros sin detonación inicial a ETL.
        /// Estos son procesos que no pudieron comunicarse con el API de ETL en su momento.
        /// </summary>
        /// <param name="context">Contexto de Hangfire para reportar progreso (opcional)</param>
        /// <remarks>
        /// TODO: Implementar la lógica específica según reglas de negocio.
        /// Por ahora es un placeholder para definir las reglas posteriormente.
        /// </remarks>
        Task EjecutarProcesosSinDetonacionInicial(PerformContext? context = null);
    }
}
