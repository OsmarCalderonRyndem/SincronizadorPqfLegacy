using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Persistence.PConnect.Contexts;
using Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;
using Infrastructure.Persistence.ProquifaDotNet.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SincronizadorPqfLegacy.API.BackgroundServices;
using SincronizadorPqfLegacy.API.ExceptionMiddleware;
using SincronizadorPqfLegacy.Application.Factorys;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Application.Services;
using SincronizadorPqfLegacy.Application.Validators;
using SincronizadorPqfLegacy.Domain.Interfaces;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;
using SincronizadorPqfLegacy.Domain.Interfaces.Repository;
using SincronizadorPqfLegacy.Infrastructure.Mappers;
using SincronizadorPqfLegacy.Infrastructure.Persistence;
using SincronizadorPqfLegacy.Infrastructure.Persistence.Context;
using SincronizadorPqfLegacy.Infrastructure.Repositories;
using SincronizadorPqfLegacy.Infrastructure.Repository;
using System.Reflection;

// Configuración inicial de Serilog (bootstrap logger)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

Log.Information("Iniciando la aplicación...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configuración completa de Serilog usando el host builder
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        //.Enrich.WithThreadName()
        .Enrich.WithThreadId()
        .Enrich.FromLogContext());

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // Incluir comentarios XML
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    });

    builder.Services.AddDbContext<MicroservicioContext>(options =>
    {
        var cs = builder.Configuration.GetConnectionString("DocumentBuilder");
        if (string.IsNullOrWhiteSpace(cs)) { 
            Log.Fatal("ConnectionStrings:DocumentBuilder is null/empty.");
            throw new InvalidOperationException("ConnectionStrings:DocumentBuilder is null/empty.");
        }

        options.UseSqlServer(cs);
    });

    builder.Services.AddDbContext<ProquifaDotNetContext>(options =>
    {
        var cs = builder.Configuration.GetConnectionString("ProquifaDotNet");
        if (string.IsNullOrWhiteSpace(cs))
        {
            Log.Fatal("ConnectionStrings:DocumentBuilder is null/empty.");
            throw new InvalidOperationException("ConnectionStrings:DocumentBuilder is null/empty.");
        }

        options.UseSqlServer(cs);
    });

    builder.Services.AddDbContext<PConnectProquifaDotNetContext>(options =>
    {
        var cs = builder.Configuration.GetConnectionString("PConnectProquifaDotNet");
        if (string.IsNullOrWhiteSpace(cs))
        {
            Log.Fatal("ConnectionStrings:DocumentBuilder is null/empty.");
            throw new InvalidOperationException("ConnectionStrings:DocumentBuilder is null/empty.");
        }

        options.UseSqlServer(cs);
    });

    builder.Services.AddDbContext<PConnectContext>(options =>
    {
        var cs = builder.Configuration.GetConnectionString("PConnect");
        if (string.IsNullOrWhiteSpace(cs))
        {
            Log.Fatal("ConnectionStrings:DocumentBuilder is null/empty.");
            throw new InvalidOperationException("ConnectionStrings:DocumentBuilder is null/empty.");
        }

        options.UseSqlServer(cs);
    });



    //Injections of dependencies
    builder.Services.AddScoped<ITableDummyService, TableDummyService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<ISincronizarCotizacion, SincronizarCotizacionService>();
    builder.Services.AddScoped<ISincronizacionMultipleService, SincronizacionMultipleService>();

    // Registrar Background Service (solo si esta habilitado)
    var habilitado = builder.Configuration.GetValue<bool>("SincronizacionAutomatica:Habilitado", false);
    if (habilitado)
    {
        builder.Services.AddHostedService<SincronizacionBackgroundService>();
    }

    //Validators
    builder.Services.AddScoped<TableDummyValidator>();

    //Repositories
    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    builder.Services.AddScoped<TableDummyRepository>();
    builder.Services.AddScoped<ICotizacionOrigenRepository, CotizacionOrigenRepository>();
    builder.Services.AddScoped<ICotizacionLegacyRepository, CotizacionLegacyRepository>();
    builder.Services.AddScoped<ICotizacionControlRepository, CotizacionControlRepository>();
    builder.Services.AddScoped<IPartidaCotizacionOrigenRepository, PartidaCotizacionOrigenRepository>();
    builder.Services.AddScoped<IPartidaCotizacionLegacyRepository, PartidaCotizacionLegacyRepository>();

    //FluentValidation
    builder.Services.AddFluentValidationAutoValidation(); // Para ASP.NET Core
    builder.Services.AddValidatorsFromAssemblyContaining<TableDummyDtoFluetValidator>();

    //Autopper profiles
    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile<ApplicationMappingProfile>();
        cfg.AddProfile<DomainMappingProfile>();
        cfg.AddProfile<CotizaMappingProfile>();
        cfg.AddProfile<RepositoryMappingProfile>();
    });


    // Customize 'ProblemDetails' for validation errors
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = ProblemDetailsHelper.CreateProblemDetails(
                context.HttpContext,
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                context.ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                )
            );

            return new BadRequestObjectResult(problem);
        };
    });

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
