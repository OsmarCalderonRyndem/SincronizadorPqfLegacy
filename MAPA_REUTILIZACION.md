# Mapa de Reutilización de Componentes

**Propósito**: Guía visual para identificar qué copiar, qué adaptar y qué descartar

---

## 1. CÓDIGO SEMÁFORO DE REUTILIZACIÓN

```
🟢 VERDE (100% Reutilizable)   → Copiar tal cual, NO modificar
🟡 AMARILLO (Plantilla)        → Copiar y adaptar para cada entidad
🔵 AZUL (Refactorizar)         → Necesita cambios antes de reutilizar
🔴 ROJO (Específico de PoC)    → NO reutilizar, reescribir desde cero
```

---

## 2. MAPA VISUAL DE COMPONENTES

### CAPA DOMAIN

```
Domain/
│
├── 🟢 Interfaces/
│   ├── IGenericRepository<T>.cs           ✅ COPIAR
│   ├── IUnitOfWork.cs                     ✅ COPIAR
│   ├── IExceptionClassifier.cs            ✅ COPIAR
│   ├── IEtlProcesoControlService.cs       ✅ COPIAR (NUEVO)
│   ├── INotificationService.cs            ✅ COPIAR (NUEVO)
│   ├── IMonitoreoService.cs               ✅ COPIAR (NUEVO)
│   ├── IValidacionProcesoService.cs       ✅ COPIAR (NUEVO)
│   ├── ICotizacionOrigenRepository.cs     🟡 PLANTILLA
│   ├── ICotizacionLegacyRepository.cs     🟡 PLANTILLA
│   └── ICotizacionValidacionService.cs    🟡 PLANTILLA (NUEVO)
│
├── 🟢 Exceptions/
│   ├── DomainException.cs                 ✅ COPIAR
│   ├── DomainArgumentNullException.cs     ✅ COPIAR
│   ├── InfrastructureException.cs         ✅ COPIAR
│   └── InfrastructureNotImplementedException.cs ✅ COPIAR
│
├── 🟢 Models/
│   ├── ErrorCategory.cs                   ✅ COPIAR
│   ├── EtlProcesoControl.cs               ✅ COPIAR (NUEVO)
│   ├── MonitoreoFiltrosDto.cs             ✅ COPIAR (NUEVO)
│   ├── MonitoreoResultadoDto.cs           ✅ COPIAR (NUEVO)
│   └── ProcesoEtlMetadata.cs             🔵 REFACTORIZAR (hacer dinámico)
│
├── 🟢 Enums/
│   └── TipoProcesoEtl.cs                 ✅ COPIAR (extender con nuevas entidades)
│
└── 🟡 DTOs/
    ├── CotizacionOrigenDto.cs            🟡 PLANTILLA
    ├── CotizacionLegacyDto.cs            🟡 PLANTILLA
    ├── CotizacionControlDto.cs           🟡 PLANTILLA
    ├── PartidaCotizacionOrigenDto.cs     🟡 PLANTILLA
    ├── PartidaCotizacionLegacyDto.cs     🟡 PLANTILLA
    ├── EtlProcesoControlDto.cs           ✅ COPIAR (NUEVO)
    ├── MonitoreoFiltrosDto.cs             ✅ COPIAR (NUEVO)
    ├── MonitoreoResultadoDto.cs           ✅ COPIAR (NUEVO)
    ├── NotificacionEmailDto.cs            ✅ COPIAR (NUEVO)
    └── SyncJobLogDto.cs                  ✅ COPIAR
```

**Resumen Domain**:
- 🟢 Reutilizable: 80%
- 🟡 Plantillas: 15%
- 🔵 Refactorizar: 5%

---

### CAPA APPLICATION

```
Application/
│
├── 🟢 Exceptions/
│   ├── AppException.cs                    ✅ COPIAR
│   ├── AppArgumentException.cs            ✅ COPIAR
│   ├── AppArgumentNullException.cs        ✅ COPIAR
│   ├── AppKeyNotFoundException.cs         ✅ COPIAR
│   └── AppFileNotFoundException.cs        ✅ COPIAR
│
├── 🟢 Services/ (Core)
│   ├── ExceptionClassifier.cs             ✅ COPIAR (NO TOCAR)
│   ├── SyncLogService.cs                  ✅ COPIAR
│   ├── EtlProcesoControlService.cs        ✅ COPIAR (NUEVO)
│   ├── NotificationService.cs            ✅ COPIAR (NUEVO)
│   ├── MonitoreoService.cs               ✅ COPIAR (NUEVO)
│   ├── ValidacionBaseService.cs           ✅ COPIAR (NUEVO)
│   └── ISincronizacionJobService.cs       ✅ COPIAR (interfaz)
│
├── 🔵 Services/ (Jobs)
│   └── SincronizacionJobService.cs        🔵 REFACTORIZAR (hacerlo más genérico)
│
├── 🟡 Services/ (ETL Específico)
│   ├── SincronizarCotizacionService.cs    🟡 PLANTILLA
│   ├── SincronizarPartidasService.cs      🟡 PLANTILLA
│   ├── CotizacionValidacionService.cs     🟡 PLANTILLA (NUEVO)
│   ├── CotizacionSubprocesoExtractService.cs 🟡 PLANTILLA (NUEVO)
│   ├── CotizacionSubprocesoTransformService.cs 🟡 PLANTILLA (NUEVO)
│   ├── CotizacionSubprocesoLoadService.cs 🟡 PLANTILLA (NUEVO)
│   └── ISincronizarCotizacion.cs          🟡 PLANTILLA (interfaz)
│
├── 🔵 Services/ (Multiple)
│   └── SincronizacionMultipleService.cs   🔵 REFACTORIZAR (hacer genérico)
│
├── 🔴 Services/ (PoC)
│   └── ProcesoEtlMetadataService.cs       🔴 REESCRIBIR (hardcodeado)
│
├── 🟢 Factorys/
│   └── ProblemDetailsHelper.cs            ✅ COPIAR
│
├── 🟡 Interfaces/
│   ├── ISincronizarCotizacion.cs          🟡 PLANTILLA
│   ├── ISincronizarPartidasService.cs     🟡 PLANTILLA
│   ├── ISincronizacionMultipleService.cs  🔵 REFACTORIZAR
│   ├── IValidacionProcesoService.cs       ✅ COPIAR (NUEVO)
│   ├── ISubprocesoExtractService.cs       ✅ COPIAR (NUEVO)
│   ├── ISubprocesoTransformService.cs     ✅ COPIAR (NUEVO)
│   ├── ISubprocesoLoadService.cs          ✅ COPIAR (NUEVO)
│   └── ISyncLogService.cs                 ✅ COPIAR
│
└── 🟡 DTOs/
    └── ResultadoSincronizacionMultipleDto.cs ✅ COPIAR
```

**Resumen Application**:
- 🟢 Reutilizable: 60%
- 🟡 Plantillas: 25%
- 🔵 Refactorizar: 10%
- 🔴 Descartar: 5%

---

### CAPA INFRASTRUCTURE

```
Infrastructure/
│
├── 🟢 Repository/ (Base)
│   ├── GenericRepository.cs               ✅ COPIAR (NO TOCAR)
│   └── (Implementa IGenericRepository<T>)
│
├── 🟡 Repository/ (Específico)
│   ├── CotizacionOrigenRepository.cs      🟡 PLANTILLA
│   ├── CotizacionLegacyRepository.cs      🟡 PLANTILLA
│   ├── CotizacionValidacionRepository.cs  🟡 PLANTILLA (NUEVO)
│   ├── PartidaCotizacionOrigenRepository.cs   🟡 PLANTILLA
│   ├── PartidaCotizacionLegacyRepository.cs   🟡 PLANTILLA
│   ├── EtlProcesoControlRepository.cs     ✅ COPIAR (NUEVO)
│   └── SyncJobLogRepository.cs            ✅ COPIAR
│
├── 🟢 Persistence/
│   └── UnitOfWork.cs                      ✅ COPIAR
│
├── 🔴 Persistence/Contexts/ (Scaffold desde BD real)
│   ├── MicroservicioContext.cs            🔴 SCAFFOLD (nuevo)
│   ├── ProquifaDotNetContext.cs           🔴 SCAFFOLD (nuevo)
│   ├── PConnectContext.cs                 🔴 SCAFFOLD (nuevo)
│   └── PConnectProquifaDotNetContext.cs   🔴 SCAFFOLD (nuevo)
│
├── 🔴 Persistence/Entities/ (Scaffold desde BD real)
│   ├── PConnect/
│   │   ├── Cotiza.cs                      🔴 SCAFFOLD
│   │   └── PCotiza.cs                     🔴 SCAFFOLD
│   ├── PConnectProquifaDotNet/
│   │   ├── EtlProcesoControl.cs            🔴 SCAFFOLD (NUEVO)
│   │   ├── Cotizacione.cs                 🔴 SCAFFOLD
│   │   ├── SyncJobLog.cs                  🔴 SCAFFOLD
│   │   └── vETLCotizacionesPendiete.cs    🔴 SCAFFOLD
│   └── ProquifaDotNet/
│       ├── cotCotizacion.cs               🔴 SCAFFOLD
│       ├── vCotizacionesTransformadasETL.cs   🔴 SCAFFOLD
│       └── vPartidasCotizacionTransformadasETL.cs 🔴 SCAFFOLD
│
├── 🟢 Mappers/ (Base)
│   ├── ApplicationMappingProfile.cs       ✅ COPIAR
│   ├── RepositoryMappingProfile.cs        ✅ COPIAR
│   └── SyncJobLogMappingProfile.cs        ✅ COPIAR
│
└── 🟡 Mappers/ (Específico)
    ├── EtlProcesoControlMappingProfile.cs  ✅ COPIAR (NUEVO)
    └── CotizaMappingProfile.cs            🟡 PLANTILLA
```

**Resumen Infrastructure**:
- 🟢 Reutilizable: 30%
- 🟡 Plantillas: 20%
- 🔴 Scaffold/Específico: 50%

---

### CAPA API

```
API/
│
├── 🟢 ExceptionMiddleware/
│   ├── ExceptionHandlerMiddleware.cs      ✅ COPIAR (NO TOCAR)
│   └── ExceptionHandlerMiddlewareExtensions.cs ✅ COPIAR
│
├── 🟢 Filters/
│   └── AutomaticRetryFailureLogFilter.cs  ✅ COPIAR (NO TOCAR)
│
├── 🟢 Extensions/
│   └── ServiceExtensions.cs               ✅ COPIAR (estructura modular)
│       ├── ConfigureSerilog()             ✅ COPIAR
│       ├── ConfigureSwagger()             ✅ COPIAR
│       ├── ConfigureDatabases()           🔵 ADAPTAR (connection strings)
│       ├── ConfigureApplicationServices() ✅ COPIAR
│       ├── ConfigureETLServices()         🔵 ADAPTAR (agregar nuevos servicios)
│       ├── ConfigureProblemDetails()      ✅ COPIAR
│       └── ConfigureHangfire()            ✅ COPIAR
│
├── 🟡 Controllers/
│   ├── EtlController.cs                   🟡 MANTENER ESTRUCTURA
│   ├── MonitoreoController.cs              ✅ COPIAR (NUEVO)
│   └── (Agregar endpoints para nuevas entidades)
│
├── 🔴 BackgroundServices/
│   └── SincronizacionBackgroundService.cs 🔴 EVALUAR (si es necesario)
│
├── 🟢 Program.cs                          ✅ COPIAR (estructura)
│   ├── Bootstrap Serilog                  ✅ COPIAR
│   ├── ConfigureServices                  ✅ COPIAR
│   ├── Build App                          ✅ COPIAR
│   ├── Middleware Pipeline                ✅ COPIAR
│   └── ConfigureRecurringJobs             🔵 ADAPTAR
│
└── 🔵 appsettings.json                    🔵 ADAPTAR
    ├── ConnectionStrings                  🔵 ACTUALIZAR
    ├── Serilog                            ✅ COPIAR
    ├── Hangfire                           ✅ COPIAR
    ├── SincronizacionAutomatica           🔵 AJUSTAR
    └── SyncLogCleanup                     ✅ COPIAR
```

**Resumen API**:
- 🟢 Reutilizable: 70%
- 🟡 Plantillas: 10%
- 🔵 Adaptar: 15%
- 🔴 Evaluar: 5%

---

## 3. PLAN DE EXTRACCIÓN POR PRIORIDAD

### FASE 1: Fundamentos (Día 1)

**Copiar directamente** (NO modificar):
```
✅ Domain/Interfaces/IGenericRepository.cs
✅ Domain/Interfaces/IUnitOfWork.cs
✅ Domain/Interfaces/IExceptionClassifier.cs
✅ Domain/Interfaces/IEtlProcesoControlService.cs (NUEVO)
✅ Domain/Interfaces/INotificationService.cs (NUEVO)
✅ Domain/Interfaces/IMonitoreoService.cs (NUEVO)
✅ Domain/Exceptions/ (carpeta completa)
✅ Domain/Models/ErrorCategory.cs
✅ Domain/Models/EtlProcesoControl.cs (NUEVO)
✅ Domain/Enums/TipoProcesoEtl.cs
✅ Domain/DTOs/SyncJobLogDto.cs
✅ Domain/DTOs/EtlProcesoControlDto.cs (NUEVO)
✅ Domain/DTOs/MonitoreoFiltrosDto.cs (NUEVO)
✅ Domain/DTOs/MonitoreoResultadoDto.cs (NUEVO)

✅ Application/Exceptions/ (carpeta completa)
✅ Application/Services/ExceptionClassifier.cs
✅ Application/Services/SyncLogService.cs
✅ Application/Services/EtlProcesoControlService.cs (NUEVO)
✅ Application/Services/NotificationService.cs (NUEVO)
✅ Application/Services/MonitoreoService.cs (NUEVO)
✅ Application/Services/ValidacionBaseService.cs (NUEVO)
✅ Application/Factorys/ProblemDetailsHelper.cs

✅ Infrastructure/Repository/GenericRepository.cs
✅ Infrastructure/Repository/EtlProcesoControlRepository.cs (NUEVO)
✅ Infrastructure/Persistence/UnitOfWork.cs
✅ Infrastructure/Mappers/ApplicationMappingProfile.cs
✅ Infrastructure/Mappers/EtlProcesoControlMappingProfile.cs (NUEVO)
✅ Infrastructure/Mappers/SyncJobLogMappingProfile.cs
✅ Infrastructure/Repository/SyncJobLogRepository.cs

✅ API/ExceptionMiddleware/ (carpeta completa)
✅ API/Filters/AutomaticRetryFailureLogFilter.cs
✅ API/Controllers/MonitoreoController.cs (NUEVO)
```

**Tiempo estimado**: 45 minutos (incluyendo nuevos componentes)

---

### FASE 2: Configuración Base (Día 1)

**Copiar y adaptar**:
```
🔵 API/Extensions/ServiceExtensions.cs
   ↳ Mantener métodos: ConfigureSerilog, ConfigureSwagger,
     ConfigureApplicationServices, ConfigureProblemDetails, ConfigureHangfire
   ↳ Adaptar: ConfigureDatabases, ConfigureETLServices

🔵 API/Program.cs
   ↳ Mantener estructura completa
   ↳ Adaptar: ConfigureRecurringJobs

🔵 API/appsettings.json
   ↳ Copiar: Serilog, Hangfire, SyncLogCleanup
   ↳ Actualizar: ConnectionStrings, SincronizacionAutomatica
```

**Tiempo estimado**: 1 hora

---

### FASE 3: Primera Entidad (Día 2-3)

**Opción A: Reutilizar Cotización**
```
1. Copiar Domain/DTOs/Cotizacion*.cs
2. Copiar Domain/Interfaces/*Cotizacion*.cs
3. Copiar Infrastructure/Repository/*Cotizacion*.cs
4. Copiar Application/Services/SincronizarCotizacionService.cs
5. Copiar Infrastructure/Mappers/CotizaMappingProfile.cs
6. Scaffold entidades EF Core desde BD real
7. Actualizar conexiones y configuración
8. Probar ETL completo
```

**Opción B: Nueva Entidad (ej. Pedido)**
```
1. Copiar DTOs de Cotización → Renombrar a Pedido
2. Copiar Interfaces → Renombrar (agregar ValidacionService)
3. Copiar Repositorios → Adaptar queries (agregar Validacion)
4. Copiar SincronizarCotizacionService → Adaptar lógica con subprocesos
5. Copiar ValidacionBaseService → Crear PedidoValidacionService
6. Crear Subprocesos: Extract, Transform, Load específicos
7. Copiar CotizaMappingProfile → Adaptar mapeos
8. Actualizar TipoProcesoEtl enum
9. Registrar en DI (incluyendo servicios nuevos)
10. Scaffold entidades
11. Probar ETL completo con validaciones y compensación
```

**Tiempo estimado**: 2-4 horas (según complejidad de entidad)

---

### FASE 4: Jobs Hangfire (Día 3)

**Copiar y configurar**:
```
✅ API/Filters/AutomaticRetryFailureLogFilter.cs (ya copiado en Fase 1)
🔵 Application/Services/SincronizacionJobService.cs
   ↳ Copiar estructura
   ↳ Agregar casos para nuevas entidades
🔵 Application/Services/SincronizacionMultipleService.cs
   ↳ Copiar
   ↳ Considerar hacer genérico (futuro)
```

**Configurar jobs recurrentes**:
```csharp
// En Program.cs
app.ConfigureRecurringJobs(configuration);

// Agregar:
recurringJobManager.AddOrUpdate<SincronizacionJobService>(
    "sincronizar-pedidos-automatico",
    service => service.EjecutarSincronizacionPendientesRecurrente(TipoProcesoEtl.Pedido, null),
    "*/30 * * * *"
);
```

**Tiempo estimado**: 2 horas

---

## 4. TABLA DE DECISIONES RÁPIDAS

| Archivo | Verde | Amarillo | Azul | Rojo | Acción |
|---------|:-----:|:--------:|:----:|:----:|--------|
| IGenericRepository.cs | ✅ | | | | Copiar tal cual |
| GenericRepository.cs | ✅ | | | | Copiar tal cual |
| UnitOfWork.cs | ✅ | | | | Copiar tal cual |
| ExceptionClassifier.cs | ✅ | | | | Copiar tal cual, NO tocar |
| SyncLogService.cs | ✅ | | | | Copiar tal cual |
| ExceptionHandlerMiddleware.cs | ✅ | | | | Copiar tal cual |
| AutomaticRetryFailureLogFilter.cs | ✅ | | | | Copiar tal cual |
| SyncJobLogDto.cs | ✅ | | | | Copiar tal cual |
| ErrorCategory.cs | ✅ | | | | Copiar tal cual |
| TipoProcesoEtl.cs | ✅ | | | | Copiar, extender |
| | | | | | |
| CotizacionOrigenDto.cs | | 🟡 | | | Usar como plantilla |
| CotizacionOrigenRepository.cs | | 🟡 | | | Usar como plantilla |
| SincronizarCotizacionService.cs | | 🟡 | | | Usar como plantilla |
| CotizaMappingProfile.cs | | 🟡 | | | Usar como plantilla |
| | | | | | |
| SincronizacionMultipleService.cs | | | 🔵 | | Refactorizar a genérico |
| SincronizacionJobService.cs | | | 🔵 | | Simplificar switch |
| ProcesoEtlMetadataService.cs | | | 🔵 | | Hacer dinámico |
| ConfigureDatabases() | | | 🔵 | | Actualizar conexiones |
| appsettings.json | | | 🔵 | | Adaptar a entorno |
| | | | | | |
| Cotiza.cs (entity) | | | | 🔴 | Scaffold desde BD |
| PConnectContext.cs | | | | 🔴 | Scaffold desde BD |
| BackgroundService.cs | | | | 🔴 | Evaluar necesidad |

---

## 5. PATRONES DE NOMENCLATURA

### Para Nueva Entidad "{Entidad}"

**DTOs**:
```
{Entidad}OrigenDto.cs       - Representa datos desde origen
{Entidad}LegacyDto.cs       - Representa datos hacia legacy
{Entidad}ControlDto.cs      - Representa tracking de sincronización
```

**Interfaces de Repositorio**:
```
I{Entidad}OrigenRepository.cs
I{Entidad}LegacyRepository.cs
I{Entidad}ControlRepository.cs
```

**Implementaciones de Repositorio**:
```
{Entidad}OrigenRepository.cs
{Entidad}LegacyRepository.cs
{Entidad}ControlRepository.cs
```

**Servicios**:
```
Sincronizar{Entidad}Service.cs
ISincronizar{Entidad}.cs
```

**AutoMapper**:
```
{Entidad}MappingProfile.cs
```

**Enum**:
```csharp
public enum TipoProcesoEtl
{
    Cotizacion = 1,
    {Entidad} = X    // ← Agregar aquí
}
```

---

## 6. CHECKLIST DE VALIDACIÓN

### Después de copiar componentes VERDES

- [ ] Todo compila sin errores
- [ ] Namespaces actualizados al nuevo proyecto
- [ ] No hay referencias a implementaciones específicas de Cotización
- [ ] ExceptionClassifier funciona correctamente
- [ ] GenericRepository compila
- [ ] UnitOfWork funciona con DbContext

### Después de adaptar componentes AMARILLOS

- [ ] DTOs reflejan estructura de nueva entidad
- [ ] Repositorios tienen queries correctas
- [ ] Servicio ETL implementa lógica específica
- [ ] AutoMapper mapea correctamente (test unitario)
- [ ] Interfaz registrada en DI
- [ ] TipoProcesoEtl actualizado

### Después de refactorizar componentes AZULES

- [ ] SincronizacionJobService maneja nueva entidad
- [ ] appsettings.json tiene conexiones correctas
- [ ] Jobs recurrentes configurados
- [ ] Hangfire dashboard accesible

### Prueba End-to-End

- [ ] Sincronización individual funciona (POST /api/etl/sincronizar)
- [ ] Sincronización masiva funciona (POST /api/etl/sincronizar-pendientes)
- [ ] Errores transitorios se reintentan automáticamente
- [ ] Errores permanentes se loguean correctamente
- [ ] Dashboard Hangfire muestra jobs correctamente
- [ ] Logs en Serilog se escriben correctamente

---

## 7. ANTI-PATRONES A EVITAR

### ❌ NO HACER:

1. **Modificar GenericRepository o ExceptionClassifier**
   - Están probados y funcionan bien
   - Cualquier cambio puede romper lógica crítica

2. **Copiar entidades EF Core manualmente**
   - Usar `dotnet ef dbcontext scaffold` siempre
   - Las entidades deben reflejar BD real

3. **Hardcodear lógica de negocio en repositorios**
   - Repositorios = solo acceso a datos
   - Lógica de negocio = servicios de aplicación

4. **Duplicar ExceptionClassifier**
   - Un solo clasificador para toda la aplicación
   - Si necesitas lógica adicional, extender el existente

5. **Mezclar concerns en servicios ETL**
   - Separar Extract, Transform, Load en métodos privados
   - Mantener métodos públicos simples

### ✅ SÍ HACER:

1. **Seguir estructura de carpetas establecida**
   - Features/{Entidad}/
   - Mantiene código organizado

2. **Usar patrones de nomenclatura consistentes**
   - {Entidad}OrigenDto, {Entidad}LegacyDto
   - Fácil búsqueda y refactoring

3. **Escribir tests inmediatamente**
   - Test de AutoMapper profile
   - Test de ExceptionClassifier (validar clasificación)
   - Test de repositorio (con BD en memoria)

4. **Documentar decisiones de mapeo**
   - Comentar reglas de negocio en AutoMapper profiles
   - Explicar defaults y transformaciones

5. **Configurar logging detallado**
   - Log cada fase ETL (Extract, Transform, Load)
   - Incluir IDs de correlación

---

## 8. RESUMEN VISUAL

```
┌─────────────────────────────────────────────────────────┐
│  COMPONENTES A COPIAR (🟢 VERDE) = 50%                  │
├─────────────────────────────────────────────────────────┤
│  • IGenericRepository, GenericRepository, UnitOfWork    │
│  • ExceptionClassifier, SyncLogService                  │
│  • ExceptionHandlerMiddleware, Filters                  │
│  • Jerarquía de Excepciones (Domain + Application)      │
│  • ErrorCategory, SyncJobLogDto                         │
│  • ServiceExtensions (estructura), Program.cs (estructura) │
│                                                           │
│  ACCIÓN: Copiar tal cual, NO modificar                   │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  PLANTILLAS A ADAPTAR (🟡 AMARILLO) = 30%               │
├─────────────────────────────────────────────────────────┤
│  • DTOs de Cotización (Origen, Legacy, Control)         │
│  • Repositorios de Cotización                           │
│  • SincronizarCotizacionService                         │
│  • CotizaMappingProfile                                 │
│                                                           │
│  ACCIÓN: Copiar, renombrar, adaptar lógica específica    │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  COMPONENTES A REFACTORIZAR (🔵 AZUL) = 15%             │
├─────────────────────────────────────────────────────────┤
│  • SincronizacionMultipleService (hacer genérico)       │
│  • SincronizacionJobService (simplificar switch)        │
│  • ProcesoEtlMetadataService (hacer dinámico)           │
│  • appsettings.json (actualizar conexiones)             │
│                                                           │
│  ACCIÓN: Analizar, refactorizar, mejorar                 │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  COMPONENTES ESPECÍFICOS DE POC (🔴 ROJO) = 5%          │
├─────────────────────────────────────────────────────────┤
│  • Entidades EF Core (scaffold desde BD real)           │
│  • DbContexts (scaffold desde BD real)                  │
│  • BackgroundService (evaluar necesidad)                │
│                                                           │
│  ACCIÓN: NO copiar, generar nuevos o evaluar            │
└─────────────────────────────────────────────────────────┘
```

---

**Fecha**: 2025-12-10
**Versión**: 1.0
**Próximo paso**: Ejecutar FASE 1 del Plan de Extracción
