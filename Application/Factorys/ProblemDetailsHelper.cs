using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SincronizadorPqfLegacy.Application.Factorys
{
    /// <summary>
    /// Provides utility methods for creating standardized <see cref="ProblemDetails"/> or  <see
    /// cref="ValidationProblemDetails"/> objects for HTTP responses.
    /// </summary>
    /// <remarks>This helper is designed to simplify the creation of problem details objects that conform to 
    /// the RFC 7807 specification for HTTP API error responses. It supports adding a trace identifier  to the problem
    /// details for easier debugging.</remarks>
    public class ProblemDetailsHelper
    {
        public static ProblemDetails CreateProblemDetails(HttpContext context,
        int statusCode,
        string title,
        string type,
        IDictionary<string, string[]>? errors = null)
        {
            var traceId = Activity.Current?.TraceId.ToString()
                          ?? context.TraceIdentifier;

            var problem = errors is null
                ? new ProblemDetails()
                : new ValidationProblemDetails(errors);

            problem.Type = type;
            problem.Title = title;
            problem.Status = statusCode;

            problem.Extensions["traceId"] = traceId;

            return problem;
        }
    }
}
