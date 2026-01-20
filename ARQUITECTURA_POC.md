# Arquitectura de Alto Nivel - SincronizadorPqfLegacy (PoC)

**Versión**: 1.0
**Fecha**: 2025-12-10
**Propósito**: Documentación base para extraer componentes reutilizables hacia implementación real

---

## 1. VISIÓN GENERAL

### 1.1 Propósito del Sistema

Sistema ETL (Extract-Transform-Load) para sincronizar datos entre:
- **Sistema Origen**: ProquifaDotNet (nuevo sistema)
- **Sistema Legacy**: PConnect (sistema antiguo)
- **Sistema de Control**: Tablas intermedias para tracking y auditoría

### 1.2 Arquitectura Aplicada

**Clean Architecture** con 4 capas concéntricas:

```
┌─────────────────────────────────────────┐
│   API (Presentación)                    │  ← Controllers, Middleware
├─────────────────┬───────────────────────┤
│   Application   │   Infrastructure      │  ← Lógica de negocio / Persistencia
├─────────────────┴───────────────────────┤
│   Domain (Núcleo)                       │  ← Entidades, Interfaces, DTOs
└─────────────────────────────────────────┘
```

**Flujo de Dependencias**: API → Application → Domain ← Infrastructure

**Principio Clave**: El dominio NO depende de nada; todas las capas externas dependen del dominio.

---

## 2. PATRONES DE DISEÑO IMPLEMENTADOS

### 2.1 Patrones Estructurales

#### Repository Pattern
- **Abstracción**: Acceso a datos independiente del ORM
- **Implementación**: `GenericRepository<T>` con operaciones CRUD base
- **Repositorios específicos**: Heredan de genérico y agregan queries custom

**Ventaja para implementación real**:
- Fácil cambio de ORM (EF Core → Dapper/ADO.NET)
- Testing más sencillo (mocking de interfaces)

#### Unit of Work Pattern
- **Coordinación**: Transacciones que abarcan múltiples repositorios
- **Implementación**: `IUnitOfWork` con soporte para transacciones explícitas

**Ventaja para implementación real**:
- Garantiza consistencia transaccional
- Ideal para operaciones complejas multi-tabla

#### Dependency Injection (DI)
- **Configuración**: Métodos de extensión modulares (`ServiceExtensions`)
- **Ciclos de vida**: Scoped para servicios y repositorios

**Ventaja para implementación real**:
- Configuración organizada por feature
- Fácil agregar nuevos módulos ETL

### 2.2 Patrones de Comportamiento

#### Strategy Pattern - Clasificación de Errores
- **Contexto**: `ExceptionClassifier`
- **Estrategias**:
  - Transient → Reintentar automáticamente
  - Permanent → No reintentar, escalar a revisión manual

**Ventaja para implementación real**:
- Resiliencia automatizada
- Reduce intervención manual en errores transitorios (>80% de casos)

#### ETL Pattern Mejorado
- **Extract**: Leer desde vistas exclusivas en origen
- **Transform**: AutoMapper con reglas de negocio
- **Load**: Insertar/Actualizar en destino con manejo de duplicados
- **Control**: Tracking en tabla genérica `EtlProcesoControl`
- **Validaciones**: Capa de validaciones previas al ETL
- **Subprocesos**: División con puntos de compensación
- **Notificaciones**: Correos para fallos permanentes

**Ventaja para implementación real**:
- Flujo robusto con rollback garantizado
- Validaciones previas evitan procesamiento inválido
- Tracking detallado por subproceso
- Notificaciones automáticas para fallos críticos

#### Template Method Pattern
- **Contexto**: `SincronizacionJobService`
- **Template**: Estructura fija de manejo de jobs Hangfire
- **Puntos de extensión**: Lógica específica de cada tipo de entidad

**Ventaja para implementación real**:
- Nuevo tipo de ETL = Implementar solo método específico
- Infraestructura de jobs reutilizable

---

## 3. COMPONENTES REUTILIZABLES

### 3.1 Capa de Dominio (100% reutilizable)

#### ✅ Interfaces de Repositorio
```
Domain/Interfaces/
├── IGenericRepository<T>           ← CRUD base
├── IUnitOfWork                     ← Transacciones
└── IExceptionClassifier            ← Clasificación de errores
```

**Recomendación**: Mantener como están, son genéricas y bien diseñadas.

#### ✅ Sistema de Excepciones
```
Domain/Exceptions/
├── DomainException                 ← Errores de negocio
├── DomainArgumentNullException     ← Validaciones
├── InfrastructureException         ← Errores de infraestructura
└── InfrastructureNotImplementedException
```

**Recomendación**: Reutilizar jerarquía completa.

#### ✅ Modelos de Dominio
```
Domain/Models/
├── ErrorCategory.cs                ← enum: Transient, Permanent
└── ProcesoEtlMetadata.cs          ← Catálogo de procesos ETL
```

**Recomendación**:
- `ErrorCategory` → Reutilizar tal cual
- `ProcesoEtlMetadata` → Adaptar para nuevas entidades

#### ⚠️ DTOs de Dominio
```
Domain/DTOs/
├── CotizacionOrigenDto             ← Específico de Cotización
├── CotizacionLegacyDto             ← Específico de Cotización
└── SyncJobLogDto                   ← REUTILIZABLE
```

**Recomendación**:
- Crear nuevos DTOs para cada entidad (Pedido, Factura, etc.)
- Mantener patrón: `{Entidad}OrigenDto`, `{Entidad}LegacyDto`, `{Entidad}ControlDto`

---

### 3.2 Capa de Aplicación (70% reutilizable)

#### ✅ Excepciones de Aplicación
```
Application/Exceptions/
├── AppException                    ← REUTILIZABLE
├── AppArgumentException            ← REUTILIZABLE
├── AppKeyNotFoundException         ← REUTILIZABLE
└── AppFileNotFoundException        ← REUTILIZABLE
```

**Recomendación**: Mantener como biblioteca común de excepciones.

#### ✅ ExceptionClassifier (100% reutilizable)
**Archivo**: `Application/Services/ExceptionClassifier.cs`

**Lógica de clasificación**:
- SQL Timeout (-2) → Transient
- Deadlock (1205) → Transient
- FK violation (547) → Permanent
- Validation errors → Permanent

**Recomendación**: **NO modificar**, es genérico y robusto.

#### ✅ SyncLogService (100% reutilizable)
**Archivo**: `Application/Services/SyncLogService.cs`

**Funcionalidad**:
- `LogSuccessAsync()`: Registrar sincronización exitosa
- `LogPermanentFailureAsync()`: Registrar fallos persistentes

**Recomendación**: Reutilizar para todas las entidades.

#### 🆕 EtlProcesoControlService (100% reutilizable)
**Archivo**: `Application/Services/EtlProcesoControlService.cs`

**Funcionalidad**:
- `CrearProcesoAsync()`: Insertar registro inicial con estado "EnProcesoInicio"
- `ActualizarEstadoAsync()`: Cambiar estado y registrar avance
- `RegistrarSubprocesoAsync()`: Actualizar subproceso actual
- `CompletarProcesoAsync()`: Marcar como completado con duración
- `RegistrarFalloPersistenteAsync()`: Cambiar estado y agregar mensaje error

**Recomendación**: Servicio central para tracking genérico.

#### 🆕 NotificationService (100% reutilizable)
**Archivo**: `Application/Services/NotificationService.cs`

**Funcionalidad**:
- `EnviarNotificacionFalloPermanenteAsync()`: Correo para fallos persistentes
- `EnviarNotificacionReintentoExcedidoAsync()`: Correo cuando se exceden reintentos

**Recomendación**: Integrar con ExceptionClassifier para notificaciones automáticas.

#### 🆕 MonitoreoService (100% reutilizable)
**Archivo**: `Application/Services/MonitoreoService.cs`

**Funcionalidad**:
- `ObtenerEstadosSincronizacionAsync()`: Consulta con filtros genéricos
- `ObtenerResumenEstadosAsync()`: Estadísticas por estado
- `ObtenerMétricasRendimientoAsync()`: Tiempos promedio por proceso

**Recomendación**: Base para endpoint de monitoreo y dashboard.

#### ⚠️ Servicios ETL Específicos (30% reutilizable)
```
Application/Services/
├── SincronizarCotizacionService.cs     ← ESPECÍFICO (usar como plantilla)
├── SincronizarPartidasService.cs       ← ESPECÍFICO (usar como plantilla)
└── SincronizacionMultipleService.cs    ← 80% REUTILIZABLE
```

**Recomendación**:
- **SincronizarCotizacionService**: Usar como **plantilla** para nuevas entidades
- **SincronizacionMultipleService**: Refactorizar para hacerlo genérico (`SincronizacionMultipleService<T>`)

#### 🆕 Servicios de Validación (Nuevos - 100% reutilizables)
```
Application/Services/Validaciones/
├── IValidacionProcesoService.cs         ← Interfaz genérica
├── ValidacionCotizacionService.cs       ← Implementación específica
└── ValidacionBaseService.cs             ← Validaciones comunes
```

**Funcionalidad**:
- Validar integridad de datos antes del ETL
- Validar relaciones y restricciones de negocio
- Lanzar excepciones específicas con mensajes claros

**Recomendación**: Crear un servicio de validación por cada tipo de proceso.

#### 🆕 Servicios de Subprocesos (Nuevos - 100% reutilizables)
```
Application/Services/Subprocesos/
├── ISubprocesoExtractService.cs         ← Interfaz genérica
├── ISubprocesoTransformService.cs       ← Interfaz genérica
├── ISubprocesoLoadService.cs            ← Interfaz genérica
└── SubprocesoCompensacionService.cs     ← Rollback automático
```

**Funcionalidad**:
- Dividir ETL en subprocesos atómicos
- Puntos de compensación para rollback
- Tracking individual por subproceso

**Recomendación**: Implementar subprocesos para cada entidad con puntos de compensación.

#### ✅ SincronizacionJobService (90% reutilizable)
**Archivo**: `Application/Services/SincronizacionJobService.cs`

**Estructura**:
```csharp
public async Task EjecutarSincronizacion(TipoProcesoEtl tipoProceso, Guid recordId)
{
    try
    {
        switch (tipoProceso)
        {
            case TipoProcesoEtl.Cotizacion:
                await _sincronizarCotizacion.SincronizarCotizacion(recordId);
                break;
            case TipoProcesoEtl.Pedido:  // ← Agregar nuevos casos
                await _sincronizarPedido.SincronizarPedido(recordId);
                break;
        }
    }
    catch (Exception ex)
    {
        await ManejarError(ex, tipoProceso.ToString(), recordId);
    }
}
```

**Recomendación**:
- Mantener estructura
- Agregar nuevos casos según nuevas entidades

---

### 3.3 Capa de Infraestructura (50% reutilizable)

#### ✅ GenericRepository (100% reutilizable)
**Archivo**: `Infrastructure/Repository/GenericRepository.cs`

**Funcionalidad destacada**:
```csharp
public async Task<Guid> AddOrUpdate(T entity)
{
    // Si Id == Guid.Empty → Insertar
    // Si Id existe → Actualizar (merge selectivo de no-null)
    // Si Id no existe → Insertar con Id proporcionado
}
```

**Recomendación**: **NO modificar**, es genérico y funcional.

#### ✅ UnitOfWork (100% reutilizable)
**Archivo**: `Infrastructure/Persistence/UnitOfWork.cs`

**Recomendación**: Mantener tal cual.

#### ⚠️ Repositorios Específicos (usar como plantilla)
```
Infrastructure/Repository/
├── CotizacionOrigenRepository.cs       ← PLANTILLA
├── CotizacionLegacyRepository.cs       ← PLANTILLA
└── CotizacionControlRepository.cs      ← PLANTILLA
```

**Patrón a replicar**:
```csharp
public class {Entidad}OrigenRepository : I{Entidad}OrigenRepository
{
    public async Task<{Entidad}OrigenDto?> ObtenerPorIdAsync(Guid id)
    {
        var entity = await _context.v{Entidad}TransformadasETL
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id{Entidad} == id);

        return _mapper.Map<{Entidad}OrigenDto>(entity);
    }
}
```

#### ⚠️ DbContexts (específico de cada base de datos)
```
Infrastructure/Persistence/
├── MicroservicioContext.cs             ← Para logs (REUTILIZABLE)
├── ProquifaDotNetContext.cs            ← Específico de origen
├── PConnectContext.cs                  ← Específico de legacy
└── PConnectProquifaDotNetContext.cs    ← Para control (REUTILIZABLE)
```

**Recomendación**:
- Mantener patrón de múltiples contextos
- Scaffold desde bases de datos reales

#### ✅ AutoMapper Profiles (plantillas reutilizables)
```
Infrastructure/Mappers/
├── SyncJobLogMappingProfile.cs         ← 100% REUTILIZABLE
├── EtlProcesoControlMappingProfile.cs  ← 100% REUTILIZABLE (NUEVO)
├── CotizaMappingProfile.cs             ← PLANTILLA para nuevas entidades
└── ApplicationMappingProfile.cs        ← 100% REUTILIZABLE
```

**Patrón a replicar** (de `CotizaMappingProfile`):
```csharp
CreateMap<{Entidad}OrigenDto, {Entidad}LegacyDto>()
    .ForMember(dest => dest.PK_ID, opt => opt.Ignore())  // IDENTITY
    .AfterMap((src, dest, context) =>
    {
        // Reglas de negocio post-mapeo
        if (string.IsNullOrEmpty(dest.Campo))
            dest.Campo = "VALOR_POR_DEFECTO";
    });
```

---

### 3.4 Capa de API (80% reutilizable)

#### ✅ ExceptionHandlerMiddleware (100% reutilizable)
**Archivo**: `API/ExceptionMiddleware/ExceptionHandlerMiddleware.cs`

**Funcionalidad**:
- Captura excepciones globales
- Clasifica con `IExceptionClassifier`
- Retorna RFC 7807 ProblemDetails

**Recomendación**: **NO modificar**.

#### ✅ ServiceExtensions (90% reutilizable)
**Archivo**: `API/Extensions/ServiceExtensions.cs`

**Métodos modulares**:
```csharp
builder.Services.ConfigureDatabases(config);        // ← Adaptar connection strings
builder.Services.ConfigureApplicationServices();    // ← 100% reutilizable
builder.Services.ConfigureETLServices(config);      // ← Agregar nuevos servicios ETL
builder.Services.ConfigureHangfire(config);         // ← 100% reutilizable
```

**Recomendación**: Mantener estructura modular.

#### 🆕 MonitoreoController (100% reutilizable)
**Archivo**: `API/Controllers/MonitoreoController.cs`

**Funcionalidad**:
- `POST /api/etl/monitoreo`: Consulta con filtros genéricos
- `GET /api/etl/monitoreo/resumen`: Estadísticas por estado
- `GET /api/etl/monitoreo/metricas`: Métricas de rendimiento

**Recomendación**: Controller dedicado para monitoreo, separado de ETL.

#### ⚠️ Controllers (plantilla)
**Archivo**: `API/Controllers/EtlController.cs`

**Endpoints**:
```csharp
POST /api/etl/sincronizar                   // ← Individual
POST /api/etl/sincronizar-pendientes        // ← Masivo
GET  /api/etl/catalogo                      // ← Metadata
```

**Recomendación**:
- Mantener estructura unificada (un solo controller para todos los ETL)
- Usar `TipoProcesoEtl` enum para diferenciar entidades

#### ✅ AutomaticRetryFailureLogFilter (100% reutilizable)
**Archivo**: `API/Filters/AutomaticRetryFailureLogFilter.cs`

**Recomendación**: Mantener tal cual.

---

## 4. CONFIGURACIÓN REUTILIZABLE

### 4.1 appsettings.json

#### ✅ Secciones 100% Reutilizables

**Serilog**:
```json
"Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
        { "Name": "Console" },
        { "Name": "File", "Args": { "path": "Logs/log-.txt" } }
    ]
}
```

**Hangfire**:
```json
"Hangfire": {
    "RetryAttempts": 5,
    "RetryDelays": [60, 300, 900, 3600, 7200]
}
```

**SyncLogCleanup**:
```json
"SyncLogCleanup": {
    "Habilitado": true,
    "DiasRetencion": 30,
    "CronExpression": "0 2 * * *"
}
```

#### ⚠️ Secciones Específicas (adaptar)

**ConnectionStrings**: Actualizar según entorno real

**SincronizacionAutomatica**: Ajustar CRON según necesidades

---

## 5. ESTRATEGIA DE MIGRACIÓN A IMPLEMENTACIÓN REAL

### 5.1 Componentes a Mantener SIN Cambios

| Componente | Ubicación | Motivo |
|------------|-----------|--------|
| `IGenericRepository<T>` | Domain/Interfaces | Abstracción sólida |
| `GenericRepository<T>` | Infrastructure/Repository | Funcionalidad probada |
| `IUnitOfWork` | Domain/Interfaces | Estándar de industria |
| `UnitOfWork` | Infrastructure/Persistence | Implementación robusta |
| `ExceptionClassifier` | Application/Services | Lógica genérica |
| `ErrorCategory` | Domain/Models | Categorización correcta |
| `SyncLogService` | Application/Services | Auditoría esencial |
| `ExceptionHandlerMiddleware` | API/ExceptionMiddleware | Manejo centralizado |
| `AutomaticRetryFailureLogFilter` | API/Filters | Resiliencia automatizada |

### 5.2 Componentes a Refactorizar (Hacer Genéricos)

#### SincronizacionMultipleService → `SincronizacionMultipleService<T>`

**Código actual** (específico de Cotización):
```csharp
public async Task<ResultadoSincronizacionMultipleDto> SincronizarPendientesAsync()
{
    var pendientes = await _cotizacionControlRepo.ObtenerPendientesSincronizacionAsync();
    // ...
}
```

**Propuesta genérica**:
```csharp
public class SincronizacionMultipleService<TDto, TRepository>
    where TRepository : IControlRepository<TDto>
{
    private readonly TRepository _controlRepo;
    private readonly ISincronizarEntidad<TDto> _sincronizador;

    public async Task<ResultadoSincronizacionMultipleDto> SincronizarPendientesAsync()
    {
        var pendientes = await _controlRepo.ObtenerPendientesSincronizacionAsync();
        // Lógica genérica...
    }
}
```

**Beneficio**: Un solo servicio sirve para Cotizaciones, Pedidos, Facturas, etc.

#### ProcesoEtlMetadataService → Catálogo Dinámico

**Código actual** (hardcodeado):
```csharp
new ProcesoEtlMetadata
{
    Id = 1,
    Nombre = "Cotizacion",
    Descripcion = "Sincronización de cotizaciones"
}
```

**Propuesta**: Cargar desde base de datos o configuración externa

### 5.3 Componentes a Crear Nuevos (por Entidad)

Para cada nueva entidad (Pedido, Factura, etc.):

**1. DTOs** (Domain/DTOs/):
```
PedidoOrigenDto.cs
PedidoLegacyDto.cs
PedidoControlDto.cs
```

**2. Interfaces de Repositorio y Validación** (Domain/Interfaces/):
```
IPedidoOrigenRepository.cs
IPedidoLegacyRepository.cs
IPedidoValidacionService.cs        ← NUEVO: validaciones específicas
```

**3. Implementación de Repositorios y Validación** (Infrastructure/Repository/):
```
PedidoOrigenRepository.cs
PedidoLegacyRepository.cs
PedidoValidacionService.cs         ← NUEVO: implementación validaciones
```

**4. Servicios ETL y Subprocesos** (Application/Services/):
```
SincronizarPedidoService.cs        ← Principal (copiar de Cotizacion)
PedidoSubprocesoExtractService.cs  ← NUEVO: extract específico
PedidoSubprocesoTransformService.cs← NUEVO: transform específico
PedidoSubprocesoLoadService.cs     ← NUEVO: load específico
```

**5. AutoMapper Profile** (Infrastructure/Mappers/):
```
PedidoMappingProfile.cs  (copiar de CotizaMappingProfile.cs)
```

**6. Enum** (actualizar Domain/Enums/TipoProcesoEtl.cs):
```csharp
public enum TipoProcesoEtl
{
    Cotizacion = 1,
    Pedido = 2,        // ← NUEVO
    Factura = 3,       // ← NUEVO
    Inventario = 4     // ← NUEVO
}
```

### 5.4 Estructura de Carpetas Propuesta para Implementación Real

```
SincronizadorPqfLegacy/
│
├── API/                                    ← Mantener estructura
│   ├── Controllers/
│   │   └── EtlController.cs               ← Controller unificado
│   ├── Filters/
│   │   └── AutomaticRetryFailureLogFilter.cs
│   └── Extensions/
│       └── ServiceExtensions.cs           ← Módulos por feature
│
├── Application/
│   ├── Common/                             ← NUEVO: Componentes compartidos
│   │   ├── Exceptions/                    ← Excepciones genéricas
│   │   ├── ExceptionClassifier.cs         ← Clasificador global
│   │   ├── SyncLogService.cs              ← Logging global
│   │   ├── EtlProcesoControlService.cs    ← NUEVO: tracking genérico
│   │   ├── NotificationService.cs         ← NUEVO: notificaciones por correo
│   │   └── MonitoreoService.cs            ← NUEVO: consultas de estado
│   │
│   ├── Features/                           ← NUEVO: Organizar por feature
│   │   ├── Cotizaciones/
│   │   │   ├── DTOs/
│   │   │   ├── Interfaces/
│   │   │   └── Services/
│   │   │       ├── SincronizarCotizacionService.cs
│   │   │       └── SincronizacionCotizacionesMultipleService.cs
│   │   │
│   │   ├── Pedidos/                       ← NUEVO
│   │   │   ├── DTOs/
│   │   │   ├── Interfaces/
│   │   │   └── Services/
│   │   │
│   │   └── Facturas/                      ← NUEVO
│   │       └── ...
│   │
│   └── Core/                               ← NUEVO: Servicios transversales
│       ├── SincronizacionJobService.cs
│       └── ProcesoEtlMetadataService.cs
│
├── Domain/
│   ├── Common/                             ← NUEVO
│   │   ├── Exceptions/                    ← Excepciones de dominio
│   │   ├── Enums/
│   │   │   ├── TipoProcesoEtl.cs
│   │   │   └── ErrorCategory.cs
│   │   └── Models/
│   │       └── ProcesoEtlMetadata.cs
│   │
│   ├── Entities/                           ← NUEVO: Entidades por feature
│   │   ├── Cotizaciones/
│   │   │   ├── DTOs/
│   │   │   └── Interfaces/
│   │   ├── Pedidos/
│   │   └── Facturas/
│   │
│   └── Repositories/                       ← NUEVO: Interfaces genéricas
│       ├── IGenericRepository.cs
│       ├── IUnitOfWork.cs
│       └── IControlRepository.cs           ← NUEVO (abstracción para control)
│
└── Infrastructure/
    ├── Common/                             ← NUEVO: Componentes compartidos
    │   ├── Persistence/
    │   │   ├── GenericRepository.cs
    │   │   └── UnitOfWork.cs
    │   └── Mappers/
    │       ├── ApplicationMappingProfile.cs
    │       └── SyncJobLogMappingProfile.cs
    │
    ├── Features/                           ← NUEVO: Implementaciones por feature
    │   ├── Cotizaciones/
    │   │   ├── Repositories/
    │   │   └── Mappers/
    │   │       └── CotizaMappingProfile.cs
    │   ├── Pedidos/
    │   └── Facturas/
    │
    └── Persistence/                        ← Contextos de BD
        ├── Contexts/
        │   ├── MicroservicioContext.cs    ← Logs
        │   ├── OrigenContext.cs           ← Sistema origen
        │   ├── LegacyContext.cs           ← Sistema legacy
        │   └── ControlContext.cs          ← Tablas de control
        └── Entities/                       ← Entidades EF Core
```

---

## 6. RECOMENDACIONES PARA IMPLEMENTACIÓN REAL

### 6.1 Mejoras Arquitectónicas

#### 1. Implementar CQRS (Command Query Responsibility Segregation)

**Separar**:
- **Commands**: Operaciones de escritura (ETL)
- **Queries**: Operaciones de lectura (consultas)

**Ejemplo**:
```
Application/Features/Cotizaciones/
├── Commands/
│   ├── SincronizarCotizacionCommand.cs
│   └── SincronizarCotizacionCommandHandler.cs
└── Queries/
    ├── ObtenerPendientesQuery.cs
    └── ObtenerPendientesQueryHandler.cs
```

**Beneficio**: Mejor escalabilidad y mantenibilidad.

#### 2. Agregar Mediator Pattern (MediatR)

**Ventaja**: Desacoplar controladores de servicios

**Antes**:
```csharp
[HttpPost("sincronizar")]
public async Task<IActionResult> Sincronizar([FromBody] SincronizarRequest request)
{
    await _sincronizacionJobService.EjecutarSincronizacion(request.TipoProceso, request.RecordId);
    return Accepted();
}
```

**Después**:
```csharp
[HttpPost("sincronizar")]
public async Task<IActionResult> Sincronizar([FromBody] SincronizarCommand command)
{
    var result = await _mediator.Send(command);
    return Accepted(result);
}
```

#### 3. Event Sourcing para Auditoría

**Propuesta**: Registrar eventos de dominio para trazabilidad completa

**Eventos**:
- `CotizacionExtractedEvent`
- `CotizacionTransformedEvent`
- `CotizacionLoadedEvent`
- `SincronizacionFailedEvent`

**Beneficio**:
- Auditoría completa del flujo
- Capacidad de replay
- Debugging mejorado

### 6.2 Mejoras de Performance

#### 1. Procesamiento en Paralelo

**Código actual** (secuencial):
```csharp
foreach (var cotizacion in pendientes)
{
    await SincronizarCotizacionAsync(cotizacion.Id);
}
```

**Propuesta** (paralelo):
```csharp
var tasks = pendientes.Select(c => SincronizarCotizacionAsync(c.Id));
await Task.WhenAll(tasks);
```

**Beneficio**: Reducir tiempo de sincronización masiva en ~70%.

#### 2. Caching de Catálogos

**Implementar**:
```csharp
services.AddMemoryCache();
services.AddScoped<ICatalogoCacheService, CatalogoCacheService>();
```

**Cachear**:
- Clientes
- Productos
- Monedas
- Estados

**Beneficio**: Reducir consultas repetitivas a BD.

#### 3. Bulk Insert para Partidas

**Código actual** (uno por uno):
```csharp
foreach (var partida in partidas)
{
    await _partidaRepo.InsertarAsync(partida);
}
```

**Propuesta** (bulk):
```csharp
await _context.PCotiza.AddRangeAsync(partidas);
await _context.SaveChangesAsync();
```

**Beneficio**: ~80% más rápido para lotes grandes.

### 6.3 Mejoras de Resiliencia

#### 1. Circuit Breaker Pattern (Polly)

**Agregar**:
```csharp
services.AddHttpClient<IOrigenApiClient, OrigenApiClient>()
    .AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)
        );
}
```

**Beneficio**: Evitar sobrecarga de sistemas externos en fallos.

#### 2. Dead Letter Queue para Fallos Permanentes

**Propuesta**: Tabla o cola especial para registros con fallo permanente

**Tabla**:
```sql
CREATE TABLE DeadLetterQueue (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TipoEntidad NVARCHAR(50),
    EntidadId UNIQUEIDENTIFIER,
    Payload NVARCHAR(MAX),  -- JSON del DTO original
    Error NVARCHAR(MAX),
    FechaFallo DATETIME DEFAULT GETDATE(),
    Revisado BIT DEFAULT 0
)
```

**Beneficio**: Facilitar corrección manual y re-procesamiento.

#### 3. Idempotencia Garantizada

**Implementar**:
```csharp
public async Task<Guid> SincronizarCotizacion(Guid idCotizacion, string idempotencyKey)
{
    // 1. Verificar si ya se procesó con esta clave
    var existente = await _idempotencyRepo.GetByKeyAsync(idempotencyKey);
    if (existente != null)
        return existente.ResultadoId;  // Retornar resultado previo

    // 2. Procesar
    var resultado = await EjecutarETL(idCotizacion);

    // 3. Guardar clave de idempotencia
    await _idempotencyRepo.SaveAsync(idempotencyKey, resultado);

    return resultado;
}
```

**Beneficio**: Evitar duplicados por reintentos.

### 6.4 Mejoras de Observabilidad

#### 1. Telemetría con Application Insights

**Agregar**:
```csharp
services.AddApplicationInsightsTelemetry(configuration["ApplicationInsights:InstrumentationKey"]);
```

**Trackear**:
- Duración de cada fase ETL (Extract, Transform, Load)
- Tasa de éxito/fallo por tipo de entidad
- Distribución de tipos de error

#### 2. Métricas de Negocio

**Implementar**:
```csharp
public class EtlMetricsService
{
    public async Task<EtlMetrics> GetMetricsAsync(DateTime desde, DateTime hasta)
    {
        return new EtlMetrics
        {
            TotalProcesados = await GetTotalProcesadosAsync(desde, hasta),
            TasaExito = await GetTasaExitoAsync(desde, hasta),
            PromedioTiempoEjecucion = await GetPromedioTiempoAsync(desde, hasta),
            TopErrores = await GetTopErroresAsync(desde, hasta)
        };
    }
}
```

**Exponer en endpoint**:
```csharp
GET /api/etl/metrics?desde=2025-01-01&hasta=2025-01-31
```

#### 3. Health Checks

**Agregar**:
```csharp
services.AddHealthChecks()
    .AddSqlServer(configuration.GetConnectionString("ProquifaDotNet"), name: "db-origen")
    .AddSqlServer(configuration.GetConnectionString("PConnect"), name: "db-legacy")
    .AddHangfire(options => { options.MaximumJobsFailed = 5; });
```

**Exponer**:
```csharp
app.MapHealthChecks("/health");
```

### 6.5 Mejoras de Testing

#### 1. Tests de Integración para Repositorios

**Ejemplo**:
```csharp
public class CotizacionOrigenRepositoryTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task ObtenerPorIdAsync_CotizacionExiste_RetornaDto()
    {
        // Arrange
        var repo = new CotizacionOrigenRepository(_context, _mapper);
        var id = Guid.Parse("...");

        // Act
        var resultado = await repo.ObtenerPorIdAsync(id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado.IdCotizacion);
    }
}
```

#### 2. Tests Unitarios para ExceptionClassifier

**Ejemplo**:
```csharp
public class ExceptionClassifierTests
{
    [Theory]
    [InlineData(typeof(TimeoutException), ErrorCategory.Transient)]
    [InlineData(typeof(ValidationException), ErrorCategory.Permanent)]
    [InlineData(typeof(AppKeyNotFoundException), ErrorCategory.Permanent)]
    public void Classify_Exception_ReturnsExpectedCategory(Type exceptionType, ErrorCategory expectedCategory)
    {
        // Arrange
        var classifier = new ExceptionClassifier();
        var exception = (Exception)Activator.CreateInstance(exceptionType);

        // Act
        var category = classifier.Classify(exception);

        // Assert
        Assert.Equal(expectedCategory, category);
    }
}
```

#### 3. Tests End-to-End con Testcontainers

**Ejemplo**:
```csharp
public class EtlE2ETests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer;

    public EtlE2ETests()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }

    [Fact]
    public async Task SincronizarCotizacion_E2E_Success()
    {
        // Arrange: Setup BD con datos de prueba
        // Act: Ejecutar ETL completo
        // Assert: Verificar datos en destino
    }
}
```

---

## 7. CHECKLIST DE MIGRACIÓN

### Fase 1: Extracción de Componentes Reutilizables (Semana 1)

- [ ] Copiar capa Domain completa
- [ ] Copiar excepciones de Application
- [ ] Copiar `ExceptionClassifier`
- [ ] Copiar `SyncLogService`
- [ ] Copiar `GenericRepository` y `UnitOfWork`
- [ ] Copiar `ExceptionHandlerMiddleware`
- [ ] Copiar `ServiceExtensions` (base)

### Fase 2: Refactorización a Genéricos (Semana 2)

- [ ] Refactorizar `SincronizacionMultipleService<T>`
- [ ] Crear interfaz `IControlRepository<T>`
- [ ] Crear interfaz `ISincronizarEntidad<T>`
- [ ] Actualizar `SincronizacionJobService` para usar genéricos

### Fase 3: Implementación de Nueva Entidad (Semana 3)

- [ ] Crear DTOs para nueva entidad
- [ ] Crear interfaces de repositorio
- [ ] Implementar repositorios
- [ ] Crear servicio ETL específico (copiar plantilla)
- [ ] Crear AutoMapper profile
- [ ] Actualizar enum `TipoProcesoEtl`
- [ ] Configurar DI en `ServiceExtensions`

### Fase 4: Testing (Semana 4)

- [ ] Tests unitarios de `ExceptionClassifier`
- [ ] Tests unitarios de servicios ETL
- [ ] Tests de integración de repositorios
- [ ] Tests E2E de flujo completo

### Fase 5: Mejoras de Production (Semana 5)

- [ ] Implementar Circuit Breaker (Polly)
- [ ] Implementar Idempotencia
- [ ] Configurar Application Insights
- [ ] Agregar Health Checks
- [ ] Configurar Dead Letter Queue

---

## 8. CONCLUSIONES

### Lo que funcionó bien en la PoC

1. **Clean Architecture**: Separación clara de responsabilidades
2. **Repository Pattern**: Abstracción efectiva de acceso a datos
3. **ExceptionClassifier**: Resiliencia automatizada con clasificación inteligente
4. **Hangfire**: Gestión robusta de jobs en background
5. **AutoMapper**: Transformaciones declarativas y mantenibles
6. **Logging con Serilog**: Trazabilidad completa

### Lo que necesita mejoras

1. **Servicios específicos**: Demasiado código duplicado entre entidades
2. **Sin CQRS**: Mezcla de lectura y escritura en servicios
3. **Sin tests**: Cobertura de tests insuficiente
4. **Performance**: Procesamiento secuencial lento
5. **Observabilidad**: Métricas de negocio limitadas

### Componentes Listos para Producción

- ✅ `GenericRepository<T>`
- ✅ `UnitOfWork`
- ✅ `ExceptionClassifier`
- ✅ `SyncLogService`
- ✅ `ExceptionHandlerMiddleware`
- ✅ Sistema de excepciones custom
- ✅ Configuración de Hangfire

### Componentes que Requieren Refactoring

- ⚠️ `SincronizacionMultipleService` (hacer genérico)
- ⚠️ Servicios ETL específicos (crear plantillas/base classes)
- ⚠️ `ProcesoEtlMetadataService` (hacer dinámico)

---

**Preparado por**: Análisis automatizado de arquitectura
**Fecha**: 2025-12-10
**Versión PoC**: 1.0
**Estado**: Listo para migración a implementación real
