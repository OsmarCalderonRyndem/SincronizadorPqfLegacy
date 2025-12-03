using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Console;
using Infrastructure.Persistence.PConnect.Contexts;
using Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;
using Infrastructure.Persistence.ProquifaDotNet.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SincronizadorPqfLegacy.API.BackgroundServices;
using SincronizadorPqfLegacy.Application.Factorys;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Application.Services;
using SincronizadorPqfLegacy.Domain.Interfaces;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;
using SincronizadorPqfLegacy.Domain.Interfaces.Repository;
using SincronizadorPqfLegacy.Domain.Models;
using SincronizadorPqfLegacy.Infrastructure.Mappers;
using SincronizadorPqfLegacy.Infrastructure.Persistence;
using SincronizadorPqfLegacy.Infrastructure.Persistence.Context;
using SincronizadorPqfLegacy.Infrastructure.Repositories;
using SincronizadorPqfLegacy.Infrastructure.Repository;
using SincronizadorPqfLegacy.Infrastructure.Services;
using System.Reflection;

namespace SincronizadorPqfLegacy.API.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Configura Serilog como el proveedor de logging de la aplicación.
        /// Lee la configuración desde appsettings.json y enriquece los logs con contexto.
        /// </summary>
        /// <param name="host">El IHostBuilder a configurar.</param>
        public static void ConfigureSerilog(this IHostBuilder host)
        {
            host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.WithThreadId()
                .Enrich.FromLogContext());
        }

        /// <summary>
        /// Configura Swagger para la documentación de la API.
        /// Incluye los comentarios XML del ensamblado para enriquecer la documentación.
        /// </summary>
        /// <param name="services">La colección de servicios.</param>
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
        }

        /// <summary>
        /// Configura los contextos de base de datos de la aplicación.
        /// Registra MicroservicioContext, ProquifaDotNetContext, PConnectProquifaDotNetContext y PConnectContext.
        /// </summary>
        /// <param name="services">La colección de servicios.</param>
        /// <param name="configuration">La configuración de la aplicación.</param>
        public static void ConfigureDatabases(this IServiceCollection services, IConfiguration configuration)
        {
            // MicroservicioContext
            services.AddDbContext<MicroservicioContext>(options =>
            {
                var cs = configuration.GetConnectionString("DocumentBuilder");
                if (string.IsNullOrWhiteSpace(cs))
                {
                    Log.Fatal("ConnectionStrings:DocumentBuilder is null/empty.");
                    throw new InvalidOperationException("ConnectionStrings:DocumentBuilder is null/empty.");
                }
                options.UseSqlServer(cs);
            });

            // ProquifaDotNetContext
            services.AddDbContext<ProquifaDotNetContext>(options =>
            {
                var cs = configuration.GetConnectionString("ProquifaDotNet");
                if (string.IsNullOrWhiteSpace(cs))
                {
                    Log.Fatal("ConnectionStrings:ProquifaDotNet is null/empty.");
                    throw new InvalidOperationException("ConnectionStrings:ProquifaDotNet is null/empty.");
                }
                options.UseSqlServer(cs);
            });

            // PConnectProquifaDotNetContext
            services.AddDbContext<PConnectProquifaDotNetContext>(options =>
            {
                var cs = configuration.GetConnectionString("PConnectProquifaDotNet");
                if (string.IsNullOrWhiteSpace(cs))
                {
                    Log.Fatal("ConnectionStrings:PConnectProquifaDotNet is null/empty.");
                    throw new InvalidOperationException("ConnectionStrings:PConnectProquifaDotNet is null/empty.");
                }
                options.UseSqlServer(cs);
            });

            // PConnectContext
            services.AddDbContext<PConnectContext>(options =>
            {
                var cs = configuration.GetConnectionString("PConnect");
                if (string.IsNullOrWhiteSpace(cs))
                {
                    Log.Fatal("ConnectionStrings:PConnect is null/empty.");
                    throw new InvalidOperationException("ConnectionStrings:PConnect is null/empty.");
                }
                options.UseSqlServer(cs);
            });
        }

        /// <summary>
        /// Configura los servicios de la aplicación, repositorios, validadores y AutoMapper.
        /// </summary>
        /// <param name="services">La colección de servicios.</param>
        public static void ConfigureApplicationServices(this IServiceCollection services)
        {
            // Injections of dependencies

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Hangfire Job Services
            services.AddScoped<ISincronizacionJobService, SincronizacionJobService>();

            // ETL Metadata Service
            services.AddSingleton<ProcesoEtlMetadataService>();

            // Validators


            // Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ISyncJobLogRepository, SyncJobLogRepository>(); // Repositorio específico con DTOs


            // FluentValidation
            services.AddFluentValidationAutoValidation();


            // AutoMapper profiles
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ApplicationMappingProfile>();
                cfg.AddProfile<SyncJobLogMappingProfile>();
            });
        }

        /// <summary>
        /// Configura los servicios específicos para el proceso ETL de sincronización.
        /// Incluye servicios de negocio, repositorios específicos y mapeos adicionales.
        /// </summary>
        /// <param name="services">La colección de servicios.</param>
        /// <param name="configuration">La configuración de la aplicación.</param>
        public static void ConfigureETLServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Servicios de sincronización
            services.AddScoped<ISincronizarCotizacion, SincronizarCotizacionService>();
            services.AddScoped<ISincronizacionMultipleService, SincronizacionMultipleService>();
            services.AddScoped<ISincronizarPartidasService, SincronizarPartidasService>();

            // Repositorios ETL
            services.AddScoped<ICotizacionOrigenRepository, CotizacionOrigenRepository>();
            services.AddScoped<ICotizacionLegacyRepository, CotizacionLegacyRepository>();
            services.AddScoped<ICotizacionControlRepository, CotizacionControlRepository>();
            services.AddScoped<IPartidaCotizacionOrigenRepository, PartidaCotizacionOrigenRepository>();
            services.AddScoped<IPartidaCotizacionLegacyRepository, PartidaCotizacionLegacyRepository>();
            services.AddScoped<ISincronizacionJobService, SincronizacionJobService>();

            // AutoMapper profiles adicionales
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<CotizaMappingProfile>();
                cfg.AddProfile<RepositoryMappingProfile>();
            });

            // Background Service (solo si está habilitado)
            var habilitado = configuration.GetValue<bool>("SincronizacionAutomatica:Habilitado", false);
            if (habilitado)
            {
                services.AddHostedService<SincronizacionBackgroundService>();
            }
        }

        /// <summary>
        /// Configura el comportamiento de la API para manejar errores de validación.
        /// Personaliza la respuesta de BadRequest para devolver un ProblemDetails estandarizado.
        /// </summary>
        /// <param name="services">La colección de servicios.</param>
        public static void ConfigureProblemDetails(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
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
        }

        /// <summary>
        /// Configura Hangfire para el procesamiento de tareas en segundo plano.
        /// Establece el almacenamiento en SQL Server y configura la política de reintentos.
        /// </summary>
        /// <param name="services">La colección de servicios.</param>
        /// <param name="configuration">La configuración de la aplicación.</param>
        public static void ConfigureHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            // Leer configuración de reintentos
            var retryAttempts = configuration.GetValue<int>("Hangfire:RetryAttempts", 5);
            var retryDelays = configuration.GetSection("Hangfire:RetryDelays").Get<int[]>()
                ?? [60, 300, 900, 3600, 7200]; // Valores por defecto en segundos

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("PConnectProquifaDotNet"))
                .UseConsole() // Habilita Hangfire.Console para barras de progreso y logs con colores
                .UseFilter(new AutomaticRetryAttribute
                {
                    Attempts = retryAttempts,
                    DelaysInSeconds = retryDelays
                }));

            services.AddHangfireServer();

            // Registrar opciones de limpieza de logs
            services.Configure<SyncLogCleanupOptions>(
                configuration.GetSection(SyncLogCleanupOptions.SectionName));

            // Register custom services for Error Handling
            services.AddScoped<IExceptionClassifier, ExceptionClassifier>();
            services.AddScoped<ISyncLogService, SyncLogService>();
            services.AddScoped<SyncLogCleanupService>();
        }

        /// <summary>
        /// Configura los jobs recurrentes de Hangfire.
        /// Debe llamarse después de app.Build() en Program.cs.
        /// </summary>
        /// <param name="app">El constructor de la aplicación.</param>
        /// <param name="configuration">La configuración de la aplicación.</param>
        public static void ConfigureRecurringJobs(this IApplicationBuilder app, IConfiguration configuration)
        {
            var cleanupOptions = configuration.GetSection(SyncLogCleanupOptions.SectionName)
                .Get<SyncLogCleanupOptions>() ?? new SyncLogCleanupOptions();

            // Leer configuración de sincronización automática
            var syncPendientesCron = configuration.GetValue<string>("SincronizacionAutomatica:CronExpression", "*/30 * * * *"); // Cada 30 minutos por defecto
            var sinDetonacionCron = configuration.GetValue<string>("SincronizacionAutomatica:SinDetonacionCron", "*/30 * * *"); // Cada 30 minutos por defecto

            // Obtener el gestor de jobs recurrentes del contenedor de servicios
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

                // 1. Job recurrente para limpieza de logs
                if (cleanupOptions.Habilitado)
                {
                    recurringJobManager.AddOrUpdate<SyncLogCleanupService>(
                        "cleanup-sync-logs",
                        service => service.LimpiarLogsAntiguos(),
                        cleanupOptions.CronExpression,
                        new RecurringJobOptions
                        {
                            TimeZone = TimeZoneInfo.Utc
                        });
                }
                else
                {
                    recurringJobManager.RemoveIfExists("cleanup-sync-logs");
                }

                // 2. Job recurrente para sincronización automática de pendientes
                recurringJobManager.AddOrUpdate<SincronizacionJobService>(
                    "sincronizar-pendientes-automatico",
                    service => service.EjecutarSincronizacionPendientesRecurrente(null),
                    syncPendientesCron,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.Local
                    });

                // 3. Job recurrente para procesos sin detonación inicial (placeholder)
                recurringJobManager.AddOrUpdate<SincronizacionJobService>(
                    "procesos-sin-detonacion-inicial",
                    service => service.EjecutarProcesosSinDetonacionInicial(null),
                    sinDetonacionCron,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.Local
                    });
            }
        }
    }
}
