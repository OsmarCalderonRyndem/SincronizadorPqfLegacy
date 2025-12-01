using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SincronizadorPqfLegacy.Application.Factorys;
using SincronizadorPqfLegacy.Application.Interfaces;
using SincronizadorPqfLegacy.Application.Services;
using SincronizadorPqfLegacy.Application.Validators;
using SincronizadorPqfLegacy.Domain.Interfaces;
using SincronizadorPqfLegacy.Infrastructure.Mappers;
using SincronizadorPqfLegacy.Infrastructure.Persistence;
using SincronizadorPqfLegacy.Infrastructure.Persistence.Context;
using SincronizadorPqfLegacy.Infrastructure.Repository;
using System.Reflection;

namespace SincronizadorPqfLegacy.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureSerilog(this IHostBuilder host)
        {
            host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.WithThreadId()
                .Enrich.FromLogContext());
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
        }

        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
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
        }

        public static void ConfigureApplicationServices(this IServiceCollection services)
        {
            // Injections of dependencies
            services.AddScoped<ITableDummyService, TableDummyService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Validators
            services.AddScoped<TableDummyValidator>();

            // Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<TableDummyRepository>();

            // FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<TableDummyDtoFluetValidator>();

            // AutoMapper profiles
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ApplicationMappingProfile>();
                cfg.AddProfile<DomainMappingProfile>();
            });
        }

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
    }
}
