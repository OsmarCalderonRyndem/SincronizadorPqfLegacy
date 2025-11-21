
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.API.Model
{
    /// <summary>
    /// Extends the standard ProblemDetails class to include additional information such as TraceId.
    /// TraceId can be useful for tracking and correlating errors in logs and diagnostics.
    /// </summary>
    public class ExtendedProblemDetails : ProblemDetails
    {
        /// <summary>
        /// Gets or sets the unique identifier for tracing the request.
        /// </summary>
        public required string TraceId { get; set; }
    }
}
