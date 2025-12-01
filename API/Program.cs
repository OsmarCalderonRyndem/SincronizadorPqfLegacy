using SincronizadorPqfLegacy.API.ExceptionMiddleware;
using SincronizadorPqfLegacy.API.Extensions;
using Serilog;

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

    // Add services to the container.
    builder.Services.AddControllers();
    
    // Configuración de Swagger
    builder.Services.ConfigureSwagger();

    // Configuración de Base de Datos
    builder.Services.ConfigureDatabase(builder.Configuration);

    // Configuración de Servicios de Aplicación (Services, Repos, Validators, Mapper)
    builder.Services.ConfigureApplicationServices();

    // Configuración de ProblemDetails
    builder.Services.ConfigureProblemDetails();

    var app = builder.Build();

    // Middleware para registrar automáticamente las solicitudes HTTP
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    var showSwagger = builder.Configuration.GetValue<bool>("EnvironmentSettings:ShowSwagger");
    if (showSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    // Habilitar middleware de tratamento de excepciones
    app.UseMiddleware<ExceptionHandlerMiddleware>();

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
