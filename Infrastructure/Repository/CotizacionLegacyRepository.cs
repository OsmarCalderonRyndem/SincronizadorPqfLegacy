using AutoMapper;
using Infrastructure.Persistence.PConnect.Contexts;
using Infrastructure.Persistence.PConnect.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

namespace SincronizadorPqfLegacy.Infrastructure.Repositories;

/// <summary>
/// Repositorio para acceso a cotizaciones del sistema legacy (PConnect)
/// Implementa operaciones de lectura y escritura
/// </summary>
public class CotizacionLegacyRepository : ICotizacionLegacyRepository
{
    private readonly PConnectContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CotizacionLegacyRepository> _logger;

    public CotizacionLegacyRepository(
        PConnectContext context,
        IMapper mapper,
        ILogger<CotizacionLegacyRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region Consultas

    /// <summary>
    /// Obtiene una cotización por su clave
    /// </summary>
    public async Task<CotizacionLegacyDto?> ObtenerPorClaveAsync(string clave)
    {
        try
        {
            _logger.LogDebug("Buscando cotización legacy por clave: {Clave}", clave);

            var entidad = await _context.Cotizas
                .AsNoTracking()
                .Where(c => c.Clave == clave)
                .FirstOrDefaultAsync();

            if (entidad == null)
            {
                _logger.LogDebug("Cotización legacy no encontrada: {Clave}", clave);
                return null;
            }

            var dto = _mapper.Map<CotizacionLegacyDto>(entidad);

            _logger.LogDebug("Cotización legacy encontrada: {Clave}, PK: {PK}", clave, dto.PK_Folio);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización legacy por clave: {Clave}", clave);
            throw;
        }
    }

    /// <summary>
    /// Obtiene una cotización por su PK_Folio
    /// </summary>
    public async Task<CotizacionLegacyDto?> ObtenerPorPKAsync(int pkFolio)
    {
        try
        {
            _logger.LogDebug("Buscando cotización legacy por PK: {PK}", pkFolio);

            var entidad = await _context.Cotizas
                .AsNoTracking()
                .Where(c => c.PK_Folio == pkFolio)
                .FirstOrDefaultAsync();

            if (entidad == null)
            {
                _logger.LogDebug("Cotización legacy no encontrada: PK {PK}", pkFolio);
                return null;
            }

            var dto = _mapper.Map<CotizacionLegacyDto>(entidad);

            _logger.LogDebug("Cotización legacy encontrada: PK {PK}, Clave: {Clave}", pkFolio, dto.Clave);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización legacy por PK: {PK}", pkFolio);
            throw;
        }
    }

    #endregion

    #region Comandos (Insert/Update)

    /// <summary>
    /// Inserta una nueva cotización en legacy
    /// </summary>
    public async Task<CotizacionLegacyDto> InsertarAsync(CotizacionLegacyDto cotizacion)
    {
        try
        {
            _logger.LogInformation("Insertando cotización legacy: {Clave}", cotizacion.Clave);

            // Mapear DTO → Entity
            var entidad = _mapper.Map<Cotiza>(cotizacion);

            // Insertar
            await _context.Cotizas.AddAsync(entidad);
            await _context.SaveChangesAsync();

            // Mapear Entity → DTO (ahora con PK_Folio asignado)
            var dtoInsertado = _mapper.Map<CotizacionLegacyDto>(entidad);

            _logger.LogInformation("✓ Cotización legacy insertada: {Clave}, PK: {PK}",
                dtoInsertado.Clave,
                dtoInsertado.PK_Folio);

            return dtoInsertado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar cotización legacy: {Clave}", cotizacion.Clave);
            throw;
        }
    }

    /// <summary>
    /// Actualiza una cotización existente
    /// </summary>
    public async Task<CotizacionLegacyDto> ActualizarAsync(CotizacionLegacyDto cotizacion)
    {
        try
        {
            _logger.LogInformation("Actualizando cotización legacy: {Clave}, PK: {PK}",
                cotizacion.Clave,
                cotizacion.PK_Folio);

            // Buscar la entidad existente
            var entidadExistente = await _context.Cotizas
                .Where(c => c.PK_Folio == cotizacion.PK_Folio)
                .FirstOrDefaultAsync();

            if (entidadExistente == null)
            {
                throw new InvalidOperationException(
                    $"No se puede actualizar. Cotización con PK {cotizacion.PK_Folio} no existe.");
            }

            // Mapear DTO → Entity (actualiza los valores)
            _mapper.Map(cotizacion, entidadExistente);

            // Actualizar
            _context.Cotizas.Update(entidadExistente);
            await _context.SaveChangesAsync();

            // Mapear Entity → DTO
            var dtoActualizado = _mapper.Map<CotizacionLegacyDto>(entidadExistente);

            _logger.LogInformation("✓ Cotización legacy actualizada: {Clave}, PK: {PK}",
                dtoActualizado.Clave,
                dtoActualizado.PK_Folio);

            return dtoActualizado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cotización legacy: PK {PK}", cotizacion.PK_Folio);
            throw;
        }
    }

    #endregion
}