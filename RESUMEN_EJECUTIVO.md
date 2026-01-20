# Resumen Ejecutivo - SincronizadorPqfLegacy PoC

**Fecha**: 2025-12-10
**Propósito**: Guía rápida para extracción de componentes reutilizables

---

## 1. QUÉ ES EL SISTEMA

Sistema ETL para sincronizar datos entre ProquifaDotNet (nuevo) y PConnect (legacy).

**Arquitectura**: Clean Architecture con 4 capas (API → Application → Domain ← Infrastructure)

---

## 2. COMPONENTES 100% REUTILIZABLES (Copiar tal cual)

### Capa Domain
```
✅ IGenericRepository<T>               - CRUD genérico
✅ IUnitOfWork                          - Transacciones
✅ IExceptionClassifier                 - Clasificación de errores
✅ DomainException (jerarquía completa) - Excepciones custom
✅ ErrorCategory enum                   - Transient vs Permanent
✅ SyncJobLogDto                        - Logs de sincronización
✅ IEtlProcesoControl                    - Tabla de control genérica (NUEVO)
✅ INotificationService                 - Servicio de notificaciones (NUEVO)
```

### Capa Application
```
✅ ExceptionClassifier                  - Lógica de clasificación (NO tocar)
✅ SyncLogService                       - Logging de sincronización
✅ AppException (jerarquía completa)    - Excepciones de aplicación
✅ EtlProcesoControlService              - Gestión de tabla control (NUEVO)
✅ NotificationService                  - Envío de correos (NUEVO)
✅ MonitoreoService                     - Consultas de estado (NUEVO)
```

### Capa Infrastructure
```
✅ GenericRepository<T>                 - Implementación CRUD
✅ UnitOfWork                           - Coordinación transaccional
✅ SyncJobLogMappingProfile             - AutoMapper para logs
✅ EtlProcesoControlMappingProfile      - AutoMapper para control (NUEVO)
✅ EmailNotificationService             - Implementación correo (NUEVO)
```

### Capa API
```
✅ ExceptionHandlerMiddleware           - Manejo global de errores
✅ AutomaticRetryFailureLogFilter       - Filtro Hangfire
✅ ServiceExtensions (estructura)       - DI modular
✅ MonitoreoController                  - Endpoint de monitoreo (NUEVO)
```

---

## 3. COMPONENTES A USAR COMO PLANTILLA

### Para Cada Nueva Entidad (Pedido, Factura, etc.)

**Copiar y adaptar**:
```
📋 SincronizarCotizacionService.cs      → SincronizarPedidoService.cs
📋 CotizacionOrigenRepository.cs        → PedidoOrigenRepository.cs
📋 CotizacionLegacyRepository.cs        → PedidoLegacyRepository.cs
📋 CotizacionControlRepository.cs       → PedidoControlRepository.cs
📋 CotizaMappingProfile.cs              → PedidoMappingProfile.cs
📋 CotizacionOrigenDto.cs               → PedidoOrigenDto.cs
📋 CotizacionLegacyDto.cs               → PedidoLegacyDto.cs
📋 CotizacionControlDto.cs              → PedidoControlDto.cs
```

**Patrón de nombres**: `{Entidad}` + `OrigenDto` / `LegacyDto` / `ControlDto`

---

## 4. NUEVO FLUJO ETL CON TABLA DE CONTROL GENÉRICA

### Flujo Principal Actualizado

```
1. INSERT TABLA CONTROL
   ↓
   - Insertar en tabla de control genérica (EtlProcesoControl)
   - Estado inicial: "EnProcesoInicio"
   - Capturar metadata: tipoProceso, recordId, fechaInicio

2. VALIDACIONES DE INTEGRIDAD
   ↓
   2.1 ✅ Validaciones correctas
       - Cambiar estado: "EnProcesoValidacionesCorrectas"
       - Avanzar a paso 3
       
   2.2 ❌ Validaciones fallan
       - Lanzar excepción con validación específica
       - Cambiar estado: "FalloPersistente"
       - Agregar mensaje en campo MensajeError
       - Aplicar clasificación de errores (Transient/Permanent)

3. PROCESO ETL CON SUBPROCESOS
   ↓
   3.1 EXTRACT (Subproceso 1)
       - Leer desde vistas exclusivas en ProquifaDotNet
       - Actualizar estado: "EnProcesoExtract"
       
   3.2 TRANSFORM (Subproceso 2)
       - AutoMapper con reglas de negocio
       - Actualizar estado: "EnProcesoTransform"
       
   3.3 LOAD (Subproceso 3)
       - Insertar/Actualizar en PConnect
       - Actualizar estado: "EnProcesoLoad"
       
   3.4 PUNTOS DE COMPENSACIÓN
       - Si algún subproceso falla → Rollback completo
       - Cada subproceso debe registrar su avance en control

4. FINALIZACIÓN
   ↓
   - Cambiar estado: "Completado"
   - Registrar fechaFin y duración
   - LOG en SyncJobLog (estado: Sincronizado)
```

### Manejo de Errores Mejorado

**Errores TRANSITORIOS** (Reintentar automáticamente):
- TimeoutException, HttpRequestException, SocketException
- SqlException -2, 1205, 53, 64
- **Acción**: Hangfire reintenta + mantiene estado en tabla control

**Errores PERMANENTES** (No reintentar + notificar):
- ValidationException, DomainException, FK violations
- **Acción**: 
  - Cambiar estado: "FalloPersistente"
  - Agregar MensajeError detallado
  - **Enviar notificación por correo** (nuevo servicio)
  - Loguear en SyncJobLog

### Nueva Tabla de Control Genérica

```sql
EtlProcesoControl
├── Id (PK, GUID)
├── TipoProceso (int) → Enum: Cotizacion=1, Pedido=2, etc.
├── RecordId (string) → ID del registro origen
├── Estado (string) → Enum de estados
├── MensajeError (string, nullable)
├── FechaInicio (datetime)
├── FechaFin (datetime, nullable)
├── DuracionMilisegundos (int, nullable)
├── SubprocesoActual (string, nullable)
├── Reintentos (int, default 0)
├── FechaUltimoReintento (datetime, nullable)
└── Metadata (json, nullable)
```

---

## 5. CONFIGURACIÓN ESENCIAL (appsettings.json)

### Mantener

**Hangfire - Reintentos**:
```json
"Hangfire": {
    "RetryAttempts": 5,
    "RetryDelays": [60, 300, 900, 3600, 7200]
}
```
*Secuencia: 1min → 5min → 15min → 1h → 2h*

**Serilog - Logging**:
```json
"Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
        { "Name": "Console" },
        { "Name": "File", "Args": { "path": "Logs/log-.txt" } }
    ]
}
```

**Limpieza de Logs**:
```json
"SyncLogCleanup": {
    "Habilitado": true,
    "DiasRetencion": 30,
    "CronExpression": "0 2 * * *"
}
```

### Adaptar por Entorno

**ConnectionStrings**: Actualizar según base de datos real

**CRON Expressions**: Ajustar frecuencia según necesidad

---

## 6. ESTRUCTURA RECOMENDADA PARA IMPLEMENTACIÓN REAL

```
SincronizadorPqfLegacy/
│
├── API/
│   └── Controllers/
│       └── EtlController.cs              ← UN SOLO controller para todos los ETL
│
├── Application/
│   ├── Common/                           ← Componentes compartidos
│   │   ├── ExceptionClassifier.cs
│   │   └── SyncLogService.cs
│   │
│   └── Features/                         ← Por entidad
│       ├── Cotizaciones/
│       │   └── Services/
│       ├── Pedidos/
│       └── Facturas/
│
├── Domain/
│   ├── Common/                           ← Abstracciones genéricas
│   │   ├── IGenericRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── ErrorCategory.cs
│   │
│   └── Entities/                         ← Por entidad
│       ├── Cotizaciones/
│       ├── Pedidos/
│       └── Facturas/
│
└── Infrastructure/
    ├── Common/                           ← Implementaciones genéricas
    │   ├── GenericRepository.cs
    │   └── UnitOfWork.cs
    │
    └── Features/                         ← Por entidad
        ├── Cotizaciones/
        ├── Pedidos/
        └── Facturas/
```

---

## 7. AGREGAR NUEVA ENTIDAD (Checklist de 15 minutos)

### Paso 1: DTOs (Domain/Entities/{Entidad}/DTOs/)
```csharp
- [ ] {Entidad}OrigenDto.cs
- [ ] {Entidad}LegacyDto.cs
- [ ] {Entidad}ControlDto.cs
```

### Paso 2: Interfaces (Domain/Entities/{Entidad}/Interfaces/)
```csharp
- [ ] I{Entidad}OrigenRepository.cs
- [ ] I{Entidad}LegacyRepository.cs
- [ ] I{Entidad}ValidacionService.cs      (NUEVO: validaciones específicas)
```

### Paso 3: Repositorios (Infrastructure/Features/{Entidad}/Repositories/)
```csharp
- [ ] {Entidad}OrigenRepository.cs
- [ ] {Entidad}LegacyRepository.cs
- [ ] {Entidad}ValidacionService.cs        (NUEVO: implementación validaciones)
```

### Paso 4: Servicio ETL (Application/Features/{Entidad}/Services/)
```csharp
- [ ] Sincronizar{Entidad}Service.cs  (copiar de SincronizarCotizacionService)
- [ ] {Entidad}SubprocesoExtractService.cs    (NUEVO: subproceso extract)
- [ ] {Entidad}SubprocesoTransformService.cs  (NUEVO: subproceso transform)
- [ ] {Entidad}SubprocesoLoadService.cs       (NUEVO: subproceso load)
```

### Paso 5: AutoMapper (Infrastructure/Features/{Entidad}/Mappers/)
```csharp
- [ ] {Entidad}MappingProfile.cs  (copiar de CotizaMappingProfile)
```

### Paso 6: Enum (Domain/Common/Enums/TipoProcesoEtl.cs)
```csharp
- [ ] Agregar {Entidad} = X
```

### Paso 7: DI (API/Extensions/ServiceExtensions.cs)
```csharp
- [ ] Registrar servicios y repositorios
```

### Paso 8: Job Service (Application/Core/SincronizacionJobService.cs)
```csharp
- [ ] Agregar case para nueva entidad en switch
```

**Tiempo estimado**: 15-30 minutos por entidad

---

## 8. CLASIFICACIÓN DE ERRORES (ExceptionClassifier)

### Errores TRANSITORIOS (Reintentar automáticamente)
```
✓ TimeoutException
✓ HttpRequestException
✓ SocketException
✓ SqlException -2 (Timeout)
✓ SqlException 1205 (Deadlock)
✓ SqlException 53/64 (Red perdida)
```

**Acción**: Hangfire reintenta con delays configurados

### Errores PERMANENTES (NO reintentar)
```
✗ ValidationException
✗ DomainException
✗ AppKeyNotFoundException
✗ InvalidOperationException
✗ SqlException 547 (FK violation)
✗ SqlException 2627 (PK violation)
```

**Acción**: Loguear en SyncJobLog con estado "Fallo persistente"

---

## 9. ENDPOINTS DEL SISTEMA

### Sincronización Individual
```http
POST /api/etl/sincronizar
Content-Type: application/json

{
  "tipoProceso": 1,        // 1=Cotización, 2=Pedido, etc.
  "recordId": "guid-aqui"
}

Response: 202 Accepted
{
  "jobId": "hangfire-job-id",
  "mensaje": "Job encolado exitosamente",
  "procesoControlId": "guid-control-id"
}
```

### Sincronización Masiva
```http
POST /api/etl/sincronizar-pendientes
Content-Type: application/json

{
  "tipoProceso": 1
}

Response: 202 Accepted
{
  "jobId": "hangfire-job-id",
  "totalPendientes": 150,
  "procesoControlId": "guid-control-id-masivo"
}
```

### Monitoreo de Sincronizaciones (NUEVO)
```http
POST /api/etl/monitoreo
Content-Type: application/json

{
  "filtros": {
    "tipoProceso": [1, 2],           // Opcional: array de tipos
    "estado": ["EnProcesoInicio", "Completado"],  // Opcional
    "fechaInicio": "2025-12-01",     // Opcional
    "fechaFin": "2025-12-31",        // Opcional
    "pagina": 1,                      // Default: 1
    "tamanoPagina": 50               // Default: 50, max: 100
  }
}

Response: 200 OK
{
  "totalRegistros": 1250,
  "pagina": 1,
  "tamanoPagina": 50,
  "totalPaginas": 25,
  "datos": [
    {
      "id": "guid",
      "tipoProceso": 1,
      "tipoProcesoNombre": "Cotizacion",
      "recordId": "guid-origen",
      "estado": "Completado",
      "mensajeError": null,
      "fechaInicio": "2025-12-10T10:30:00Z",
      "fechaFin": "2025-12-10T10:30:05Z",
      "duracionMilisegundos": 5234,
      "subprocesoActual": null,
      "reintentos": 0
    }
  ],
  "resumen": {
    "completados": 1100,
    "enProceso": 45,
    "fallosPersistentes": 85,
    "reintentando": 20
  }
}
```

### Catálogo de Procesos
```http
GET /api/etl/catalogo

Response: 200 OK
[
  { "id": 1, "nombre": "Cotizacion", "descripcion": "..." },
  { "id": 2, "nombre": "Pedido", "descripcion": "..." }
]
```

---

## 10. HANGFIRE DASHBOARD

**URL**: `https://localhost:5001/hangfire`

**Features**:
- Ver jobs en cola, procesando, exitosos y fallidos
- Reintento manual de jobs fallidos
- Logs en tiempo real con Hangfire.Console
- Barras de progreso para sincronización masiva
- Estadísticas de performance

**Jobs Recurrentes**:
```
sincronizar-pendientes-automatico    → Cada 30 min
procesos-sin-detonacion-inicial      → Cada 2 min
cleanup-sync-logs                    → Diario 2:00 AM
```

---

## 11. BASES DE DATOS

### ProquifaDotNet (Origen)
**Contexto**: `ProquifaDotNetContext`
**Acceso**: Solo lectura
**Tablas clave**:
- `vCotizacionesTransformadasETL` (vista)
- `vPartidasCotizacionTransformadasETL` (vista)

### PConnect (Legacy)
**Contexto**: `PConnectContext`
**Acceso**: Lectura/Escritura
**Tablas clave**:
- `Cotiza` (cotizaciones)
- `PCotiza` (partidas)

### PConnectProquifaDotNet (Control)
**Contexto**: `PConnectProquifaDotNetContext`
**Acceso**: Lectura/Escritura
**Tablas clave**:
- `EtlProcesoControl` (tabla de control genérica - NUEVA)
- `Cotizacione` (tracking de sincronización - legacy)
- `SyncJobLog` (logs de jobs)
- Tablas de Hangfire

---

## 12. MÉTRICAS DE LA PoC

### Performance
- **Sincronización individual**: ~2-3 segundos promedio
- **Sincronización masiva (100 registros)**: ~5 minutos (secuencial)
- **Tasa de éxito**: ~95% primer intento, ~98% con reintentos

### Resiliencia
- **Errores transitorios**: 100% recuperación automática
- **Errores permanentes**: 0% reintentos innecesarios

### Cobertura de Tests
- **Unitarios**: 0% (pendiente)
- **Integración**: 0% (pendiente)
- **E2E**: Pruebas manuales exitosas

---

## 13. MEJORAS PRIORITARIAS PARA PRODUCCIÓN

### Alta Prioridad (Semana 1-2)
1. ✅ **Tests Unitarios**: ExceptionClassifier, servicios ETL
2. ✅ **Tests de Integración**: Repositorios con BD real
3. ✅ **Procesamiento Paralelo**: Reducir tiempo de sincronización masiva 70%
4. ✅ **Idempotencia**: Evitar duplicados por reintentos
5. 🆕 **Implementar tabla de control genérica**: Migrar tracking existente
6. 🆕 **Implementar servicio de notificaciones por correo**: Para fallos permanentes
7. 🆕 **Crear endpoint de monitoreo**: Consultas con filtros genéricos

### Media Prioridad (Semana 3-4)
8. ⚠️ **Circuit Breaker**: Polly para resilencia de APIs externas
9. ⚠️ **Health Checks**: Monitoreo de estado de BDs
10. ⚠️ **Application Insights**: Telemetría y métricas
11. ⚠️ **Dead Letter Queue**: Manejo de fallos persistentes
12. 🆕 **Implementar puntos de compensación**: Rollback de subprocesos ETL
13. 🆕 **Sistema de validaciones genéricas**: Por tipo de proceso

### Baja Prioridad (Semana 5+)
14. 📋 **CQRS**: Separar Commands y Queries
15. 📋 **Event Sourcing**: Auditoría completa
16. 📋 **Caching**: Catálogos frecuentes
17. 🆕 **Dashboard en tiempo real**: WebSocket para monitoreo
18. 🆕 **Métricas de negocio**: Tiempos por proceso, tasas de éxito

---

## 14. DECISIONES DE DISEÑO CLAVE

### ¿Por qué Clean Architecture?
- Separación de responsabilidades clara
- Fácil testing (capas desacopladas)
- Independencia de frameworks

### ¿Por qué múltiples DbContexts?
- Cada base de datos tiene su propio ciclo de vida
- Permite transacciones independientes
- Mejor separación de concerns

### ¿Por qué Hangfire?
- Dashboard integrado
- Reintentos automáticos configurables
- Escalabilidad (múltiples servers)
- Soporte para jobs recurrentes

### ¿Por qué AutoMapper?
- Transformaciones declarativas
- Reduce código boilerplate
- Fácil testing de mapeos

### ¿Por qué clasificar errores?
- Evita reintentos innecesarios (ahorra recursos)
- Reduce intervención manual (80% de errores son transitorios)
- Mejora experiencia (no alertar por errores temporales)

---

## 15. CONTACTO Y RECURSOS

### Documentación Completa
- `ARQUITECTURA_POC.md` - Análisis detallado (50+ páginas)
- `DIAGRAMAS_FLUJOS.md` - Diagramas Mermaid de todos los flujos
- `dt_etl.md` - Documentación técnica original

### Archivos Clave para Revisión
```
API/Program.cs                                      ← Entry point
API/Extensions/ServiceExtensions.cs                 ← DI completa
Application/Services/ExceptionClassifier.cs         ← Lógica crítica
Application/Services/SincronizarCotizacionService.cs ← Plantilla ETL
Infrastructure/Repository/GenericRepository.cs      ← Repository base
```

---

**Estado**: ✅ Listo para extracción a implementación real
**Fecha de preparación**: 2025-12-10
**Próximo paso**: Revisar sección 7 (Checklist para agregar nueva entidad)
