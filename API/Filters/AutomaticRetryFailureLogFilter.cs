using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using SincronizadorPqfLegacy.Application.Interfaces;

namespace SincronizadorPqfLegacy.API.Filters
{
    /// <summary>
    /// Provides a Hangfire state filter that logs permanent job failures after all automatic retry attempts have been
    /// exhausted. This filter enables auditing and diagnostics of background jobs that have failed irrecoverably.
    /// </summary>
    /// <remarks>This filter is intended for use with Hangfire's automatic retry mechanism. It is invoked only
    /// when a job transitions to a failed state, ensuring that details of permanent failures are recorded for
    /// monitoring or troubleshooting purposes. The filter relies on a logging service resolved from the provided
    /// service provider. Thread safety and logging implementation depend on the underlying logging service.</remarks>
    public class AutomaticRetryFailureLogFilter : IElectStateFilter
    {
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// Initializes a new instance of the AutomaticRetryFailureLogFilter class using the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies required by the filter. Cannot be null.</param>
        public AutomaticRetryFailureLogFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Handles the election of a job state when a background job has failed, and logs permanent failures for
        /// auditing or diagnostics.
        /// </summary>
        /// <remarks>This method is invoked only when a job transitions to a failed state, typically after
        /// all automatic retry attempts have been exhausted. It records details of the permanent failure using a
        /// logging service, which can be used for monitoring or troubleshooting. The method assumes the first job
        /// argument represents a unique identifier for the failed entity.</remarks>
        /// <param name="context">The context containing information about the candidate state and background job. Must not be null.</param>
        public void OnStateElection(ElectStateContext context)
        {
            // Solo nos interesa cuando el job falla
            if (!(context.CandidateState is FailedState failedState))
            {
                return;
            }

            // Verificar si es el último intento o si no hay reintentos configurados
            // Hangfire maneja los reintentos automáticos antes de pasar a FailedState final si se usa AutomaticRetryAttribute.
            // Si llega a FailedState, significa que ya agotó los reintentos o falló de manera fatal.
            
            // Extraer información del Job
            var jobId = context.BackgroundJob.Id;
            var exception = failedState.Exception;
            
            // Intentar obtener argumentos para identificar la entidad (asumiendo que el primer argumento es el ID)
            var jobArgs = context.BackgroundJob.Job.Args;
            string recordIdentifier = jobArgs.Count > 0 ? jobArgs[0]?.ToString() ?? Guid.Empty.ToString() : Guid.Empty.ToString();
            string entityName = "JobFailure"; // Nombre genérico, idealmente se extraería del tipo de Job

            // Usar un scope para resolver el servicio de logs
            using (var scope = _serviceProvider.CreateScope())
            {
                var syncLogService = scope.ServiceProvider.GetService<ISyncLogService>();
                if (syncLogService != null)
                {
                    // Ejecutar de manera síncrona porque estamos en un filtro síncrono
                    syncLogService.LogPermanentFailureAsync(entityName, Guid.Parse(recordIdentifier), exception).GetAwaiter().GetResult();
                }
            }
        }
    }
}
