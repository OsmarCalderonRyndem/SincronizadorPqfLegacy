using Microsoft.Data.SqlClient;
using SincronizadorPqfLegacy.Domain.Exceptions;
using SincronizadorPqfLegacy.Domain.Interfaces;
using SincronizadorPqfLegacy.Domain.Models;
using SincronizadorPqfLegacy.Application.Exceptions;
using SincronizadorPqfLegacy.Infrastructure.Exceptions;
using FluentValidation;

namespace SincronizadorPqfLegacy.Application.Services
{
    /// <summary>
    /// Clasifica excepciones para determinar su categoría (Transient/Permanent) y mapeo HTTP.
    /// </summary>
    public class ExceptionClassifier : IExceptionClassifier
    {
        public ErrorCategory Classify(Exception ex)
        {
            if (ex is ValidationException || ex is DomainException || ex is ArgumentException || ex is InvalidOperationException)
            {
                return ErrorCategory.Permanent;
            }

            if (ex is SqlException sqlEx)
            {
                // SQL Error Codes for Transient Failures:
                // -2: Timeout
                // 1205: Deadlock
                // 53: Network path not found
                // 64: Connection lost
                if (sqlEx.Number == -2 || sqlEx.Number == 1205 || sqlEx.Number == 53 || sqlEx.Number == 64)
                {
                    return ErrorCategory.Transient;
                }

                // FK violation (547), PK violation (2627, 2601), Data truncation (8152) -> Permanent
                return ErrorCategory.Permanent;
            }

            if (ex is TimeoutException)
            {
                return ErrorCategory.Transient;
            }

            // Default to Transient for unknown errors to allow Hangfire retries
            return ErrorCategory.Transient; 
        }

        public (int StatusCode, string Title) GetHttpDetails(Exception ex)
        {
            return ex switch
            {
                // Domain Exceptions -> 400 Bad Request
                DomainException => (400, "Domain Error"),
                DomainArgumentNullException => (400, "Domain Argument Null Error"),
                
                // Application Exceptions -> 400/404
                AppArgumentException => (400, "Application Argument Error"),
                AppArgumentNullException => (400, "Application Argument Null Error"),
                AppKeyNotFoundException => (404, "Application Key Not Found Error"),
                AppFileNotFoundException => (404, "Application File Not Found Error"),
                AppException => (400, "Application Error"),
                
                // Infrastructure Exceptions
                InfrastructureNotImplementedException => (501, "Infrastructure Not Implemented Error"),
                InfrastructureException => (400, "Infrastructure Error"),
                
                // FluentValidation
                ValidationException => (400, "Validation Error"),
                
                // Transient errors -> 503 Service Unavailable
                TimeoutException => (503, "Service Temporarily Unavailable"),
                SqlException sqlEx when Classify(sqlEx) == ErrorCategory.Transient => (503, "Database Temporarily Unavailable"),
                
                // Default -> 500 Internal Server Error
                _ => (500, "Internal Server Error")
            };
        }
    }
}
