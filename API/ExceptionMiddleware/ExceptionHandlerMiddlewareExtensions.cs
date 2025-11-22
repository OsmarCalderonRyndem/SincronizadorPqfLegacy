namespace SincronizadorPqfLegacy.API.ExceptionMiddleware
{

    /// <summary>
    /// Provides an (extension) method for registering custom exception handling middleware in the application.
    /// </summary>
    /// <remarks>This method adds the <see cref="ExceptionHandlerMiddleware"/> middleware to the pipeline
    /// of requests to capture and handle unhandled exceptions centrally.</remarks>
    public static class ExceptionHandlerMiddlewareExtensions
    {
        /// <summary>
        /// Adds the <see cref="ExceptionHandlerMiddleware"/> to the application's request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance to configure.</param>
        public static void UseExceptionHandlerMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}
