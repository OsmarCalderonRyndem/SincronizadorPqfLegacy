using SincronizadorPqfLegacy.Application.Interfaces;

namespace SincronizadorPqfLegacy.API.BackgroundServices;

public class SincronizacionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SincronizacionBackgroundService> _logger;
    private readonly TimeSpan _intervalo;

    public SincronizacionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<SincronizacionBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Leer intervalo desde appsettings.json (en minutos)
        var intervaloMinutos = configuration.GetValue<int>("SincronizacionAutomatica:IntervaloMinutos", 60);
        _intervalo = TimeSpan.FromMinutes(intervaloMinutos);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SincronizacionBackgroundService iniciado. Intervalo: {Intervalo}", _intervalo);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Ejecutando sincronizacion automatica...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var sincronizacionService = scope.ServiceProvider
                        .GetRequiredService<ISincronizacionMultipleService>();

                    var resultado = await sincronizacionService.SincronizarPendientesAsync();

                    _logger.LogInformation("Sincronizacion automatica completada. Total: {Total}, Exitosos: {Exitosos}, Fallidos: {Fallidos}",
                        resultado.TotalPendientes,
                        resultado.Exitosos,
                        resultado.Fallidos);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en sincronizacion automatica");
            }

            _logger.LogInformation("Siguiente ejecucion en: {Intervalo}", _intervalo);
            await Task.Delay(_intervalo, stoppingToken);
        }

        _logger.LogInformation("SincronizacionBackgroundService detenido");
    }
}