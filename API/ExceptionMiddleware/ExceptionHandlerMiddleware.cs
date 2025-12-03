using SincronizadorPqfLegacy.Application.Factorys;
using SincronizadorPqfLegacy.Domain.Interfaces;
using System.Text.Json;

namespace SincronizadorPqfLegacy.API.ExceptionMiddleware
{

    /// <summary>
    /// Middleware for centralized exception handling under the 'RFC 7807 – ProblemDetails' standard.
    /// </summary>
    /// <remarks>This middleware intercepts exceptions that occur during
    /// HTTP request processing and generates an HTTP response with a status code and error message
    /// in JSON format. Uses IExceptionClassifier for unified exception classification and HTTP mapping.</remarks>
    /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    public class ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger,
        IConfiguration configuration)
    {
        private readonly RequestDelegate _next = next;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Procesa una solicitud HTTP y maneja cualquier excepción que ocurra durante la ejecución del middleware.
        /// </summary>
        /// <param name="httpContext">El contexto HTTP que contiene toda la información sobre la solicitud actual.</param>
        /// <param name="exceptionClassifier">Service for classifying exceptions (injected per request).</param>
        /// <returns>Una tarea que representa la operación asincrónica de procesamiento de la solicitud.</returns>
        public async Task InvokeAsync(HttpContext httpContext, IExceptionClassifier exceptionClassifier)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                var showDetails = _configuration.GetValue<bool>("EnvironmentSettings:ShowDetailsExceptions");
                var exeptionDetails = showDetails ? GetExceptionDetails(ex) : null;

                logger.LogError(ex, "Unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(httpContext, ex, exeptionDetails, exceptionClassifier);
            }
        }


        /// <summary>
        /// Maneja excepciones no controladas y configura una respuesta HTTP adecuada en formato JSON.
        /// Utiliza IExceptionClassifier para determinar el código de estado y título apropiados.
        /// </summary>
        /// <param name="context">El contexto HTTP asociado con la solicitud actual.</param>
        /// <param name="exception">La excepción que se produjo durante el procesamiento de la solicitud.</param>
        /// <param name="exeptionDetails">Detalle de la exception que solo se muestra en ambiente de desarrollo</param>
        /// <param name="exceptionClassifier">Service for classifying exceptions.</param>
        /// <returns>Una tarea que representa la operación asincrónica de escritura de la respuesta HTTP.</returns>
        public async Task HandleExceptionAsync(HttpContext context, Exception exception, object? exeptionDetails, IExceptionClassifier exceptionClassifier)
        {
            // Usar el clasificador para obtener detalles HTTP
            var (statusCode, title) = exceptionClassifier.GetHttpDetails(exception);

            //Se configura la respuesta HTTP
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var problem = ProblemDetailsHelper.CreateProblemDetails(context, statusCode, title, "");

            problem.Detail = exception.Message ?? "";
            if (exeptionDetails != null)
                problem.Extensions["exceptionDetails"] = exeptionDetails;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsJsonAsync(problem, options);
        }

        private static object? GetExceptionDetails(Exception? exception)
        {
            if (exception == null) return null;

            return new
            {
                exception.Source,
                exception.Message,
                exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
        }

    }
}
