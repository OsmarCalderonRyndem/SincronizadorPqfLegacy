using Hangfire;
using Serilog;
using SincronizadorPqfLegacy.API.ExceptionMiddleware;
using SincronizadorPqfLegacy.API.Extensions;

// Configuración inicial de Serilog (bootstrap logger)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

Log.Information("Iniciando la aplicación...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configuración de Serilog
    builder.Host.ConfigureSerilog();

    // Configuración de servicios
    builder.Services.AddControllers();
    builder.Services.ConfigureSwagger();
    builder.Services.ConfigureDatabases(builder.Configuration);
    builder.Services.ConfigureApplicationServices();
    builder.Services.ConfigureETLServices(builder.Configuration);
    builder.Services.ConfigureProblemDetails();
    builder.Services.ConfigureHangfire(builder.Configuration);

    var app = builder.Build();

    // Configurar filtros globales de Hangfire con inyección de dependencias
    GlobalJobFilters.Filters.Add(new SincronizadorPqfLegacy.API.Filters.AutomaticRetryFailureLogFilter(app.Services));

    // Configurar jobs recurrentes de Hangfire
    app.ConfigureRecurringJobs(builder.Configuration);

    // Middleware para registrar solicitudes HTTP
    app.UseSerilogRequestLogging();

    // Configurar Swagger
    var showSwagger = builder.Configuration.GetValue<bool>("EnvironmentSettings:ShowSwagger");
    if (showSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();

    // Middleware de manejo de excepciones
    app.UseMiddleware<ExceptionHandlerMiddleware>();

    // Dashboard de Hangfire
    app.UseHangfireDashboard();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación no pudo iniciarse correctamente.");
}
finally
{
    Log.CloseAndFlush();
}
