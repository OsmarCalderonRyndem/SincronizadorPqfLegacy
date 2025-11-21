using Microservicio.Application.Exceptions;
using Microservicio.Application.Factorys;
using Microservicio.Domain.Exceptions;
using Microservicio.Infrastructure.Exceptions;
using System.Text.Json;

namespace Microservicio.API.ExceptionMiddleware
{

    /// <summary>
    /// Middleware for centralized exception handling under the 'RFC 7807 – ProblemDetails' standard.
    /// </summary>
    /// <remarks>This middleware intercepts exceptions that occur during
    /// HTTP request processing and generates an HTTP response with a status code and error message
    /// in JSON format. The HTTP status code is assigned based on the type of exception:
    /// <list type="bullet">
    /// <item> <description> <see cref="ApplicationException"/> generates a status code 400 (Bad Request).</description> </item>
    /// <item> <description><see cref="KeyNotFoundException"/> generates a status code 404 (Not Found).</description> </item>
    /// <item> <description>Other types of exceptions generate a status code 500 (Internal Server Error).
    /// </description> </item>
    /// </list></remarks>
    /// <remarks>
    /// Middleware that handles unhandled exceptions in the application. It captures exceptions thrown during
    /// request processing and converts them into appropriate HTTP responses.
    /// </remarks>
    /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IConfiguration configuration)
    {
        private readonly RequestDelegate _next = next;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Procesa una solicitud HTTP y maneja cualquier excepción que ocurra durante la ejecución del middleware.
        /// </summary>
        /// <param name="httpContext">El contexto HTTP que contiene toda la información sobre la solicitud actual.</param>
        /// <returns>Una tarea que representa la operación asincrónica de procesamiento de la solicitud.</returns>
        public async Task InvokeAsync(HttpContext httpContext)
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
                await HandleExceptionAsync(httpContext, ex, exeptionDetails);
            }
        }


        /// <summary>
        /// Maneja excepciones no controladas y configura una respuesta HTTP adecuada en formato JSON.
        /// </summary>
        /// <remarks>Este método analiza el tipo de excepción proporcionada y genera una respuesta HTTP
        /// con un código de estado y un cuerpo JSON que describe el error. Los tipos de excepciones manejados incluyen:
        /// <list type="bullet"> 
        /// <item> <description><see cref="DomainException"/>: Devuelve un código de estado 400 (Bad Request) con detalles específicos del dominio.</description> </item> 
        /// <item> <description><see cref="KeyNotFoundException"/>: Devuelve un código de estado 404 (Not Found) con un mensaje de error.</description> </item> 
        /// <item> <description>Otras excepciones: Devuelve un código de estado 500 (Internal Server Error) con detalles de la excepción.</description> </item> 
        /// </list></remarks>
        /// <param name="context">El contexto HTTP asociado con la solicitud actual.</param>
        /// <param name="exception">La excepción que se produjo durante el procesamiento de la solicitud.</param>
        /// <param name="exeptionDetails">Detalle de la exception que solo se muestra en ambiente de desarrollo</param>
        /// <returns>Una tarea que representa la operación asincrónica de escritura de la respuesta HTTP.</returns>
        public async Task HandleExceptionAsync(HttpContext context, Exception exception, object? exeptionDetails)
        {


            int statusCode;
            string title = "";
            //Se configura la respuesta HTTP
            context.Response.ContentType = "application/json";

            switch (exception)
            {
                case DomainException:
                    title = "Domain Error";
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case DomainArgumentNullException:
                    title = "Domain Argument Null Error";
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case AppArgumentException:
                    title = "Application Argument Error";
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case AppArgumentNullException:
                    title = "Application Argument Null Error";
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case AppKeyNotFoundException:
                    title = "Application Key Not Found Error";
                    statusCode = StatusCodes.Status404NotFound;
                    break;
                case AppFileNotFoundException:
                    title = "Application File Not Found Error";
                    statusCode = StatusCodes.Status404NotFound;
                    break;
                case AppException:
                    title = "Application Error";
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case InfrastructureNotImplementedException:
                    title = "Infrastructure Not Implemented Error";
                    statusCode = StatusCodes.Status501NotImplemented;
                    break;
                case InfrastructureException:
                    title = "Infrastructure Error";
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                default:
                    title = "Internal Server Error";
                    statusCode = StatusCodes.Status500InternalServerError;
                    break;
            }
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
