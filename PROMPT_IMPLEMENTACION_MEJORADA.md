# Prompt para Implementación Mejorada de Manejo de Errores y Flujos de Reintento

## CONTEXTO
Necesito implementar un sistema robusto de manejo de errores y flujos de reintento para el sistema ETL. Ya tenemos la arquitectura definida y documentación completa. Ahora necesito la implementación completa siguiendo los estándares actuales de .NET 10 y las mejoras específicas que diseñamos.

## COMPONENTES A IMPLEMENTAR

### 1. SERVICIO DE CLASIFICACIÓN DE ERRORES MEJORADO
- Actualizar `ExceptionClassifier` para soportar nuevos tipos de errores
- Implementar clasificación jerárquica de errores
- Agregar soporte para errores específicos de negocio
- Incluir métricas de clasificación
- Implementar logging estructurado de errores

### 2. SERVICIO DE REINTENTOS MEJORADO
- Crear `IRetryService` con políticas de reintento configurables
- Implementar backoff exponencial con jitter
- Soportar reintentos con circuit breaker
- Incluir límites de reintentos por tipo de error
- Implementar cola de prioridad para reintentos

### 3. SERVICIO DE COMPENSACIÓN Y ROLLBACK
- Crear `ICompensationService` para manejo de compensaciones
- Implementar `IRollbackService` específico para subproceso LOAD
- Crear puntos de compensación por subproceso
- Implementar rollback transaccional completo
- Soportar rollback parcial cuando sea posible

### 4. SERVICIO DE NOTIFICACIONES MEJORADO
- Actualizar `NotificationService` para múltiples canales
- Implementar plantillas de email dinámicas
- Soportar notificaciones por tipo de error
- Incluir contexto detallado en notificaciones
- Implementar cola de notificaciones asíncrona

### 5. SERVICIO DE CONTROL DE PROCESOS
- Actualizar `EtlProcesoControlService` con nuevos estados
- Implementar transiciones de estado atómicas
- Soportar subprocesos con puntos de compensación
- Incluir métricas de duración por subproceso
- Implementar bloqueos optimistas para evitar concurrencia

### 6. SERVICIO DE VALIDACIONES MEJORADO
- Crear `IValidationService` genérico
- Implementar validaciones por tipo de proceso
- Soportar validaciones en cascada
- Incluir validaciones de integridad de datos
- Implementar validaciones de negocio específicas

### 7. SERVICIO DE MONITOREO DE ERRORES
- Crear `ErrorMonitoringService` para seguimiento de errores
- Implementar dashboards en tiempo real de errores
- Soportar alertas por umbrales de error
- Incluir análisis de patrones de errores
- Implementar métricas de recuperación

### 8. SERVICIO DE RECUPERACIÓN AUTOMÁTICA
- Crear `IRecoveryService` para recuperación automática
- Implementar estrategias de recuperación por tipo de error
- Soportar recuperación con datos de snapshots
- Incluir validaciones post-recuperación
- Implementar logging de eventos de recuperación

## FLUJOS DE REINTENO MEJORADOS

### 1. FLUJO DE REINTENTO AUTOMÁTICO
```
Error Detectado → ExceptionClassifier → ErrorCategory
    ↓
ErrorCategory.Transient → RetryService → Backoff Exponencial
    ↓
Hangfire.Schedule → Esperar Delay → Reintentar
    ↓
Éxito → Actualizar Estado → Continuar
Fallo → Incrementar Reintentos → Verificar Límite
    ↓
Límite Excedido → Marcar como Permanent → Notificar
```

### 2. FLUJO DE REINTENTO MANUAL
```
Usuario solicita → MonitoreoService → Identificar Error
    ↓
Error Transient → Reintentar Inmediato → Sin Contador
    ↓
Error Permanent → Analizar Causa → Corregir Manualmente
    ↓
Corregido → Reintentar Manual → Verificar Éxito
```

### 3. FLUJO DE REINTENTO PROGRAMADO
```
Job Recurrente → Consultar Errores → Filtrar Candidatos
    ↓
Error Transient + Reintentos < 5 → Encolar Reintento
    ↓
Error Permanent → Analizar → Notificar si es crítico
    ↓
Reintentos Excedidos → Escalar a Equipo Soporte
```

### 4. FLUJO DE COMPENSACIÓN Y ROLLBACK
```
Error en Subproceso → Verificar Tipo de Subproceso
    ↓
Subproceso LOAD → ICompensationService → Ejecutar Rollback
    ↓
Subproceso Extract/Transform → Manejar Error sin Rollback
    ↓
Rollback Completo → Actualizar Estado → Notificar
```

## TECNOLOGÍA ESPECÍFICA

### .NET 10 Features
- `Polly` para circuit breaker y retry con backoff
- `System.Threading.Channels` para comunicación asíncrona
- `System.Threading.Tasks.Task` con `CancellationToken`
- `System.Text.Json` para serialización de datos de error
- `Microsoft.Extensions.Logging` para logging estructurado

### Patrones de Diseño
- **Circuit Breaker Pattern** con Polly
- **Retry Pattern** con backoff exponencial y jitter
- **Saga Pattern** para compensaciones complejas
- **Observer Pattern** para notificaciones
- **Strategy Pattern** para diferentes tipos de error
- **Command Pattern** para acciones de recuperación

### Bibliotecas Recomendadas
- **Polly 7+**: Circuit breaker, retry, timeout
- **Serilog 3+**: Logging estructurado
- **Hangfire 1.8+**: Jobs recurrentes y reintentos
- **AutoMapper 12+**: Mapeos de objetos
- **FluentValidation 11+**: Validaciones
- **MassTransit**: Publicación de eventos

## IMPLEMENTACIÓN DETALLADA

### 1. ExceptionClassifier Mejorado
```csharp
public class ExceptionClassifier : IExceptionClassifier
{
    private readonly ILogger<ExceptionClassifier> _logger;
    private readonly IOptions<ErrorClassificationOptions> _options;
    private readonly IMetricsService _metrics;
    
    public ExceptionClassifier(
        ILogger<ExceptionClassifier> logger,
        IOptions<ErrorClassificationOptions> options,
        IMetricsService metrics)
    {
        _logger = logger;
        _options = options;
        _metrics = metrics;
    }

    public ErrorCategory ClassifyException(Exception exception)
    {
        try
        {
            // Clasificación jerárquica
            var category = ClassifyByType(exception);
            
            // Clasificación por mensaje
            if (category == ErrorCategory.Unknown)
            {
                category = ClassifyByMessage(exception);
            }
            
            // Clasificación por contexto
            if (category == ErrorCategory.Unknown)
            {
                category = ClassifyByContext(exception);
            }
            
            // Registrar métricas
            _metrics.IncrementClassification(category);
            _logger.LogDebug("Error clasificado como {Category} para {ExceptionType}: {Message}", 
                category, exception.GetType().Name, exception.Message);
            
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al clasificar excepción");
            return ErrorCategory.Unknown;
        }
    }
    
    private ErrorCategory ClassifyByType(Exception exception)
    {
        // Errores Transientes (reintentar automáticamente)
        if (exception is TimeoutException ||
            exception is HttpRequestException ||
            exception is SocketException ||
            exception is SqlException sql && IsTransientSql(sql))
            return ErrorCategory.Transient;
        
        // Errores de Negocio (requieren intervención manual)
        if (exception is ValidationException ||
            exception is DomainException ||
            exception is InvalidOperationException ||
            exception is ArgumentException)
            return ErrorCategory.Business;
        
        // Errores de Sistema (requieren intervención técnica)
        if (exception is InvalidOperationException ||
            exception is NotSupportedException ||
            exception is NotImplementedException ||
            exception is OutOfMemoryException)
            return ErrorCategory.System;
        
        // Errores de Datos (requieren corrección de datos)
        if (exception is DbUpdateException ||
            exception is DbUpdateConcurrencyException ||
            exception is SqlException sql && IsDataSql(sql))
            return ErrorCategory.Data;
        
        // Errores de Seguridad (requieren atención inmediata)
        if (exception is UnauthorizedAccessException ||
            exception is SecurityException ||
            exception is CryptographicException)
            return ErrorCategory.Security;
        
        return ErrorCategory.Unknown;
    }
    
    private bool IsTransientSql(SqlException sql)
    {
        // Códigos de error SQL transitorios
        int[] transientCodes = { -2, 1205, 53, 64, 1205, 11001 };
        return transientCodes.Contains(sql.Number);
    }
    
    private bool IsDataSql(SqlException sql)
    {
        // Códigos de error de datos
        int[] dataCodes = { 547, 2601, 2627, 547, 2601, 2627 };
        return dataCodes.Contains(sql.Number);
    }
    
    private ErrorCategory ClassifyByMessage(Exception exception)
    {
        var message = exception.Message?.ToLower() ?? string.Empty;
        
        // Palabras clave que indican errores transitorios
        var transientKeywords = new[]
        {
            "timeout", "deadline", "network", "connection", "temporary", "retry"
        };
        
        if (transientKeywords.Any(keyword => message.Contains(keyword)))
            return ErrorCategory.Transient;
        
        // Palabras clave que indican errores de negocio
        var businessKeywords = new[]
        {
            "validation", "invalid", "required", "duplicate", "conflict", "constraint"
        };
        
        if (businessKeywords.Any(keyword => message.Contains(keyword)))
            return ErrorCategory.Business;
        
        return ErrorCategory.Unknown;
    }
    
    private ErrorCategory ClassifyByContext(Exception exception)
    {
        // Analizar el stack trace para contexto
        var stackTrace = exception.StackTrace ?? string.Empty;
        
        // Contexto de base de datos
        if (stackTrace.Contains("System.Data.SqlClient"))
            return ErrorCategory.Data;
        
        // Contexto de red
        if (stackTrace.Contains("System.Net.Http"))
            return ErrorCategory.System;
        
        // Contexto de archivos
        if (stackTrace.Contains("System.IO"))
            return ErrorCategory.System;
        
        return ErrorCategory.Unknown;
    }
}
```

### 2. RetryService con Circuit Breaker
```csharp
public class RetryService : IRetryService
{
    private readonly ILogger<RetryService> _logger;
    private readonly IOptions<RetryOptions> _options;
    private readonly IMetricsService _metrics;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreaker;
    
    public RetryService(
        ILogger<RetryService> logger,
        IOptions<RetryOptions> options,
        IMetricsService metrics,
        IAsyncPolicy<HttpResponseMessage> circuitBreaker)
    {
        _logger = logger;
        _options = options;
        _metrics = metrics;
        _circuitBreaker = circuitBreaker;
    }
    
    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = Policy
            .Handle<Exception>(ex => IsTransient(ex))
            .WaitAndRetryAsync(
                retryCount: _options.MaxRetryAttempts,
                sleepDurationProvider: _options.SleepDurationProvider,
                onRetry: (exception, delay, attempt, context) =>
                {
                    _logger.LogWarning(
                        "Intento {Attempt} de {OperationName} después de {Delay}ms} (Error: {Error})",
                        attempt, operationName, delay.TotalMilliseconds, exception.Message);
                    
                    _metrics.IncrementRetry(operationName, exception);
                })
            )
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: _options.BreakerExceptionsAllowedBeforeBreaking,
                durationOfBreak: _options.BreakerDuration,
                onBreak: (exception, delay) =>
                {
                    _logger.LogError("Circuit breaker abierto para {OperationName} debido a: {Error}", 
                        operationName, exception.Message);
                    
                    _metrics.IncrementCircuitBreaker(operationName);
                })
            .WrapAsync();
        
        try
        {
            return await retryPolicy.ExecuteAsync(operation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error final en operación {OperationName}: {Error}", 
                operationName, ex.Message);
            throw;
        }
    }
    
    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = Policy
            .Handle<Exception>(ex => IsTransient(ex))
            .WaitAndRetryAsync(
                retryCount: _options.MaxRetryAttempts,
                sleepDurationProvider: _options.SleepDurationProvider,
                onRetry: (exception, delay, attempt, context) =>
                {
                    _logger.LogWarning(
                        "Intento {Attempt} de {OperationName} después de {Delay}ms} (Error: {Error})",
                        attempt, operationName, delay.TotalMilliseconds, exception.Message);
                    
                    _metrics.IncrementRetry(operationName, exception);
                })
            )
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: _options.BreakerExceptionsAllowedBeforeBreaking,
                durationOfBreak: _options.BreakerDuration,
                onBreak: (exception, delay) =>
                {
                    _logger.LogError("Circuit breaker abierto para {OperationName} debido a: {Error}", 
                        operationName, exception.Message);
                    
                    _metrics.IncrementCircuitBreaker(operationName);
                })
            .WrapAsync();
        
        try
        {
            return await retryPolicy.ExecuteAsync(operation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error final en operación {OperationName}: {Error}", 
                operationName, ex.Message);
            throw;
        }
    }
    
    private bool IsTransient(Exception exception)
    {
        // Implementar lógica de clasificación mejorada
        var classifier = new ExceptionClassifier(_logger, _options, _metrics);
        return classifier.ClassifyException(exception) == ErrorCategory.Transient;
    }
}
```

### 3. CompensationService para Rollback
```csharp
public class CompensationService : ICompensationService
{
    private readonly ILogger<CompensationService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Dictionary<string, List<ICompensationAction>> _compensationActions;
    
    public CompensationService(
        ILogger<CompensationService> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        
        // Registrar acciones de compensación por tipo de proceso
        _compensationActions = new Dictionary<string, List<ICompensationAction>>();
        
        // Para Cotizaciones
        _compensationActions["Cotizacion"] = new List<ICompensationAction>
        {
            new DeleteLegacyCotizacionAction(),
            new DeleteLegacyPartidasCotizacionAction(),
            new ResetControlCotizacionAction()
        };
        
        // Para Pedidos
        _compensationActions["Pedido"] = new List<ICompensationAction>
        {
            new DeleteLegacyPedidoAction(),
            // Agregar acciones específicas de pedido aquí
        };
        
        // Para Facturas
        _compensationActions["Factura"] = new List<ICompensationAction>
        {
            new DeleteLegacyFacturaAction(),
            // Agregar acciones específicas de factura aquí
        };
    }
    
    public async Task ExecuteCompensationAsync(
        string tipoProceso,
        Guid processId,
        Dictionary<string, object> context = null)
    {
        try
        {
            _logger.LogInformation("Iniciando compensación para {TipoProceso} - ProcessId: {ProcessId}", 
                tipoProceso, processId);
            
            if (!_compensationActions.ContainsKey(tipoProceso))
            {
                _logger.LogWarning("No hay acciones de compensación configuradas para {TipoProceso}", tipoProceso);
                return;
            }
            
            var actions = _compensationActions[tipoProceso];
            
            // Iniciar transacción para rollback
            using var transaction = await _unitOfWork.BeginTransactionAsync()
            {
                try
                {
                    // Ejecutar acciones de compensación en orden inverso
                    for (int i = actions.Count - 1; i >= 0; i--)
                    {
                        var action = actions[i];
                        await action.ExecuteAsync(processId, context);
                    }
                    
                    _logger.LogInformation("Compensación completada para {TipoProceso} - ProcessId: {ProcessId}", 
                        tipoProceso, processId);
                    
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en compensación para {TipoProceso} - ProcessId: {ProcessId}", 
                tipoProceso, processId);
            throw;
        }
    }
    
    public async Task<bool> CanCompensateAsync(string tipoProceso)
    {
        return _compensationActions.ContainsKey(tipoProceso);
    }
    
    public async Task<List<string>> GetCompensationActionsAsync(string tipoProceso)
    {
        if (!_compensationActions.ContainsKey(tipoProceso))
            return new List<string>();
        
        return _compensationActions[tipoProceso]
            .Select(action => action.GetType().Name)
            .ToList();
    }
}
```

### 4. NotificationService Mejorado
```csharp
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IOptions<NotificationOptions> _options;
    private readonly IEmailService _emailService;
    private readonly ISlackService _slackService;
    private readonly IQueueService _queueService;
    private readonly IMetricsService _metrics;
    
    public NotificationService(
        ILogger<NotificationService> logger,
        IOptions<NotificationOptions> options,
        IEmailService emailService,
        ISlackService slackService,
        IQueueService queueService,
        IMetricsService metrics)
    {
        _logger = logger;
        _options = options;
        _emailService = emailService;
        _slackService = slackService;
        _queueService = queueService;
        _metrics = metrics;
    }
    
    public async Task SendNotificationAsync(
        NotificationMessage notification,
        NotificationType type = NotificationType.Error,
        Dictionary<string, object> context = null)
    {
        try
        {
            _logger.LogInformation("Enviando notificación de tipo {Type}: {Subject}", 
                type, notification.Subject);
            
            // Enviar por múltiples canales en paralelo
            var tasks = new List<Task>();
            
            // Email
            if (_options.EnableEmailNotifications)
            {
                tasks.Add(_emailService.SendEmailAsync(notification));
            }
            
            // Slack
            if (_options.EnableSlackNotifications)
            {
                tasks.Add(_slackService.SendMessageAsync(notification));
            }
            
            // Cola de mensajes
            if (_options.EnableQueueNotifications)
            {
                tasks.Add(_queueService.EnqueueAsync(notification));
            }
            
            // Esperar a que se completen todas las notificaciones
            await Task.WhenAll(tasks);
            
            _metrics.IncrementNotification(type);
            _logger.LogInformation("Notificación enviada exitosamente por {Count} canales", tasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación: {Error}", ex.Message);
            throw;
        }
    }
    
    public async Task SendPermanentErrorNotificationAsync(
        string tipoProceso,
        Guid processId,
        Exception exception,
        Dictionary<string, object> context = null)
    {
        var notification = new NotificationMessage
        {
            Subject = $"[ERROR PERMANENTE] {tipoProceso} - {processId}",
            Body = BuildErrorBody(tipoProceso, processId, exception, context),
            Priority = NotificationPriority.High,
            Timestamp = DateTime.UtcNow,
            Context = context ?? new Dictionary<string, object>()
        };
        
        await SendNotificationAsync(notification, NotificationType.Error);
    }
    
    public async Task SendRetryExceededNotificationAsync(
        string tipoProceso,
        Guid processId,
        int reintentos,
        Exception lastException,
        Dictionary<string, object> context = null)
    {
        var notification = new NotificationMessage
        {
            Subject = $"[ALERTA] Exceso de Reintentos - {tipoProceso} - {processId}",
            Body = BuildRetryExceededBody(tipoProceso, processId, reintentos, lastException, context),
            Priority = NotificationPriority.High,
            Timestamp = DateTime.UtcNow,
            Context = context ?? new Dictionary<string, object>()
        };
        
        await SendNotificationAsync(notification, NotificationType.Error);
    }
    
    private string BuildErrorBody(
        string tipoProceso,
        Guid processId,
        Exception exception,
        Dictionary<string, object> context)
    {
        var body = new StringBuilder();
        body.AppendLine($"Tipo Proceso: {tipoProceso}");
        body.AppendLine($"Process ID: {processId}");
        body.AppendLine($"Tipo Error: {exception.GetType().Name}");
        body.AppendLine($"Mensaje: {exception.Message}");
        body.AppendLine($"Timestamp: {DateTime.UtcNow:O}");
        
        if (context != null && context.Any())
        {
            body.AppendLine("Contexto:");
            foreach (var kvp in context)
            {
                body.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }
        
        if (!string.IsNullOrEmpty(exception.StackTrace))
        {
            body.AppendLine("Stack Trace:");
            body.AppendLine(exception.StackTrace);
        }
        
        return body.ToString();
    }
    
    private string BuildRetryExceededBody(
        string tipoProceso,
        Guid processId,
        int reintentos,
        Exception lastException,
        Dictionary<string, object> context)
    {
        var body = new StringBuilder();
        body.AppendLine($"Tipo Proceso: {tipoProceso}");
        body.AppendLine($"Process ID: {processId}");
        body.AppendLine($"Reintentos Realizados: {reintentos}");
        body.AppendLine($"Límite de Reintentos: {_options.MaxRetryAttempts}");
        body.AppendLine($"Último Error: {lastException.Message}");
        body.AppendLine($"Timestamp: {DateTime.UtcNow:O}");
        
        if (context != null && context.Any())
        {
            body.AppendLine("Contexto:");
            foreach (var kvp in context)
            {
                body.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }
        
        return body.ToString();
    }
}
```

### 5. EtlProcesoControlService Mejorado
```csharp
public class EtlProcesoControlService : IEtlProcesoControlService
{
    private readonly ILogger<EtlProcesoControlService> _logger;
    private readonly IGenericRepository<EtlProcesoControl> _repository;
    private readonly IMetricsService _metrics;
    private readonly IOptions<ProcesoControlOptions> _options;
    
    public EtlProcesoControlService(
        ILogger<EtlProcesoControlService> logger,
        IGenericRepository<EtlProcesoControl> repository,
        IMetricsService metrics,
        IOptions<ProcesoControlOptions> options)
    {
        _logger = logger;
        _repository = repository;
        _metrics = metrics;
        _options = options;
    }
    
    public async Task<Guid> CrearProcesoAsync(
        int tipoProceso,
        Guid recordId,
        Dictionary<string, object> metadata = null)
    {
        try
        {
            var proceso = new EtlProcesoControl
            {
                Id = Guid.NewGuid(),
                TipoProceso = tipoProceso,
                RecordId = recordId,
                ProcessId = recordId, // ID unificado para consultas externas
                Estado = "EnProcesoInicio",
                FechaInicio = DateTime.UtcNow,
                Metadata = metadata ?? new Dictionary<string, object>(),
                Reintentos = 0,
                SubprocesoActual = null,
                FechaUltimoReintento = null
            };
            
            var procesoCreado = await _repository.AddAsync(proceso);
            
            _logger.LogInformation("Proceso creado: {ProcesoId} - Tipo: {TipoProceso} - RecordId: {RecordId}", 
                procesoCreado.Id, tipoProceso, recordId);
            
            _metrics.IncrementProcesoCreado(tipoProceso);
            
            return procesoCreado.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proceso: {Error}", ex.Message);
            throw;
        }
    }
    
    public async Task ActualizarEstadoAsync(
        Guid procesoId,
        string nuevoEstado,
        string? subprocesoActual = null,
        Dictionary<string, object> metadata = null)
    {
        try
        {
            var proceso = await _repository.GetByIdAsync(procesoId);
            if (proceso == null)
                throw new KeyNotFoundException($"Proceso no encontrado: {procesoId}");
            
            // Bloquear para evitar concurrencia en el mismo proceso
            var lock = new SemaphoreSlim(1, 1);
            await lock.WaitAsync();
            
            try
            {
                proceso.Estado = nuevoEstado;
                proceso.SubprocesoActual = subprocesoActual;
                proceso.FechaUltimoModificacion = DateTime.UtcNow;
                
                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    proceso.Metadata[kvp.Key] = kvp.Value;
                }
                
                await _repository.UpdateAsync(proceso);
                
                _logger.LogInformation("Estado actualizado: {ProcesoId} - Nuevo Estado: {NuevoEstado} - Subproceso: {SubprocesoActual}", 
                    procesoId, nuevoEstado, subprocesoActual);
                
                _metrics.IncrementEstadoActualizacion(nuevoEstado);
            }
            finally
            {
                lock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado: {Error}", ex.Message);
            throw;
        }
    }
    
    public async Task ActualizarReintentosAsync(
        Guid procesoId,
        int reintentos,
        DateTime fechaUltimoReintento)
    {
        try
        {
            var proceso = await _repository.GetByIdAsync(procesoId);
            if (proceso == null)
                throw new KeyNotFoundException($"Proceso no encontrado: {procesoId}");
            
            proceso.Reintentos = reintentos;
            proceso.FechaUltimoReintento = fechaUltimoReintento;
            
            await _repository.UpdateAsync(proceso);
            
            _logger.LogInformation("Reintentos actualizados: {ProcesoId} - Total: {Reintentos} - Último: {FechaUltimoReintento}", 
                procesoId, reintentos, fechaUltimoReintento);
            
            _metrics.IncrementReintentos();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar reintentos: {Error}", ex.Message);
            throw;
        }
    }
    
    public async Task CompletarProcesoAsync(
        Guid procesoId,
        Dictionary<string, object> metadata = null)
    {
        try
        {
            var proceso = await _repository.GetByIdAsync(procesoId);
            if (proceso == null)
                throw new KeyNotFoundException($"Proceso no encontrado: {procesoId}");
            
            proceso.Estado = "Completado";
            proceso.FechaFin = DateTime.UtcNow;
            proceso.DuracionMilisegundos = (int)(proceso.FechaFin - proceso.FechaInicio).TotalMilliseconds;
            
            if (metadata != null)
            {
                foreach (var kvp in metadata)
                    proceso.Metadata[kvp.Key] = kvp.Value;
            }
            
            await _repository.UpdateAsync(proceso);
            
            _logger.LogInformation("Proceso completado: {ProcesoId} - Duración: {Duracion}ms", 
                procesoId, proceso.DuracionMilisegundos);
            
            _metrics.IncrementProcesoCompletado();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar proceso: {Error}", ex.Message);
            throw;
        }
    }
    
    public async Task MarcarComoFalloPersistenteAsync(
        Guid procesoId,
        string mensajeError,
        Dictionary<string, object> context = null)
    {
        try
        {
            var proceso = await _repository.GetByIdAsync(procesoId);
            if (proceso == null)
                throw new KeyNotFoundException($"Proceso no encontrado: {procesoId}");
            
            proceso.Estado = "FalloPersistente";
            proceso.MensajeError = mensajeError;
            proceso.FechaFin = DateTime.UtcNow;
            proceso.DuracionMilisegundos = (int)(proceso.FechaFin - proceso.FechaInicio).TotalMilliseconds;
            
            if (context != null)
            {
                foreach (var kvp in context)
                    proceso.Metadata[kvp.Key] = kvp.Value;
            }
            
            await _repository.UpdateAsync(proceso);
            
            _logger.LogError("Proceso marcado como fallo persistente: {ProcesoId} - Error: {MensajeError}", 
                procesoId, mensajeError);
            
            _metrics.IncrementProcesoFalloPersistente();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar fallo persistente: {Error}", ex.Message);
            throw;
        }
    }
    
    public async Task<bool> PuedeReintentarAsync(Guid procesoId)
    {
        try
        {
            var proceso = await _repository.GetByIdAsync(procesoId);
            if (proceso == null)
                return false;
            
            // Solo se puede reintentar si no es fallo persistente
            return proceso.Estado != "FalloPersistente" && 
                   proceso.Reintentos < _options.MaxRetryAttempts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar si se puede reintentar: {Error}", ex.Message);
            return false;
        }
    }
    
    public async Task<List<EtlProcesoControl>> ObtenerProcesosParaReintentarAsync(
        int limite = 100)
    {
        try
        {
            var sql = @"
                SELECT TOP (@limite) * FROM EtlProcesoControl
                WHERE Estado IN ('EnProcesoInicio', 'EnProcesoValidacionesCorrectas', 'EnProcesoExtract', 'EnProcesoTransform', 'EnProcesoLoad')
                  AND Reintentos < @MaxReintentos
                  AND (FechaUltimoReintento IS NULL OR 
                          FechaUltimoReintento < DATEADD(HOUR, -1, GETDATE()))
                  ORDER BY FechaUltimoReintento ASC";
            
            var procesos = await _repository.GetBySqlAsync<EtlProcesoControl>(sql, 
                new { MaxRetryAttempts = _options.MaxRetryAttempts, Limite = limite });
            
            return procesos.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener procesos para reintentar: {Error}", ex.Message);
            return new List<EtlProcesoControl>();
        }
    }
}
```

## TESTING COMPLETO

### Unit Tests
```csharp
[Test]
public class ExceptionClassifierTests
{
    private readonly ExceptionClassifier _classifier;
    
    public ExceptionClassifierTests()
    {
        _classifier = new ExceptionClassifier(logger, options, metrics);
    }
    
    [Theory]
    [InlineData("timeout", ErrorCategory.Transient)]
    [InlineData("validation", ErrorCategory.Business)]
    [InlineData("sql", ErrorCategory.Unknown)]
    public void ClassifyException_ReturnsCorrectCategory(
        Exception exception, ErrorCategory expectedCategory)
    {
        // Act
        var result = _classifier.ClassifyException(exception);
        
        // Assert
        Assert.Equal(expectedCategory, result);
    }
    
    [Theory]
    [InlineData("timeout", ErrorCategory.Transient)]
    public void ClassifyTimeoutException_ReturnsTransient()
    {
        // Arrange
        var exception = new TimeoutException("Test timeout");
        
        // Act
        var result = _classifier.ClassifyException(exception);
        
        // Assert
        Assert.Equal(ErrorCategory.Transient, result);
    }
    
    [Theory]
    [InlineData("validation", ErrorCategory.Business)]
    public void ClassifyValidationException_ReturnsBusiness()
    {
        // Arrange
        var exception = new ValidationException("Test validation");
        
        // Act
        var result = _classifier.ClassifyException(exception);
        
        // Assert
        return Assert.Equal(ErrorCategory.Business, result);
    }
}
```

### Integration Tests
```csharp
[Test]
public class RetryServiceIntegrationTests : IClassFixture<RetryServiceTestsFixture>
{
    [Fact]
    public async Task RetryService_TransientError_RetriesSuccessfully()
    {
        // Arrange
        var retryCount = 0;
        var operation = new Func<Task<string>>(() =>
        {
            retryCount++;
            if (retryCount < 3)
                throw new TimeoutException("Test timeout");
            return Task.FromResult("Success");
        });
        
        // Act
        var result = await _retryService.ExecuteWithRetryAsync(
            operation, 
            "TestOperation",
            cancellationToken);
        
        // Assert
        Assert.Equal("Success", result);
        Assert.Equal(3, retryCount);
    }
    
    [Fact]
    public async Task RetryService_PermanentError_ThrowsImmediately()
    {
        // Arrange
        var operation = new Func<Task<string>>(() =>
        {
            throw new ValidationException("Test validation");
        });
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _retryService.ExecuteWithRetryAsync(operation, "TestOperation"));
    }
    
    [Fact]
    public async Task RetryService_CircuitBreaker_OpensAfterThreshold()
    {
        // Arrange
        var operation = new Func<Task<string>>(() =>
        {
            throw new HttpRequestException("Test circuit breaker");
        });
        
        // Act
        var exception = await Assert.ThrowsAsync<BrokenCircuitException>(
            () => _retryService.ExecuteWithRetryOperation(operation, "TestOperation"));
        
        // Assert
        Assert.NotNull(exception);
    }
}
```

## CONFIGURACIÓN

### appsettings.json
```json
{
  "ErrorClassification": {
    "TransientKeywords": ["timeout", "deadline", "network", "connection", "temporary", "retry"],
    "BusinessKeywords": ["validation", "invalid", "required", "duplicate", "conflict", "constraint"],
    "SystemKeywords": ["System.IO", "System.Net", "NotSupportedException", "NotImplementedException"],
    "DataKeywords": ["constraint", "duplicate", "conflict", "unique", "required"]
  },
  "Retry": {
    "MaxRetryAttempts": 5,
    "SleepDurationProvider": "ExponentialBackoff",
    "BreakerExceptionsAllowedBeforeBreaking": 3,
    "BreakerDuration": "00:05:00",
    "JitterFactor": 0.1
  },
  "Notification": {
    "EnableEmailNotifications": true,
    "EnableSlackNotifications": true,
    "EnableQueueNotifications": false,
    "EmailSettings": {
      "SmtpServer": "smtp.example.com",
      "Port": 587,
      "Username": "notifications@company.com",
      "Password": "password",
      "EnableSsl": true
    },
    "SlackSettings": {
      "WebhookUrl": "https://hooks.slack.com/services/errors",
      "Channel": "#errors"
    }
  },
  "ProcesoControl": {
    "MaxRetryAttempts": 5,
    "RetryCooldownMinutes": 60,
    "MaxConcurrentProcesses": 10
  },
  "Compensation": {
    "Enabled": true,
    "TimeoutSeconds": 30,
    "MaxActionsPerProcess": 10
  }
}
```

## ENTREGABLES ESPERADOS

### Fase 1: Infraestructura Base (1 semana)
- [ ] Actualizar base de datos con nuevas columnas
- [ ] Implementar servicios básicos (ExceptionClassifier, RetryService)
- [ ] Crear interfaces de servicios mejorados
- [ ] Configuración básica de logging y métricas
- [ ] Tests unitarios de servicios básicos

### Fase 2: Integración ETL (1 semana)
- [ ] Integrar servicios en servicios ETL existentes
- [ ] Implementar captura de snapshots en flujo normal
- [ ] Implementar manejo de errores con nuevos servicios
- [ ] Integrar notificaciones mejoradas
- [ ] Tests de integración de flujo completo

### Fase 3: APIs y Consultas (1 semana)
- [ ] Crear SnapshotController con endpoints completos
- [] Implementar endpoints de comparación de snapshots
- [] Actualizar servicios de monitoreo con nuevos filtros
- [ ] Implementar endpoints de reintento mejorados
- [ ] Tests de APIs y consultas

### Fase 4: Optimización y Producción (1 semana)
- [] Implementar circuit breaker y políticas de reintento
- [ ] Configurar monitoreo y métricas avanzadas


Este prompt proporciona una guía completa para implementar el sistema de manejo de errores y flujos de reintento mejorado, manteniendo toda la arquitectura existente y siguiendo las mejores prácticas de .NET 10.