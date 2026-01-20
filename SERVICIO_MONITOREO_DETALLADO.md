# Diseño Detallado del Servicio de Monitoreo

**Propósito**: Servicio unificado para consultar registros de sincronización con vista SQL optimizada
**Fecha**: 2025-12-19
**Versión**: 2.0

---

## 🎯 Objetivo del Servicio

Crear un servicio genérico que permita:
1. Consultar registros de sincronización por ID sin importar el tipo de entidad
2. Aplicar filtros flexibles por múltiples criterios
3. Unificar información de distintas tablas de control
4. Soportar paginación y ordenamiento
5. Proporcionar métricas agregadas en tiempo real

---

## 🗄️ Vista SQL Unificada

### Vista Principal: `vSyncJobLogUnificado`

```sql
CREATE VIEW vSyncJobLogUnificado AS
-- ==========================================
-- Vista unificada de sincronización
-- Combina EtlProcesoControl + SyncJobLog + Tablas específicas
-- ==========================================

SELECT 
    -- Campos base de EtlProcesoControl
    epc.Id AS ProcesoControlId,
    epc.TipoProceso,
    CASE epc.TipoProceso
        WHEN 1 THEN 'Cotizacion'
        WHEN 2 THEN 'Pedido'
        WHEN 3 THEN 'Factura'
        WHEN 4 THEN 'Inventario'
        ELSE 'Desconocido'
    END AS TipoProcesoNombre,
    epc.RecordId,
    epc.RecordId AS ProcessId,  -- ID unificado para consultas externas
    epc.Estado,
    epc.MensajeError,
    epc.FechaInicio,
    epc.FechaFin,
    epc.DuracionMilisegundos,
    epc.SubprocesoActual,
    epc.Reintentos,
    epc.FechaUltimoReintento,
    epc.Metadata,
    
    -- Campos de SyncJobLog
    sjl.Id AS SyncJobLogId,
    sjl.TipoProcesoEtl,
    sjl.Estado AS EstadoJobLog,
    sjl.Mensaje AS MensajeJobLog,
    sjl.FechaCreacion,
    sjl.JobId,
    sjl.FechaEjecucion,
    sjl.Duracion,
    sjl.Excepcion,
    sjl.SnapshotJson AS SnapshotJson,
    sjl.TipoSnapshot AS TipoSnapshot,
    
    -- Información de operación (INSERT/UPDATE)
    CASE 
        WHEN sjl.Mensaje LIKE '%INSERT%' THEN 'INSERT'
        WHEN sjl.Mensaje LIKE '%UPDATE%' THEN 'UPDATE'
        ELSE 'UNKNOWN'
    END AS TipoOperacion,
    
    -- Campos de tablas específicas (LEFT JOIN)
    -- Cotizaciones
    cot.CotizacionPQF AS IdentificadorCotizacion,
    cot.Folio AS FolioCotizacion,
    cot.RegistroCompleto AS CompletadoCotizacion,
    
    -- Pedidos (ejemplo futuro)
    NULL AS IdentificadorPedido,
    NULL AS FolioPedido,
    NULL AS CompletadoPedido,
    
    -- Facturas (ejemplo futuro)
    NULL AS IdentificadorFactura,
    NULL AS FolioFactura,
    NULL AS CompletadoFactura,
    
    -- Campos calculados
    CASE 
        WHEN epc.FechaFin IS NULL THEN NULL
        ELSE DATEDIFF(SECOND, epc.FechaInicio, epc.FechaFin)
    END AS DuracionSegundos,
    
    CASE epc.Estado
        WHEN 'Completado' THEN 1
        WHEN 'EnProcesoInicio' THEN 2
        WHEN 'EnProcesoValidacionesCorrectas' THEN 3
        WHEN 'EnProcesoExtract' THEN 4
        WHEN 'EnProcesoTransform' THEN 5
        WHEN 'EnProcesoLoad' THEN 6
        WHEN 'FalloPersistente' THEN 7
        ELSE 8
    END AS EstadoOrden,
    
    -- Clasificación para filtros rápidos
    CASE 
        WHEN epc.Estado IN ('Completado') THEN 'Exitosos'
        WHEN epc.Estado LIKE 'EnProceso%' THEN 'EnProceso'
        WHEN epc.Estado = 'FalloPersistente' THEN 'ConErrores'
        ELSE 'Otros'
    END AS CategoriaEstado,
    
    -- Flag si fue rollback
    CASE 
        WHEN sjl.Mensaje LIKE '%ROLLBACK%' THEN 1
        ELSE 0
    END AS TuvoRollback,
    
    -- Indicador si tiene snapshot
    CASE 
        WHEN sjl.SnapshotJson IS NOT NULL THEN 1
        ELSE 0
    END AS TieneSnapshot
    
FROM EtlProcesoControl epc
LEFT JOIN SyncJobLog sjl ON 
    sjl.IdentificadorRegistro = epc.RecordId AND 
    sjl.TipoProcesoEtl = epc.TipoProceso

LEFT JOIN Cotizacione cot ON 
    cot.CotizacionPQF = epc.RecordId AND 
    epc.TipoProceso = 1

-- LEFT JOIN Pedido ped ON ped.PedidoId = epc.RecordId AND epc.TipoProceso = 2 (futuro)
-- LEFT JOIN Factura fac ON fac.FacturaId = epc.RecordId AND epc.TipoProceso = 3 (futuro)
GO
```

### Vista de Métricas: `vSyncMetricasUnificadas`

```sql
CREATE VIEW vSyncMetricasUnificadas AS
-- ==========================================
-- Vista de métricas agregadas por tipo de proceso
-- ==========================================

SELECT 
    TipoProceso,
    TipoProcesoNombre,
    COUNT(*) AS TotalRegistros,
    SUM(CASE WHEN CategoriaEstado = 'Exitosos' THEN 1 ELSE 0 END) AS Exitosos,
    SUM(CASE WHEN CategoriaEstado = 'ConErrores' THEN 1 ELSE 0 END) AS ConErrores,
    SUM(CASE WHEN CategoriaEstado = 'EnProceso' THEN 1 ELSE 0 END) AS EnProceso,
    SUM(CASE WHEN CategoriaEstado = 'Otros' THEN 1 ELSE 0 END) AS Otros,
    
    -- Métricas de tiempo
    AVG(CASE WHEN DuracionMilisegundos IS NOT NULL THEN DuracionMilisegundos END) AS DuracionPromedioMs,
    MIN(CASE WHEN DuracionMilisegundos IS NOT NULL THEN DuracionMilisegundos END) AS DuracionMinimaMs,
    MAX(CASE WHEN DuracionMilisegundos IS NOT NULL THEN DuracionMilisegundos END) AS DuracionMaximaMs,
    
    -- Métricas de reintentos
    AVG(Reintentos) AS ReintentosPromedio,
    MAX(Reintentos) AS ReintentosMaximo,
    
    -- Métricas de operaciones
    SUM(CASE WHEN TipoOperacion = 'INSERT' THEN 1 ELSE 0 END) AS TotalInserts,
    SUM(CASE WHEN TipoOperacion = 'UPDATE' THEN 1 ELSE 0 END) AS TotalUpdates,
    SUM(CASE WHEN TuvoRollback = 1 THEN 1 ELSE 0 END) AS TotalConRollback,
    
    -- Métricas de snapshots
    SUM(CASE WHEN TieneSnapshot = 1 THEN 1 ELSE 0 END) AS TotalConSnapshot,
    SUM(CASE WHEN TipoSnapshot = 'ERROR_PROCESO' THEN 1 ELSE 0 END) AS TotalSnapshotsError,
    
    -- Timestamps
    MAX(FechaInicio) AS UltimaSincronizacion,
    MIN(FechaInicio) AS PrimeraSincronizacion,
    
    -- Tasas
    CAST(
        SUM(CASE WHEN CategoriaEstado = 'Exitosos' THEN 1 ELSE 0 END) * 100.0 / 
        NULLIF(COUNT(*), 0)
    AS DECIMAL(5,2)) AS TasaExitoPorcentaje,
    
    CAST(
        SUM(CASE WHEN CategoriaEstado = 'ConErrores' THEN 1 ELSE 0 END) * 100.0 / 
        NULLIF(COUNT(*), 0)
    AS DECIMAL(5,2)) AS TasaErrorPorcentaje,

    -- Tasa de Rollback
    CAST(
        SUM(CASE WHEN TuvoRollback = 1 THEN 1 ELSE 0 END) * 100.0 / 
        NULLIF(COUNT(*), 0)
    AS DECIMAL(5,2)) AS TasaRollbackPorcentaje

FROM vSyncJobLogUnificado
GROUP BY TipoProceso, TipoProcesoNombre
GO
```

---

## 🔧 DTOs del Servicio

### MonitoreoFiltrosDto

```csharp
public class MonitoreoFiltrosDto
{
    // Filtro por ID específico (usado para búsqueda individual)
    public Guid? ProcessId { get; set; }
    
    // Filtros por tipo de proceso
    public List<int>? TiposProceso { get; set; }  // [1,2,3]
    public string? TipoProcesoNombre { get; set; }  // "Cotizacion"
    
    // Filtros por estado
    public List<string>? Estados { get; set; }  // ["Completado", "FalloPersistente"]
    public string? CategoriaEstado { get; set; }  // "Exitosos", "ConErrores", "EnProceso"
    
    // Filtros por fechas
    public DateTime? FechaInicioDesde { get; set; }
    public DateTime? FechaInicioHasta { get; set; }
    public DateTime? FechaFinDesde { get; set; }
    public DateTime? FechaFinHasta { get; set; }
    
    // Filtros por duración
    public int? DuracionMinimaMs { get; set; }
    public int? DuracionMaximaMs { get; set; }
    
    // Filtros por operación
    public string? TipoOperacion { get; set; }  // INSERT, UPDATE, UNKNOWN
    public bool? SoloConRollback { get; set; }
    
    // Filtros por snapshots
    public bool? SoloConSnapshot { get; set; }
    public string? TipoSnapshot { get; set; }  // PRE_PROCESO, POST_PROCESO, ERROR_PROCESO
    
    // Filtros por reintentos
    public int? MinimoReintentos { get; set; }
    public int? MaximoReintentos { get; set; }
    
    // Filtros por snapshots
    public bool? SoloConSnapshot { get; set; }
    public string? TipoSnapshot { get; set; }  // PRE_PROCESO, POST_PROCESO, ERROR_PROCESO
    public bool? SoloConRollback { get; set; }
    
    // Filtros por errores
    public bool? SoloConErrores { get; set; }
    public string? MensajeErrorContiene { get; set; }
    
    // Filtros específicos de entidad (ej. folio de cotización)
    public string? Folio { get; set; }
    
    // Paginación
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 50;
    public string? OrdenarPor { get; set; } = "FechaInicio";
    public bool OrdenDescendente { get; set; } = true;
    
    // Métodos de validación
    public bool EsValido()
    {
        if (TamanoPagina < 1 || TamanoPagina > 100)
            return false;
        if (Pagina < 1)
            return false;
        return true;
    }
    
    public Dictionary<string, object> ToParameterDictionary()
    {
        var parametros = new Dictionary<string, object>();
        
        if (ProcessId.HasValue)
            parametros["@ProcessId"] = ProcessId.Value;
        
        if (TiposProceso?.Any() == true)
            parametros["@TiposProceso"] = string.Join(",", TiposProceso);
        
        if (!string.IsNullOrEmpty(TipoProcesoNombre))
            parametros["@TipoProcesoNombre"] = TipoProcesoNombre;
        
        if (Estados?.Any() == true)
            parametros["@Estados"] = string.Join(",", Estados);
        
        if (!string.IsNullOrEmpty(CategoriaEstado))
            parametros["@CategoriaEstado"] = CategoriaEstado;
        
        if (FechaInicioDesde.HasValue)
            parametros["@FechaInicioDesde"] = FechaInicioDesde.Value;
        
        if (FechaInicioHasta.HasValue)
            parametros["@FechaInicioHasta"] = FechaInicioHasta.Value;
        
        if (FechaFinDesde.HasValue)
            parametros["@FechaFinDesde"] = FechaFinDesde.Value;
        
        if (FechaFinHasta.HasValue)
            parametros["@FechaFinHasta"] = FechaFinHasta.Value;
        
        if (DuracionMinimaMs.HasValue)
            parametros["@DuracionMinimaMs"] = DuracionMinimaMs.Value;
        
        if (DuracionMaximaMs.HasValue)
            parametros["@DuracionMaximaMs"] = DuracionMaximaMs.Value;
        
        if (MinimoReintentos.HasValue)
            parametros["@MinimoReintentos"] = MinimoReintentos.Value;
        
        if (MaximoReintentos.HasValue)
            parametros["@MaximoReintentos"] = MaximoReintentos.Value;
        
        if (SoloConErrores.HasValue)
            parametros["@SoloConErrores"] = SoloConErrores.Value;
        
        if (!string.IsNullOrEmpty(MensajeErrorContiene))
            parametros["@MensajeErrorContiene"] = $"%{MensajeErrorContiene}%";
        
        if (!string.IsNullOrEmpty(Folio))
            parametros["@Folio"] = $"%{Folio}%";
        
        if (TipoOperacion != null)
            parametros["@TipoOperacion"] = TipoOperacion;
        
        if (SoloConRollback.HasValue)
            parametros["@SoloConRollback"] = SoloConRollback.Value;
        
        if (SoloConSnapshot.HasValue)
            parametros["@SoloConSnapshot"] = SoloConSnapshot.Value;
        
        if (!string.IsNullOrEmpty(TipoSnapshot))
            parametros["@TipoSnapshot"] = TipoSnapshot;
        
        return parametros;
    }
}
```

### MonitoreoResultadoDto

```csharp
public class MonitoreoResultadoDto
{
    // Metadatos de paginación
    public int TotalRegistros { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalPaginas { get; set; }
    
    // Datos
    public List<MonitoreoRegistroDto> Datos { get; set; } = new();
    
    // Resumen
    public MonitoreoResumenDto Resumen { get; set; } = new();
}

public class MonitoreoRegistroDto
{
    public Guid ProcesoControlId { get; set; }
    public int TipoProceso { get; set; }
    public string TipoProcesoNombre { get; set; } = string.Empty;
    public Guid RecordId { get; set; }  // ID del registro de negocio
    public Guid ProcessId { get; set; }  // ID unificado para consultas (igual a RecordId o ProcesoControlId)
    public string? TipoOperacion { get; set; }  // INSERT, UPDATE, UNKNOWN
    public bool TuvoRollback { get; set; }  // Flag si aplicó rollback
    public bool TieneSnapshot { get; set; }  // Flag si tiene snapshot
    public string? TipoSnapshot { get; set; }  // Tipo de snapshot guardado
    public string Estado { get; set; } = string.Empty;
    public string? MensajeError { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? DuracionMilisegundos { get; set; }
    public string? SubprocesoActual { get; set; }
    public int Reintentos { get; set; }
    public DateTime? FechaUltimoReintento { get; set; }
    public string? Metadata { get; set; }
    
    // Campos específicos de entidad
    public Guid? IdentificadorCotizacion { get; set; }
    public string? FolioCotizacion { get; set; }
    public bool? CompletadoCotizacion { get; set; }
    
    // Campos calculados
    public int? DuracionSegundos { get; set; }
    public string CategoriaEstado { get; set; } = string.Empty;
    public int EstadoOrden { get; set; }
    
    // Propiedades calculadas
    public bool TieneError => !string.IsNullOrEmpty(MensajeError);
    public bool EstaEnProceso => Estado.StartsWith("EnProceso");
    public bool EstaCompletado => Estado == "Completado";
    public bool FalloPersistente => Estado == "FalloPersistente";
}

public class MonitoreoResumenDto
{
    public int Completados { get; set; }
    public int EnProceso { get; set; }
    public int FallosPersistentes { get; set; }
    public int Otros { get; set; }
    public int Total { get; set; }
    
    public decimal TasaExito { get; set; }
    public decimal TasaError { get; set; }
    public decimal TasaEnProceso { get; set; }
    
    // Métricas de tiempo
    public double DuracionPromedioSegundos { get; set; }
    public int DuracionMinimaSegundos { get; set; }
    public int DuracionMaximaSegundos { get; set; }
    
    // Métricas de reintentos
    public double ReintentosPromedio { get; set; }
    public int ReintentosMaximos { get; set; }
    
    // Métricas de operaciones
    public int TotalInserts { get; set; }
    public int TotalUpdates { get; set; }
    public int TotalConRollback { get; set; }
    
    // Métricas de snapshots
    public int TotalConSnapshot { get; set; }
    public int TotalSnapshotsError { get; set; }
}
```

---

## 🎯 Servicio de Monitoreo

### IMonitoreoAvanzadoService

```csharp
public interface IMonitoreoAvanzadoService
{
    Task<MonitoreoResultadoDto> ObtenerRegistrosAsync(MonitoreoFiltrosDto filtros);
    Task<MonitoreoRegistroDto?> ObtenerRegistroPorIdAsync(Guid idRegistro);
    Task<List<MonitoreoMetricasDto>> ObtenerMetricasAsync();
    Task<MonitoreoRegistroDto?> ObtenerUltimoRegistroPorEntidadAsync(int tipoProceso, Guid recordId);
    Task<List<MonitoreoRegistroDto>> ObtenerRegistrosParaReintentarAsync(int limite = 100);
}
```

### MonitoreoAvanzadoService

```csharp
public class MonitoreoAvanzadoService : IMonitoreoAvanzadoService
{
    private readonly IGenericRepository<SyncJobLogUnificado> _repository;
    private readonly ILogger<MonitoreoAvanzadoService> _logger;
    private readonly IMapper _mapper;

    public MonitoreoAvanzadoService(
        IGenericRepository<SyncJobLogUnificado> repository,
        ILogger<MonitoreoAvanzadoService> logger,
        IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<MonitoreoResultadoDto> ObtenerRegistrosAsync(MonitoreoFiltrosDto filtros)
    {
        try
        {
            if (!filtros.EsValido())
                throw new ArgumentException("Parámetros de filtrado inválidos");

            // Construir consulta SQL dinámica
            var sql = ConstruirSqlConsulta(filtros);
            var parametros = filtros.ToParameterDictionary();

            // Obtener total de registros
            var sqlCount = $"SELECT COUNT(*) FROM vSyncJobLogUnificado WHERE {ConstruirSqlWhere(filtros)}";
            var total = await _repository.ExecuteScalarAsync<int>(sqlCount, parametros);

            // Obtener datos paginados
            var datos = await _repository.GetBySqlAsync<SyncJobLogUnificado>(sql, parametros);

            // Calcular resumen
            var resumen = await CalcularResumenAsync(filtros);

            return new MonitoreoResultadoDto
            {
                TotalRegistros = total,
                Pagina = filtros.Pagina,
                TamanoPagina = filtros.TamanoPagina,
                TotalPaginas = (int)Math.Ceiling((double)total / filtros.TamanoPagina),
                Datos = _mapper.Map<List<MonitoreoRegistroDto>>(datos),
                Resumen = resumen
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registros de monitoreo con filtros: {@Filtros}", filtros);
            throw;
        }
    }

    public async Task<MonitoreoRegistroDto?> ObtenerRegistroPorIdAsync(Guid processId)
    {
        try
        {
            var sql = @"
                SELECT * FROM vSyncJobLogUnificado 
                WHERE ProcesoControlId = @ProcessId 
                   OR RecordId = @ProcessId
                ORDER BY FechaInicio DESC";

            var resultado = await _repository.FirstOrDefaultAsync<SyncJobLogUnificado>(sql, new { ProcessId = processId });
            return _mapper.Map<MonitoreoRegistroDto>(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registro por ProcessId: {ProcessId}", processId);
            throw;
        }
    }

    public async Task<MonitoreoRegistroDto?> ObtenerUltimoRegistroPorEntidadAsync(int tipoProceso, Guid processId)
    {
        try
        {
            var sql = @"
                SELECT TOP 1 * FROM vSyncJobLogUnificado 
                WHERE TipoProceso = @TipoProceso AND RecordId = @ProcessId
                ORDER BY FechaInicio DESC";

            var resultado = await _repository.FirstOrDefaultAsync<SyncJobLogUnificado>(
                sql, 
                new { TipoProceso = tipoProceso, ProcessId = processId });
            
            return _mapper.Map<MonitoreoRegistroDto>(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener último registro por entidad: {TipoProceso}, {RecordId}", tipoProceso, recordId);
            throw;
        }
    }

    public async Task<List<MonitoreoRegistroDto>> ObtenerRegistrosParaReintentarAsync(int limite = 100)
    {
        try
        {
            var sql = @"
                SELECT TOP (@Limite) * FROM vSyncJobLogUnificado 
                WHERE CategoriaEstado = 'ConErrores' 
                   AND (Estado != 'FalloPersistente' OR MensajeError LIKE '%timeout%')
                   AND Reintentos < 5
                   AND FechaUltimoReintento < DATEADD(HOUR, -1, GETDATE())
                ORDER BY FechaUltimoReintento ASC";

            var resultados = await _repository.GetBySqlAsync<SyncJobLogUnificado>(
                sql, 
                new { Limite = limite });
            
            return _mapper.Map<List<MonitoreoRegistroDto>>(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registros para reintentar");
            throw;
        }
    }

    private string ConstruirSqlConsulta(MonitoreoFiltrosDto filtros)
    {
        var whereClause = ConstruirSqlWhere(filtros);
        var orderClause = ConstruirSqlOrder(filtros);
        var offset = (filtros.Pagina - 1) * filtros.TamanoPagina;

        return $@"
            SELECT * FROM vSyncJobLogUnificado 
            WHERE {whereClause}
            ORDER BY {orderClause}
            OFFSET {offset} ROWS FETCH NEXT {filtros.TamanoPagina} ROWS ONLY";
    }

    private string ConstruirSqlWhere(MonitoreoFiltrosDto filtros)
    {
        var condiciones = new List<string>();

        if (filtros.IdRegistro.HasValue)
            condiciones.Add("(ProcesoControlId = @IdRegistro OR RecordId = @IdRegistro)");

        if (filtros.TiposProceso?.Any() == true)
            condiciones.Add("TipoProceso IN (SELECT value FROM STRING_SPLIT(@TiposProceso, ','))");

        if (!string.IsNullOrEmpty(filtros.TipoProcesoNombre))
            condiciones.Add("TipoProcesoNombre LIKE @TipoProcesoNombre");

        if (filtros.Estados?.Any() == true)
            condiciones.Add("Estado IN (SELECT value FROM STRING_SPLIT(@Estados, ','))");

        if (!string.IsNullOrEmpty(filtros.CategoriaEstado))
            condiciones.Add("CategoriaEstado = @CategoriaEstado");

        if (filtros.FechaInicioDesde.HasValue)
            condiciones.Add("FechaInicio >= @FechaInicioDesde");

        if (filtros.FechaInicioHasta.HasValue)
            condiciones.Add("FechaInicio <= @FechaInicioHasta");

        if (filtros.FechaFinDesde.HasValue)
            condiciones.Add("FechaFin >= @FechaFinDesde");

        if (filtros.FechaFinHasta.HasValue)
            condiciones.Add("FechaFin <= @FechaFinHasta");

        if (filtros.DuracionMinimaMs.HasValue)
            condiciones.Add("DuracionMilisegundos >= @DuracionMinimaMs");

        if (filtros.DuracionMaximaMs.HasValue)
            condiciones.Add("DuracionMilisegundos <= @DuracionMaximaMs");

        if (filtros.MinimoReintentos.HasValue)
            condiciones.Add("Reintentos >= @MinimoReintentos");

        if (filtros.MaximoReintentos.HasValue)
            condiciones.Add("Reintentos <= @MaximoReintentos");

        if (filtros.SoloConErrores.HasValue && filtros.SoloConErrores.Value)
            condiciones.Add("MensajeError IS NOT NULL");

        if (!string.IsNullOrEmpty(filtros.MensajeErrorContiene))
            condiciones.Add("MensajeError LIKE @MensajeErrorContiene");

        if (!string.IsNullOrEmpty(filtros.Folio))
            condiciones.Add("FolioCotizacion LIKE @Folio");

        return condiciones.Any() ? string.Join(" AND ", condiciones) : "1=1";
    }

    private string ConstruirSqlOrder(MonitoreoFiltrosDto filtros)
    {
        var columna = filtros.OrdenarPor switch
        {
            "FechaInicio" => "FechaInicio",
            "FechaFin" => "FechaFin",
            "Duracion" => "DuracionMilisegundos",
            "Estado" => "EstadoOrden",
            "Reintentos" => "Reintentos",
            "TipoProceso" => "TipoProceso",
            _ => "FechaInicio"
        };

        var direccion = filtros.OrdenDescendente ? "DESC" : "ASC";
        return $"{columna} {direccion}";
    }

    private async Task<MonitoreoResumenDto> CalcularResumenAsync(MonitoreoFiltrosDto filtros)
    {
        var whereClause = ConstruirSqlWhere(filtros);
        var sql = $@"
            SELECT 
                COUNT(*) as Total,
                SUM(CASE WHEN CategoriaEstado = 'Exitosos' THEN 1 ELSE 0 END) as Completados,
                SUM(CASE WHEN CategoriaEstado = 'EnProceso' THEN 1 ELSE 0 END) as EnProceso,
                SUM(CASE WHEN CategoriaEstado = 'ConErrores' THEN 1 ELSE 0 END) as FallosPersistentes,
                SUM(CASE WHEN CategoriaEstado = 'Otros' THEN 1 ELSE 0 END) as Otros,
                AVG(DuracionSegundos) as DuracionPromedioSegundos,
                MIN(DuracionSegundos) as DuracionMinimaSegundos,
                MAX(DuracionSegundos) as DuracionMaximaSegundos,
                AVG(Reintentos) as ReintentosPromedio,
                MAX(Reintentos) as ReintentosMaximos
            FROM vSyncJobLogUnificado 
            WHERE {whereClause}";

        var parametros = filtros.ToParameterDictionary();
        var resultado = await _repository.FirstOrDefaultAsync<ResumenQueryResult>(sql, parametros);

        if (resultado == null) return new MonitoreoResumenDto();

        return new MonitoreoResumenDto
        {
            Total = resultado.Total,
            Completados = resultado.Completados,
            EnProceso = resultado.EnProceso,
            FallosPersistentes = resultado.FallosPersistentes,
            Otros = resultado.Otros,
            DuracionPromedioSegundos = resultado.DuracionPromedioSegundos ?? 0,
            DuracionMinimaSegundos = resultado.DuracionMinimaSegundos ?? 0,
            DuracionMaximaSegundos = resultado.DuracionMaximaSegundos ?? 0,
            ReintentosPromedio = resultado.ReintentosPromedio ?? 0,
            ReintentosMaximos = resultado.ReintentosMaximos ?? 0,
            TasaExito = resultado.Total > 0 ? (decimal)resultado.Completados / resultado.Total * 100 : 0,
            TasaError = resultado.Total > 0 ? (decimal)resultado.FallosPersistentes / resultado.Total * 100 : 0,
            TasaEnProceso = resultado.Total > 0 ? (decimal)resultado.EnProceso / resultado.Total * 100 : 0
        };
    }
}

// Entidades auxiliares para queries
public class SyncJobLogUnificado
{
    public Guid ProcesoControlId { get; set; }
    public int TipoProceso { get; set; }
    public string TipoProcesoNombre { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? MensajeError { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? DuracionMilisegundos { get; set; }
    public string? SubprocesoActual { get; set; }
    public int Reintentos { get; set; }
    public DateTime? FechaUltimoReintento { get; set; }
    public string? Metadata { get; set; }
    public Guid? IdentificadorCotizacion { get; set; }
    public string? FolioCotizacion { get; set; }
    public bool? CompletadoCotizacion { get; set; }
    public int? DuracionSegundos { get; set; }
    public string CategoriaEstado { get; set; } = string.Empty;
    public int EstadoOrden { get; set; }
}

public class ResumenQueryResult
{
    public int Total { get; set; }
    public int Completados { get; set; }
    public int EnProceso { get; set; }
    public int FallosPersistentes { get; set; }
    public int Otros { get; set; }
    public double? DuracionPromedioSegundos { get; set; }
    public int? DuracionMinimaSegundos { get; set; }
    public int? DuracionMaximaSegundos { get; set; }
    public double? ReintentosPromedio { get; set; }
    public int? ReintentosMaximos { get; set; }
}
```

---

## 🎮 Controller Actualizado

### MonitoreoController

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MonitoreoController : ControllerBase
{
    private readonly IMonitoreoAvanzadoService _monitoreoService;
    private readonly ILogger<MonitoreoController> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public MonitoreoController(
        IMonitoreoAvanzadoService monitoreoService,
        ILogger<MonitoreoController> logger,
        IBackgroundJobClient backgroundJobClient)
    {
        _monitoreoService = monitoreoService;
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    /// <summary>
    /// Obtiene registros de sincronización con filtros avanzados
    /// </summary>
    [HttpPost("registros")]
    public async Task<ActionResult<MonitoreoResultadoDto>> ObtenerRegistros([FromBody] MonitoreoFiltrosDto filtros)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _monitoreoService.ObtenerRegistrosAsync(filtros);
            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registros de monitoreo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un registro específico por ProcessId
    /// </summary>
    [HttpGet("registro/{processId}")]
    public async Task<ActionResult<MonitoreoRegistroDto>> ObtenerRegistroPorId(Guid processId)
    {
        try
        {
            var registro = await _monitoreoService.ObtenerRegistroPorIdAsync(processId);
            
            if (registro == null)
                return NotFound($"Registro con ProcessId {processId} no encontrado");
            
            return Ok(registro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registro por ProcessId: {ProcessId}", processId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Reintenta un registro específico por ProcessId
    /// </summary>
    [HttpPost("registro/{processId}/reintentar")]
    public async Task<ActionResult> ReintentarRegistro(Guid processId)
    {
        try
        {
            var registro = await _monitoreoService.ObtenerRegistroPorIdAsync(processId);
            
            if (registro == null)
                return NotFound($"Registro con ProcessId {processId} no encontrado");

            // Encolar reintento
            var jobId = _backgroundJobClient.Enqueue<ISincronizacionJobService>(
                job => job.EjecutarSincronizacion(
                    (TipoProcesoEtl)registro.TipoProceso, 
                    registro.RecordId));

            return Ok(new { 
                Message = "Reintento encolado exitosamente", 
                JobId = jobId,
                ProcessId = processId,
                Registro = new { registro.TipoProcesoNombre, registro.RecordId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al encolar reintento para registro: {ProcessId}", processId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene métricas agregadas
    /// </summary>
    [HttpGet("metricas")]
    public async Task<ActionResult<List<MonitoreoMetricasDto>>> ObtenerMetricas()
    {
        try
        {
            var metricas = await _monitoreoService.ObtenerMetricasAsync();
            return Ok(metricas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene registros que pueden ser reintentados automáticamente
    /// </summary>
    [HttpGet("para-reintentar")]
    public async Task<ActionResult<List<MonitoreoRegistroDto>>> ObtenerRegistrosParaReintentar([FromQuery] int limite = 100)
    {
        try
        {
            var registros = await _monitoreoService.ObtenerRegistrosParaReintentarAsync(limite);
            return Ok(registros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registros para reintentar");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Reintenta múltiples registros en lote
    /// </summary>
    [HttpPost("reintentar-lote")]
    public async Task<ActionResult> ReintentarLote([FromBody] ReintentarLoteDto request)
    {
        try
        {
            if (request.IdsProcessIds == null || !request.IdsProcessIds.Any())
                return BadRequest("Debe especificar al menos un registro para reintentar");

            var resultados = new List<object>();
            
            foreach (var processId in request.IdsProcessIds)
            {
                try
                {
                    var registro = await _monitoreoService.ObtenerRegistroPorIdAsync(processId);
                    
                    if (registro == null)
                    {
                        resultados.Add(new { ProcessId = processId, Exitoso = false, Error = "Registro no encontrado" });
                        continue;
                    }

                    var jobId = _backgroundJobClient.Enqueue<ISincronizacionJobService>(
                        job => job.EjecutarSincronizacion(
                            (TipoProcesoEtl)registro.TipoProceso, 
                            registro.RecordId));

                    resultados.Add(new { 
                        ProcessId = processId, 
                        Exitoso = true, 
                        JobId = jobId,
                        TipoProceso = registro.TipoProcesoNombre,
                        RecordId = registro.RecordId
                    });
                }
                catch (Exception ex)
                {
                    resultados.Add(new { ProcessId = processId, Exitoso = false, Error = ex.Message });
                }
            }

            return Ok(new { 
                Message = $"Proceso de reintento batch completado para {request.IdsProcessIds.Count} registros",
                Resultados = resultados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar reintento en lote");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}

public class ReintentarLoteDto
{
    public List<Guid> IdsProcessIds { get; set; } = new();
    public bool IncluirSoloErrores { get; set; } = true;
}

public class MonitoreoMetricasDto
{
    public int TipoProceso { get; set; }
    public string TipoProcesoNombre { get; set; } = string.Empty;
    public int TotalRegistros { get; set; }
    public int Exitosos { get; set; }
    public int ConErrores { get; set; }
    public int EnProceso { get; set; }
    public decimal TasaExitoPorcentaje { get; set; }
    public decimal TasaErrorPorcentaje { get; set; }
    public double DuracionPromedioMs { get; set; }
    public double ReintentosPromedio { get; set; }
    public DateTime UltimaSincronizacion { get; set; }
}
```

---

## 🔄 AutoMapper Configuration

```csharp
public class MonitoreoMappingProfile : Profile
{
    public MonitoreoMappingProfile()
    {
        CreateMap<SyncJobLogUnificado, MonitoreoRegistroDto>()
            .ForMember(dest => dest.TieneError, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.MensajeError)))
            .ForMember(dest => dest.EstaEnProceso, opt => opt.MapFrom(src => src.Estado.StartsWith("EnProceso")))
            .ForMember(dest => dest.EstaCompletado, opt => opt.MapFrom(src => src.Estado == "Completado"))
            .ForMember(dest => dest.FalloPersistente, opt => opt.MapFrom(src => src.Estado == "FalloPersistente"));
    }
}
```

---

## 🎯 Ejemplos de Uso

### Búsqueda por ProcessId específico
```bash
GET /api/monitoreo/registro/a1b2c3d4-e5f6-7890-1234-567890abcdef

// O con filtros
POST /api/monitoreo/registros
{
  "processId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
}
```

### Filtros combinados
```bash
POST /api/monitoreo/registros
{
  "tiposProceso": [1, 2],
  "categoriaEstado": "ConErrores", 
  "fechaInicioDesde": "2025-12-01",
  "fechaInicioHasta": "2025-12-19",
  "soloConErrores": true,
  "tipoOperacion": "INSERT",      // Nuevo: INSERT o UPDATE
  "soloConRollback": true,        // Nuevo: solo registros con rollback
  "soloConSnapshot": true,         // Nuevo: solo con snapshots
  "tipoSnapshot": "POST_PROCESO",  // Nuevo: tipo de snapshot
  "pagina": 1,
  "tamanoPagina": 50,
  "ordenarPor": "FechaInicio",
  "ordenDescendente": true
}
```

### Reintento individual
```bash
POST /api/monitoreo/registro/a1b2c3d4-e5f6-7890-1234-567890abcdef/reintentar
```

### Reintento en lote
```bash
POST /api/monitoreo/reintentar-lote
{
  "idsProcessIds": [
    "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "b2c3d4e5-f6a7-8901-2345-6789abcdef1"
  ]
}
```

Este diseño proporciona un servicio de monitoreo robusto, flexible y optimizado que permite consultar cualquier tipo de registro de sincronización sin importar su entidad origen.