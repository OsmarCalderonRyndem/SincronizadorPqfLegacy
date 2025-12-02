using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using SincronizadorPqfLegacy.Application.Interfaces;

namespace SincronizadorPqfLegacy.API.Filters
{
    public class AutomaticRetryFailureLogFilter : IElectStateFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public AutomaticRetryFailureLogFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

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
            string recordIdentifier = jobArgs.Count > 0 ? jobArgs[0]?.ToString() ?? "Unknown" : "Unknown";
            string entityName = "JobFailure"; // Nombre genérico, idealmente se extraería del tipo de Job

            // Usar un scope para resolver el servicio de logs
            using (var scope = _serviceProvider.CreateScope())
            {
                var syncLogService = scope.ServiceProvider.GetService<ISyncLogService>();
                if (syncLogService != null)
                {
                    // Ejecutar de manera síncrona porque estamos en un filtro síncrono
                    syncLogService.LogPermanentFailureAsync(entityName, recordIdentifier, exception).GetAwaiter().GetResult();
                }
            }
        }
    }
}
