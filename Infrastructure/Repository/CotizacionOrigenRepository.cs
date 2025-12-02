using AutoMapper;
using Infrastructure.Persistence.PConnect.Contexts;
using Infrastructure.Persistence.PConnectProquifaDotNet.Contexts;
using Infrastructure.Persistence.ProquifaDotNet.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SincronizadorPqfLegacy.Domain.DTOs;
using SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

namespace SincronizadorPqfLegacy.Infrastructure.Repositories;

/// <summary>
/// Repositorio para acceso a cotizaciones del sistema origen (ProquifaDotNet)
/// Implementa operaciones de solo lectura
/// </summary>
public class CotizacionOrigenRepository : ICotizacionOrigenRepository
{
    private readonly ProquifaDotNetContext _proquifaContext;
    private readonly PConnectProquifaDotNetContext _pconnectProquifaContext;
    private readonly IMapper _mapper;
    private readonly ILogger<CotizacionOrigenRepository> _logger;

    public CotizacionOrigenRepository(
        ProquifaDotNetContext context,
        PConnectProquifaDotNetContext pcpcontext,
        IMapper mapper,
        ILogger<CotizacionOrigenRepository> logger)
    {
        _proquifaContext = context;
        _pconnectProquifaContext = pcpcontext;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una cotización transformada por su ID
    /// </summary>
    public async Task<CotizacionOrigenDto?> ObtenerPorIdAsync(Guid idCotizacion)
    {
        try
        {
            _logger.LogDebug("Buscando cotización origen: {Id}", idCotizacion);

            // Query a la vista
            var entidad = await _proquifaContext.vCotizacionesTransformadasETLs
                .AsNoTracking() // Solo lectura, no tracking
                .Where(c => c.IdCotCotizacion == idCotizacion)
                .FirstOrDefaultAsync();

            if (entidad == null)
            {
                _logger.LogWarning("Cotización origen no encontrada: {Id}", idCotizacion);
                return null;
            }

            // Mapear Entity → DTO
            var dto = _mapper.Map<CotizacionOrigenDto>(entidad);

            _logger.LogDebug("Cotización origen encontrada: {Clave}", dto.Clave);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización origen: {Id}", idCotizacion);
            throw;
        }
    }

    public Task<List<Guid>> SincronizarCotizacionesPQF2Pendientes()
    {
        try
        {

            // 1. Obtener últimos registros por IdentificadorRegistro (en PConnect)
            var fallosPersistentes = _pconnectProquifaContext.SyncJobLogs
                .GroupBy(s => s.IdentificadorRegistro)
                .Select(g => g
                    .OrderByDescending(x => x.FechaRegistro)
                    .FirstOrDefault()) // último registro
                .Where(x => x.Estado == "Fallo persistente")
                .Select(x => x.IdentificadorRegistro)
                .ToList();   // lista de cotizaciones con fallo persistente


            //// 2. Consulta principal en ProquifaNet, excluyendo las anteriores
            //var resultado = proquifaContext.vcotCotizacion
            //    .Where(cc =>
            //        cc.Folio != "" &&
            //        cc.Folio != null &&
            //        cc.TotalProductos > 0 &&
            //        cc.Activo == 1 &&
            //        !fallosPersistentes.Contains(cc.IdCotCotizacion)
            //    )
            //    .Join(
            //        proquifaContext.catEstadoCotizacion
            //            .Where(e => e.Clave == "finalizada" || e.Clave == "enviada"),
            //        cc => cc.IdCatEstadoCotizacion,
            //        est => est.IdCatEstadoCotizacion,
            //        (cc, est) => new { cc, est }
            //    )
            //    .Join(
            //        proquifaContext.Empresa
            //            .Where(e => (e.FacturaServicios ?? 0) == 0),
            //        x => x.cc.IdEmpresa,
            //        emp => emp.IdEmpresa,
            //        (x, emp) => new { x.cc, x.est, emp }
            //    )
            //    .GroupJoin(
            //        pconnectContext.Cotizaciones,
            //        x => x.cc.IdCotCotizacion,
            //        cot => cot.CotizacionPQF,
            //        (x, cotList) => new { x.cc, TieneCotizacion = cotList.Any() }
            //    )
            //    .Where(x => !x.TieneCotizacion)  // equivalente a Cotizaciones.CotizacionPQF IS NULL
            //    .Select(x => x.cc.IdCotCotizacion)
            //    .ToList();






            var listaId = new List<Guid>
            {
                Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851"),
                Guid.Parse("c4a760a4-8f5b-4d3c-9a3e-2f4d7f8e9b12"),
                Guid.Parse("e1cbb0c5-3f5b-4d2a-8a3e-1f2d3c4b5a6b")
            };
            return Task.FromResult(listaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones pendientes en ProquifaNet2");
            throw;
        }
    }
}