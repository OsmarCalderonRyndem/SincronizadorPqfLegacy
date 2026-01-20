# Diagrama Completo de Manejo de Errores y Flujos de Reintento

**Propósito**: Visualización detallada de todos los flujos de error, clasificación y mecanismos de reintento
**Fecha**: 2025-12-19
**Versión**: 2.0

---

## 🔄 Diagrama Unificado de Manejo de Errores y Reintentos

```mermaid
%%{init: {'theme':'base', 'themeVariables': { 'background':'#ffffff'}}}%%
graph TB
    Start([Inicio Sincronización]) --> InsertControl[INSERT SyncJobLog<br/>Estado: EnProcesoInicio<br/>ProcessId: ID del Proceso]
    InsertControl --> Validation{Validaciones<br/>de Integridad}

    Validation -->|✅ Correctas| ValidationOK[Estado: EnProcesoValidacionesCorrectas]
    Validation -->|❌ Falla| ValidationFail[Validación Exception]
    
    ValidationOK --> ETLProcess[PROCESO ETL<br/>Extract → Transform → Load]
    
    subgraph "SUBPROCESOS ETL"
        ETLProcess --> Subprocess1[SUBPROCESO 1: EXTRACT<br/>Estado: EnProcesoExtract]
        Subprocess1 --> Error1{Error en Extract?}
        Error1 -->|✅ Éxito| Subprocess2[SUBPROCESO 2: TRANSFORM<br/>Estado: EnProcesoTransform]
        Error1 -->|❌ Error| HandleError[Manejar Error]
        
        Subprocess2 --> Error2{Error en Transform?}
        Error2 -->|✅ Éxito| Subprocess3[SUBPROCESO 3: LOAD<br/>INSERT/UPDATE<br/>Estado: EnProcesoLoad]
        Error2 -->|❌ Error| HandleError
        
        Subprocess3 --> Error3{Error en Load?}
        Error3 -->|✅ Éxito| Success[Proceso Completado<br/>Estado: Completado]
        Error3 -->|❌ Error| RollbackLoad[ROLLBACK Completo<br/>Solo en LOAD + Foto Parcial]
    end

    HandleError --> ClassifyError[ExceptionClassifier<br/>+ Actualizar Reintentos++]
    RollbackLoad --> ClassifyError[ExceptionClassifier<br/>+ Actualizar Reintentos++]
    
    ClassifyError --> ErrorType{Tipo de Error}
    ErrorType -->|Transient| RetryFlow[Reintentar Transient]
    ErrorType -->|Permanent| PermanentFlow[Error Permanent]
    
    ValidationFail --> ClassifyValidation[Validación = Permanent]
    ClassifyValidation --> PermanentFlow

    subgraph "FLUJO DE REINTENTOS"
        RetryFlow --> CheckRetry{Reintentos < 5?}
        CheckRetry -->|Sí| ScheduleRetry[Hangfire.Schedule<br/>Delays: 60s→7200s]
        CheckRetry -->|No| MaxRetries[Exceder Reintentos → Permanent]
        
        ScheduleRetry --> Delay[Esperar Delay]
        Delay --> RetryProcess[Hangfire.Server<br/>Ejecuta Retry]
        RetryProcess --> UpdateRetry[Actualizar Control<br/>FechaUltimoReintento]
        UpdateRetry --> ETLProcess
    end

    subgraph "FLUJO PERMANENTE"
        PermanentFlow --> EmailNotification[NotificationService<br/>Enviar Email]
        EmailNotification --> LogPermanent[SyncLogService<br/>Log Permanent Failure]
        LogPermanent --> UpdatePermanent[Actualizar Control<br/>Estado: FalloPersistente<br/>+ MensajeError]
        UpdatePermanent --> EndError([Fin con Error Persistente])
        
        MaxRetries --> PermanentFlow
    end

    subgraph "PROCESO EXITOSO"
        Success --> CaptureSnapshot[Capturar Foto ETL<br/>Datos procesados en JSON]
        CaptureSnapshot --> LogSuccess[SyncLogService<br/>Log Success + Foto]
        LogSuccess --> UpdateSuccess[Actualizar Control<br/>FechaFin, Duración]
        UpdateSuccess --> EndSuccess([Fin Exitoso])
    end

    subgraph "ENDPOINTS DE REINTENTO"
        ManualRetry["POST /api/monitoreo/registro/{processId}/reintentar"]
        BatchRetry["POST /api/monitoreo/reintentar-lote"]
        AutoRetry["Job Recurrente / ProcesarPendientes"]
        
        ManualRetry --> RetryProcess
        BatchRetry --> RetryProcess
        AutoRetry --> RetryProcess
    end

    style Start fill:#c8e6c9,stroke:#333,stroke-width:2px
    style EndSuccess fill:#c8e6c9,stroke:#333,stroke-width:2px
    style EndError fill:#ffcdd2,stroke:#333,stroke-width:2px
    style Validation fill:#fff3e0,stroke:#333,stroke-width:2px
    style ValidationOK fill:#c8e6c9,stroke:#333,stroke-width:2px
    style ValidationFail fill:#ffcdd2,stroke:#333,stroke-width:2px
    style ETLProcess fill:#bbdefb,stroke:#333,stroke-width:2px
    style Subprocess1 fill:#bbdefb,stroke:#333,stroke-width:2px
    style Subprocess2 fill:#bbdefb,stroke:#333,stroke-width:2px
    style Subprocess3 fill:#bbdefb,stroke:#333,stroke-width:2px
    style Success fill:#c8e6c9,stroke:#333,stroke-width:2px
    style CaptureSnapshot fill:#e8f5e8,stroke:#333,stroke-width:2px
    style ErrorType fill:#ffe0b2,stroke:#333,stroke-width:2px
    style RetryFlow fill:#fff3e0,stroke:#333,stroke-width:2px
    style PermanentFlow fill:#ffcdd2,stroke:#333,stroke-width:2px
    style MaxRetries fill:#ff9800,stroke:#333,stroke-width:2px
    style RollbackLoad fill:#ff9800,stroke:#333,stroke-width:2px
    style ScheduleRetry fill:#bbdefb,stroke:#333,stroke-width:2px
    style EmailNotification fill:#f44336,stroke:#333,stroke-width:2px
    style ManualRetry fill:#e3f2fd,stroke:#333,stroke-width:2px
    style BatchRetry fill:#e3f2fd,stroke:#333,stroke-width:2px
    style AutoRetry fill:#e3f2fd,stroke:#333,stroke-width:2px
```

---

## 🔄 Secuencia Detallada de Reintentos Automáticos

```mermaid
%%{init: {'theme':'base', 'themeVariables': { 'background':'#ffffff'}}}%%
sequenceDiagram
    participant Client as Cliente/Usuario
    participant API as ETLController
    participant Hangfire as Hangfire Server
    participant JobService as SincronizacionJobService
    participant ETLService as SincronizarEntidadService
    participant Control as EtlProcesoControlService
    participant Classifier as ExceptionClassifier
    participant Notification as NotificationService
    participant Monitor as MonitoreoService
    participant DB as Base de Datos

    Note over API,DB: ESCENARIO 1: PRIMERA EJECUCIÓN
    Client->>API: POST /api/etl/sincronizar<br/>{tipoProceso: 1, processId: "guid"}
    API->>Hangfire: BackgroundJob.Enqueue()
    Hangfire-->>API: JobId
    API-->>Client: 202 Accepted {jobId}

    Note over API,DB: ESCENARIO 2: ERROR TRANSIENTE
    Hangfire->>JobService: EjecutarSincronizacion(tipoProceso, processId)
    JobService->>Control: CrearProcesoAsync(tipoProceso, processId)
    Control->>DB: INSERT EtlProcesoControl
    DB-->>Control: ProcesoControlId
    Control-->>JobService: procesoControlId

    JobService->>ETLService: SincronizarAsync(processId, procesoControlId)
    ETLService->>Control: ActualizarSubprocesoAsync("Extract")
    Control->>DB: UPDATE estado = 'EnProcesoExtract'
    DB-->>Control: OK

    ETLService->>DB: SELECT datos FROM vista_origen
    DB-->>ETLService: SqlException: Timeout
    
    ETLService->>Classifier: ClassifyException(exception)
    Classifier-->>ETLService: ErrorCategory.Transient
    
    ETLService->>Control: RegistrarIntentoFallidoAsync(procesoControlId, exception)
    Control->>DB: UPDATE reintentos++, mensajeError
    DB-->>Control: OK

    ETLService-->>JobService: throw (para que Hangfire reintente)
    JobService->>Hangfire: Job Failed (Transient)

    Note over API,DB: ESCENARIO 3: REINTENTO AUTOMÁTICO
    Hangfire->>Hangfire: Esperar delay configurado<br/>60s → 300s → 900s → 3600s → 7200s
    Hangfire->>JobService: ReintentarSincronizacion(tipoProceso, processId, intento: 2)
    JobService->>Monitor: ObtenerUltimoRegistroPorEntidadAsync(tipoProceso, processId)
    Monitor->>DB: SELECT * FROM vSyncJobLogUnificado
    DB-->>Monitor: Registro con estado actual
    Monitor-->>JobService: registroPrevio

    JobService->>Control: ActualizarReintentoAsync(registroPrevio.ProcesoControlId)
    Control->>DB: UPDATE fechaUltimoReintento
    DB-->>Control: OK

    JobService->>ETLService: ReintentarSincronizacionAsync(processId, procesoControlId)
    ETLService->>DB: SELECT datos FROM vista_origen
    DB-->>ETLService: Datos exitosos
    ETLService->>Control: ActualizarSubprocesoAsync("Transform")
    Control->>DB: UPDATE estado = 'EnProcesoTransform'
    DB-->>Control: OK

    Note over API,DB: ESCENARIO 4: ERROR PERMANENTE EN LOAD
    ETLService->>DB: INSERT/UPDATE INTO tabla_destino
    DB-->>ETLService: SqlException: FK violation
    
    ETLService->>Classifier: ClassifyException(exception)
    Classifier-->>ETLService: ErrorCategory.Permanent

    ETLService->>ETLService: EjecutarRollbackCompleto()<br/>Solo en LOAD afecta BD destino
    ETLService->>ETLService: Capturar Foto Parcial (Datos antes del error)
    ETLService->>Control: RegistrarFalloPersistenteAsync(procesoControlId, exception)
    Control->>DB: UPDATE estado = 'FalloPersistente', mensajeError
    DB-->>Control: OK

    ETLService->>Notification: EnviarNotificacionFalloPermanenteAsync(registro, exception)
    Notification->>Notification: ConstruirEmail(plantilla, datos)
    Notification->>Notification: EnviarEmail(destinatarios)
    Notification-->>ETLService: Email enviado

    ETLService->>LogService: LogErrorConSnapshotAsync<br/>+ Foto parcial
    ETLService->>Control: CompletarConErrorAsync(procesoControlId)
    Control->>DB: UPDATE fechaFin, duracion
    DB-->>Control: OK

    ETLService-->>JobService: Proceso con fallo persistente
    JobService->>Hangfire: Job Failed (Permanent - NO reintentar)

    Note over API,DB: ESCENARIO 5: REINTENTO MANUAL DESDE MONITOREO
    Client->>Monitor: GET /api/monitoreo/registros<br/>{filtros: {categoriaEstado: "ConErrores"}}
    Monitor->>DB: SELECT FROM vSyncJobLogUnificado
    DB-->>Monitor: Lista de errores
    Monitor-->>Client: Registros con errores

    Client->>Monitor: POST /api/monitoreo/registro/{processId}/reintentar
    Monitor->>Hangfire: BackgroundJob.Enqueue(SincronizacionJobService.EjecutarSincronizacion)
    Hangfire-->>Monitor: JobId
    Monitor-->>Client: 200 OK {jobId, mensaje}

    Hangfire->>JobService: EjecutarSincronizacion(tipoProceso, processId) - Manual
    Note right of JobService: Continúa desde ESCENARIO 2<br/>pero reintentos++ = 6<br/>y Reset de lógica de reintento

    Note over API,DB: ESCENARIO 6: REINTENTO AUTOMÁTICO PROGRAMADO
    Note over Hangfire: JOB RECURRENTE CADA 30 MINUTOS
    Hangfire->>JobService: ProcesarPendientesAutomaticosJob()
    JobService->>Monitor: ObtenerRegistrosParaReintentarAsync(limite: 100)
    Monitor->>DB: SELECT errores con reintentos < 5<br/>AND fechaUltimoReintento < DATEADD(HOUR, -1)
    DB-->>Monitor: Registros para reintentar
    Monitor-->>JobService: listaRegistros

    loop Para cada registro
        JobService->>Hangfire: BackgroundJob.Enqueue(SincronizacionJobService.EjecutarSincronizacion)
        Note right of JobService: Reintento automático en batch
    end

    Note over API,DB: ESCENARIO 7: LIMPIEZA Y MANTENIMIENTO
    Note over Hangfire: JOB DIARIO 2:00 AM
    Hangfire->>JobService: CleanupSyncLogsJob()
    JobService->>DB: DELETE SyncJobLog WHERE fechaCreacion < DATEADD(DAY, -30)
    DB-->>JobService: Registros eliminados
    
    JobService->>DB: DELETE EtlProcesoControl WHERE fechaInicio < DATEADD(DAY, -90)<br/>AND estado IN ('Completado', 'FalloPersistente')
    DB-->>JobService: Registros eliminados

```

---

## 📊 Estados del Proceso y Transiciones

```mermaid
%%{init: {'theme':'base', 'themeVariables': { 'background':'#ffffff'}}}%%
stateDiagram-v2
    [*] --> EnProcesoInicio: Iniciar Sincronización
    
    EnProcesoInicio --> EnProcesoValidacionesCorrectas: Validaciones OK
    EnProcesoInicio --> FalloPersistente: Validación falla
    
    EnProcesoValidacionesCorrectas --> EnProcesoExtract: Iniciar Extract
    EnProcesoValidacionesCorrectas --> FalloPersistente: Error validation post
    
    EnProcesoExtract --> EnProcesoTransform: Extract OK
    EnProcesoExtract --> Reintentando: Error transient
    EnProcesoExtract --> FalloPersistente: Error permanent
    
    EnProcesoTransform --> EnProcesoLoad: Transform OK
    EnProcesoTransform --> Reintentando: Error transient
    EnProcesoTransform --> FalloPersistente: Error permanent
    
    EnProcesoLoad --> Completado: Load OK
    EnProcesoLoad --> Reintentando: Error transient
    EnProcesoLoad --> FalloPersistente: Error permanent
    
    Reintentando --> EnProcesoExtract: Reintentar desde inicio
    Reintentando --> FalloPersistente: Exceder 5 reintentos
    
    Completado --> [*]: Fin exitoso
    FalloPersistente --> [*]: Fin con error
    
    note right of Reintentando
        Estado temporal mientras
        espera el próximo reintento
        programado por Hangfire
    end note
    
    note right of FalloPersistente
        Estado final con:
        - MensajeError detallado
        - Notificación por email
        - No más reintentos automáticos
        - Solo reintento manual
    end note
```

---

## 🎯 Configuración de Reintentos por Tipo de Error

### Configuración en appsettings.json

```json
{
  "Sincronizacion": {
    "Reintentos": {
      "MaximoReintentos": 5,
      "DelaysSegundos": [60, 300, 900, 3600, 7200],
      "EstrategiaBackoff": "Exponential"
    },
    "ClasificacionErrores": {
      "Transient": [
        "TimeoutException",
        "SqlException:-2",
        "SqlException:1205",
        "HttpRequestException",
        "SocketException",
        "TaskCanceledException"
      ],
      "Permanent": [
        "ValidationException",
        "DomainException",
        "InvalidOperationException",
        "SqlException:547",
        "SqlException:2627",
        "SqlException:2601"
      ]
    },
    "Notificaciones": {
      "Email": {
        "Habilitado": true,
        "DestinatariosPorDefecto": [
          "admin@empresa.com",
          "soporte@empresa.com"
        ],
        "Plantillas": {
          "Validacion": "ErrorValidacionTemplate",
          "ErrorPermanent": "ErrorPermanentTemplate",
          "ExcesoReintentos": "ExcesoReintentosTemplate"
        }
      }
    }
  },
  "Hangfire": {
    "JobsRecurrentes": {
      "ProcesarPendientes": "0 */30 * * * *",
      "ProcesarSinDetonacion": "0 */2 * * * *",
      "CleanupLogs": "0 0 2 * * *"
    }
  }
}
```

---

## 🔧 Mecanismos de Reintento

### 1. Reintento Automático (Hangfire)
- **Disparador**: Error clasificado como Transient
- **Scheduling**: `BackgroundJob.Schedule` con delays configurados
- **Seguimiento**: Actualiza contador y timestamp en EtlProcesoControl
- **Límite**: Máximo 5 reintentos por registro

### 2. Reintento Manual (Monitoreo)
- **Disparador**: Usuario desde endpoint de monitoreo
- **Scheduling**: `BackgroundJob.Enqueue` inmediato
- **Seguimiento**: No incrementa contador de reintentos automáticos
- **Límite**: Ilimitado (control manual)

### 3. Reintento Programado (Batch)
- **Disparador**: Job recurrente cada 30 minutos
- **Scheduling**: Procesa hasta 100 registros por batch
- **Criterios**: 
  - Estado "ConErrores"
  - Reintentos < 5
  - Último reintento hace más de 1 hora
  - No ser "FalloPersistente" (a menos que sea timeout)

### 4. Reintento desde Validación
- **Disparador**: Error en validaciones iniciales
- **Comportamiento**: Directamente a fallo persistente
- **Notificación**: Email inmediato
- **Motivo**: Las validaciones fallidas no se resuelven solas

---

## 📧 Plantillas de Notificación

### Template 1: Validación Fallida
```
Asunto: [ERROR] Fallo de Validación - {TipoProcesoNombre} - {Folio}

Hola Equipo,

Se ha detectado un fallo en las validaciones de sincronización:

📋 Detalles del Registro:
- Tipo Proceso: {TipoProcesoNombre}
- Process ID: {ProcessId}
- ID Registro: {RecordId}
- Folio: {Folio}
- Fecha Inicio: {FechaInicio}

❌ Error de Validación:
{MensajeError}

🔍 Acción Requerida:
Por favor, corrija los datos de origen y reintente manualmente desde el dashboard de monitoreo.

📊 Monitoreo: {UrlDashboard}/registro/{ProcesoControlId}
```

### Template 2: Error Persistente
```
Asunto: [CRÍTICO] Error Persistente - {TipoProcesoNombre} - {Folio}

Hola Equipo,

Se ha producido un error persistente en la sincronización después de múltiples reintentos:

📋 Detalles del Registro:
- Tipo Proceso: {TipoProcesoNombre}
- Process ID: {ProcessId}
- ID Registro: {RecordId}
- Folio: {Folio}
- Reintentos: {Reintentos}/5
- Último Intento: {FechaUltimoReintento}

❌ Error Persistente:
{MensajeError}

🔄 Estado Actual: FalloPersistente

🔍 Investigación Requerida:
1. Verificar integridad de datos en sistema origen
2. Revisar restricciones en sistema destino
3. Considerar corrección manual si es necesario

📊 Monitoreo: {UrlDashboard}/registro/{ProcessId}
🔄 Reintentar Manual: {UrlApi}/api/monitoreo/registro/{ProcessId}/reintentar
```

### Template 3: Exceso de Reintentos
```
Asunto: [ALERTA] Exceso de Reintentos - {TipoProcesoNombre} - {Folio}

Hola Equipo,

Se ha excedido el límite de reintentos automáticos para un registro:

📋 Detalles del Registro:
- Tipo Proceso: {TipoProcesoNombre}
- Process ID: {ProcessId}
- ID Registro: {RecordId}
- Folio: {Folio}
- Reintentos: {Reintentos} (límite: 5)
- Primer Intento: {FechaInicio}
- Último Intento: {FechaUltimoReintento}

⚠️ Posibles Causas:
- Problema persistente en conectividad
- Datos corruptos o inconsistentes
- Recursos del sistema saturados

🔍 Acción Recomendada:
Investigar la causa raíz antes de reintentar manualmente.

📊 Análisis: {UrlDashboard}/registros?recordId={RecordId}
🔄 Reintento Manual: {UrlApi}/api/monitoreo/registro/{ProcesoControlId}/reintentar
```

---

## 🎯 Dashboard de Monitoreo Integrado

### Sección de Reintentos
```javascript
// Componente React/Vue para mostrar estado de reintentos
const ReintentosDashboard = {
  computed: {
    registrosConErrores() {
      return this.registros.filter(r => r.CategoriaEstado === 'ConErrores');
    },
    puedenReintentar() {
      return this.registrosConErrores.filter(r => r.Reintentos < 5);
    },
    excedieronReintentos() {
      return this.registrosConErrores.filter(r => r.Reintentos >= 5);
    }
  },
  methods: {
    async reintentarRegistro(idRegistro) {
      const response = await fetch(`/api/monitoreo/registro/${idRegistro}/reintentar`, {
        method: 'POST'
      });
      return response.json();
    },
    async reintentarLote(idsRegistros) {
      const response = await fetch('/api/monitoreo/reintentar-lote', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ idsRegistros })
      });
      return response.json();
    }
  }
};
```

### Indicadores Clave
- **Registros para Reintentar**: Errores transient con reintentos < 5
- **Excedieron Reintentos**: Errores con 5+ reintentos
- **Fallos Persistentes**: Errores permanentes (requieren acción manual)
- **Tasa de Éxito Global**: Completados / Total
- **Tiempo Promedio de Reintento**: Duración promedio de procesos que necesitaron reintentos

---

## 🔍 Troubleshooting Guide

### Casos Comunes y Soluciones

1. **Timeout en Extract**
   - **Causa**: Vista muy grande o red lenta
   - **Solución**: Optimizar vista, agregar índices, aumentar timeout
   - **Reintento**: Automático (Transient)

2. **FK Violation en Load**
   - **Causa**: Datos de referencia faltantes
   - **Solución**: Corregir datos de origen, revisar secuencia de carga
   - **Reintento**: Manual (Persistent)

3. **Deadlock en Transform**
   - **Causa**: Concurrencia alta en misma tabla
   - **Solución**: Reducir concurrencia, optimizar transacciones
   - **Reintento**: Automático (Transient)

4. **Validation Exception**
   - **Causa**: Datos inválidos o inconsistentes
   - **Solución**: Corregir datos origen, revisar reglas de validación
   - **Reintento**: Manual (Permanent)

Este diseño completo proporciona un sistema robusto de manejo de errores y reintentos que asegura la resiliencia del proceso ETL mientras mantiene visibilidad completa del estado de sincronización.