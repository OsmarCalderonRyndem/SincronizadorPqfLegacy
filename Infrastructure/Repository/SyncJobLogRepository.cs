using AutoMapper;
using Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;
using Infrastructure.Persistence.PConnectProquifaDotNet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

namespace SincronizadorPqfLegacy.Infrastructure.Repository;

/// <summary>
/// Repositorio para logs de trabajos de sincronización
/// Usa DTOs para todas las operaciones externas
/// Base de datos: PConnectProquifaDotNet
/// </summary>
public class SyncJobLogRepository : ISyncJobLogRepository
{
    private readonly PConnectProquifaDotNetContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SyncJobLogRepository> _logger;

    public SyncJobLogRepository(
        PConnectProquifaDotNetContext context,
        IMapper mapper,
        ILogger<SyncJobLogRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Inserta un nuevo registro de log
    /// </summary>
    public async Task<SyncJobLogDto> InsertarAsync(SyncJobLogDto dto)
    {
        try
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            _logger.LogDebug("Insertando log: Entidad={Entidad}, ID={Id}, Estado={Estado}",
                dto.NombreEntidad, dto.IdentificadorRegistro, dto.Estado);

            // Mapear DTO -> Entity
            var entidad = _mapper.Map<SyncJobLog>(dto);

            // Asegurar valores por defecto
            if (entidad.IdSyncJobLog == Guid.Empty)
                entidad.IdSyncJobLog = Guid.NewGuid();

            if (!entidad.FechaRegistro.HasValue)
                entidad.FechaRegistro = DateTime.Now;

            if (entidad.FechaProcesamiento == default)
                entidad.FechaProcesamiento = DateTime.Now;

            // Insertar
            await _context.SyncJobLogs.AddAsync(entidad);
            await _context.SaveChangesAsync();

            // Mapear Entity -> DTO
            var dtoInsertado = _mapper.Map<SyncJobLogDto>(entidad);

            _logger.LogInformation("Log insertado: IdLog={IdLog}, Entidad={Entidad}, Estado={Estado}",
                dtoInsertado.IdSyncJobLog, dtoInsertado.NombreEntidad, dtoInsertado.Estado);

            return dtoInsertado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar log: Entidad={Entidad}, ID={Id}",
                dto?.NombreEntidad, dto?.IdentificadorRegistro);
            throw;
        }
    }

    /// <summary>
    /// Obtiene un log por ID
    /// </summary>
    public async Task<SyncJobLogDto?> ObtenerPorIdAsync(Guid idSyncJobLog)
    {
        try
        {
            _logger.LogDebug("Buscando log: {Id}", idSyncJobLog);

            var entidad = await _context.SyncJobLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdSyncJobLog == idSyncJobLog);

            if (entidad == null)
            {
                _logger.LogDebug("Log no encontrado: {Id}", idSyncJobLog);
                return null;
            }

            var dto = _mapper.Map<SyncJobLogDto>(entidad);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener log: {Id}", idSyncJobLog);
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los logs de un registro específico
    /// </summary>
    public async Task<IEnumerable<SyncJobLogDto>> ObtenerPorRegistroAsync(
        string nombreEntidad,
        Guid identificadorRegistro)
    {
        try
        {
            _logger.LogDebug("Buscando logs: Entidad={Entidad}, ID={Id}",
                nombreEntidad, identificadorRegistro);

            var entidades = await _context.SyncJobLogs
                .AsNoTracking()
                .Where(x => x.NombreEntidad == nombreEntidad
                         && x.IdentificadorRegistro == identificadorRegistro)
                .OrderByDescending(x => x.FechaRegistro)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<SyncJobLogDto>>(entidades);

            _logger.LogDebug("Encontrados {Count} logs para Entidad={Entidad}, ID={Id}",
                dtos.Count(), nombreEntidad, identificadorRegistro);

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener logs: Entidad={Entidad}, ID={Id}",
                nombreEntidad, identificadorRegistro);
            throw;
        }
    }

    /// <summary>
    /// Elimina logs antiguos (para limpieza)
    /// </summary>
    public async Task<int> EliminarAntiguosAsync(int diasRetencion)
    {
        try
        {
            var fechaLimite = DateTime.Now.AddDays(-diasRetencion);

            _logger.LogInformation("Eliminando logs anteriores a: {Fecha}", fechaLimite);

            var logsAntiguos = await _context.SyncJobLogs
                .Where(x => x.FechaRegistro < fechaLimite)
                .ToListAsync();

            var cantidad = logsAntiguos.Count;

            if (cantidad > 0)
            {
                _context.SyncJobLogs.RemoveRange(logsAntiguos);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Eliminados {Cantidad} logs antiguos", cantidad);
            }

            return cantidad;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar logs antiguos");
            throw;
        }
    }

    /// <summary>
    /// Obtiene el último log de un registro específico
    /// </summary>
    public async Task<SyncJobLogDto?> ObtenerUltimoLogAsync(
        string nombreEntidad,
        Guid identificadorRegistro)
    {
        try
        {
            var entidad = await _context.SyncJobLogs
                .AsNoTracking()
                .Where(x => x.NombreEntidad == nombreEntidad
                         && x.IdentificadorRegistro == identificadorRegistro)
                .OrderByDescending(x => x.FechaRegistro)
                .FirstOrDefaultAsync();

            if (entidad == null)
                return null;

            return _mapper.Map<SyncJobLogDto>(entidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener último log: Entidad={Entidad}, ID={Id}",
                nombreEntidad, identificadorRegistro);
            throw;
        }
    }
}
