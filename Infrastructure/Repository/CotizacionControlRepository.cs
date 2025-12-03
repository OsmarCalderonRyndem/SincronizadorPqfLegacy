using AutoMapper;
using Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;
using Infrastructure.Persistence.PConnectProquifaDotNet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

namespace SincronizadorPqfLegacy.Infrastructure.Repositories;

/// <summary>
/// Repositorio para la tabla de control de sincronización
/// Maneja el registro y seguimiento del proceso de sincronización
/// NOTA: La tabla tiene Primary Key (IdCotizacion)
/// Base de datos: PConnectProquifaDotNet
/// </summary>
public class CotizacionControlRepository : ICotizacionControlRepository
{
    private readonly PConnectProquifaDotNetContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CotizacionControlRepository> _logger;

    public CotizacionControlRepository(
        PConnectProquifaDotNetContext context,
        IMapper mapper,
        ILogger<CotizacionControlRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region Consultas

    /// <summary>
    /// Obtiene un registro de control por ID de cotización PQF
    /// </summary>
    public async Task<CotizacionControlDto?> ObtenerPorIdAsync(Guid idCotizacion)
    {
        try
        {
            _logger.LogDebug("Buscando registro de control: {Id}", idCotizacion);

            var entidad = await _context.Cotizaciones
                .AsNoTracking() // Solo lectura
                .Where(c => c.CotizacionPQF == idCotizacion)
                .FirstOrDefaultAsync();

            if (entidad == null)
            {
                _logger.LogDebug("Registro de control no encontrado: {Id}", idCotizacion);
                return null;
            }

            var dto = _mapper.Map<CotizacionControlDto>(entidad);

            _logger.LogDebug("Registro de control encontrado: CotizacionPQF={PQF}, Folio={Folio}",
                idCotizacion,
                dto.Folio);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registro de control: {Id}", idCotizacion);
            throw;
        }
    }

    /// <summary>
    /// Obtiene cotizaciones pendientes de sincronizar
    /// </summary>
    public async Task<IEnumerable<CotizacionControlDto>> ObtenerPendientesSincronizacionAsync()
    {
        try
        {
            _logger.LogDebug("Buscando registros pendientes de sincronización");

            // 1. Obtener lista de identificadores con fallo persistente
            // Primero obtenemos todos los registros más recientes por identificador
            var ultimosEstadosPorIdentificador = await _context.SyncJobLogs
                .GroupBy(s => s.IdentificadorRegistro)
                .Select(g => new
                {
                    IdentificadorRegistro = g.Key,
                    UltimoEstado = g.OrderByDescending(x => x.FechaRegistro)
                                    .Select(x => x.Estado)
                                    .FirstOrDefault()
                })
                .ToListAsync();

            // Filtrar en memoria los que tienen fallo persistente
            var fallosPersistentes = ultimosEstadosPorIdentificador
                .Where(x => x.UltimoEstado == "Fallo persistente")
                .Select(x => x.IdentificadorRegistro)
                .ToList();

            // 2. Consulta principal, excluyendo esos identificadores
            var entidades = await _context.Cotizaciones
                .AsNoTracking()
                .Where(c =>
                    (c.RegistroCompleto == false || c.CotizacionLegacy == null) &&
                    !fallosPersistentes.Contains(c.CotizacionPQF ?? Guid.Empty)   // << discriminación
                )
                .ToListAsync();


            var dtos = _mapper.Map<IEnumerable<CotizacionControlDto>>(entidades);

            _logger.LogInformation("Encontrados {Count} registros pendientes de sincronización",
                dtos.Count());

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registros pendientes de sincronización");
            throw;
        }
    }

    #endregion

    #region Comandos (Insert/Update)

    /// <summary>
    /// Inserta un nuevo registro de control
    /// </summary>
    public async Task<CotizacionControlDto> InsertarAsync(CotizacionControlDto cotizacion)
    {
        try
        {
            _logger.LogInformation("Insertando registro de control: CotizacionPQF={Id}, Folio={Folio}",
                cotizacion.CotizacionPQF,
                cotizacion.Folio);

            // Mapear DTO → Entity
            var entidad = _mapper.Map<Cotizacione>(cotizacion);

            // Asegurar que las fechas estén establecidas
            entidad.FechaRegistro = DateTime.Now;
            entidad.FechaUltimaActualizacion = DateTime.Now;
            entidad.Insertado = true;
            entidad.Actualizado = false;

            // Insertar
            await _context.Cotizaciones.AddAsync(entidad);
            await _context.SaveChangesAsync();

            // Mapear Entity → DTO (ahora con IdCotizacion asignado)
            var dtoInsertado = _mapper.Map<CotizacionControlDto>(entidad);

            _logger.LogInformation("Registro de control insertado: CotizacionPQF={PQF}, Folio={Folio}",
                dtoInsertado.CotizacionPQF,
                dtoInsertado.Folio);

            return dtoInsertado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar registro de control: CotizacionPQF={Id}",
                cotizacion.CotizacionPQF);
            throw;
        }
    }

    /// <summary>
    /// Actualiza un registro de control existente
    /// </summary>
    public async Task<CotizacionControlDto> ActualizarAsync(CotizacionControlDto cotizacion)
    {
        try
        {
            _logger.LogInformation("Actualizando registro de control: IdCotizacion={IdCot}, CotizacionPQF={PQF}, Folio={Folio}",
                cotizacion.CotizacionPQF,
                cotizacion.CotizacionPQF,
                cotizacion.Folio);


            // Buscar la entidad existente por PK
            var entidadExistente = await _context.Cotizaciones
                .Where(c => c.CotizacionPQF == cotizacion.CotizacionPQF)
                .FirstOrDefaultAsync();

            if (entidadExistente == null)
            {
                throw new InvalidOperationException(
                    $"No se puede actualizar. Registro con IdCotizacion {cotizacion.CotizacionPQF} no existe.");
            }

            var idAux = entidadExistente.IdMapeoProquifaLegacy;
            // Mapear DTO → Entity (actualiza los valores)
            _mapper.Map(cotizacion, entidadExistente);


            // Actualizar campos de auditoría
            entidadExistente.FechaUltimaActualizacion = DateTime.Now;
            entidadExistente.Actualizado = true;
            entidadExistente.IdMapeoProquifaLegacy = idAux;

            // Actualizar en BD
            _context.Cotizaciones.Update(entidadExistente);
            await _context.SaveChangesAsync();

            // Mapear Entity → DTO
            var dtoActualizado = _mapper.Map<CotizacionControlDto>(entidadExistente);

            _logger.LogInformation("Registro de control actualizado: IdCotizacion={IdCot}, CotizacionPQF={PQF}",
                dtoActualizado.CotizacionPQF,
                dtoActualizado.CotizacionPQF);

            return dtoActualizado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar registro de control: IdCotizacion={IdCot}, CotizacionPQF={PQF}",
                cotizacion.CotizacionPQF,
                cotizacion.CotizacionPQF);
            throw;
        }
    }

    #endregion
}