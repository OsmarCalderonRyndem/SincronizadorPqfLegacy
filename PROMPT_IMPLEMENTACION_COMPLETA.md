# Prompt para Implementación Completa del Sistema ETL con Snapshots

## CONTEXTO
Necesito implementar un sistema ETL mejorado con captura de snapshots para .NET 10. Ya tenemos toda la arquitectura definida y documentación creada. Ahora necesito la implementación completa siguiendo los estándares actuales y buenas prácticas de .NET 10.

## COMPONENTES A IMPLEMENTAR

### 1. SERVICIO DE SNAPSHOTS (.NET 10)
- Crear `IEtlSnapshotService` y `EtlSnapshotService`
- Usar `System.Text.Json` (nativo de .NET 8/10)
- Implementar `IHostEnvironment` para configuración
- Usar `ILogger<EtlSnapshotService>` para logging estructurado
- Incluir cancelación tokens con `CancellationToken` en métodos asíncronos
- Validar argumentos con `ArgumentNullException.ThrowIfNull`

### 2. ACTUALIZACIÓN DE SERVICIOS ETL EXISTENTES
- Integrar captura de snapshots en `Sincronizar{Entidad}Service`
- Modificar constructores para inyectar `IEtlSnapshotService`
- Implementar captura en cada subproceso (Extract, Transform, Load)
- Manejar snapshots en flujos de error y reintento
- Usar `try-catch-finally` para garantizar captura incluso en errores

### 3. ACTUALIZACIÓN DE BASE DE DATOS
- Ejecutar script SQL para agregar columnas a `SyncJobLog`
- Crear índices para consultas de snapshots
- Considerar particionamiento si la tabla crece significativamente
- Implementar migraciones con EF Core Migrations
- Configurar constraints CHECK para integridad de datos JSON

### 4. CONTROLADOR DE SNAPSHOTS (.NET 10)
- Implementar `SnapshotController` con atributos `[ApiController]` y `[Route]`
- Usar `[FromQuery]` y `[FromBody]` para parámetros
- Implementar soporte para `ProblemDetails` (RFC 7807)
- Incluir CORS si es necesario para APIs web
- Implementar rate limiting si se requiere

### 5. DTOs Y MAPEOS (.NET 10)
- Usar `record` types donde sea apropiado para inmutabilidad
- Implementar validaciones con Data Annotations o FluentValidation
- Crear perfiles de AutoMapper para snapshots
- Considerar `IOptions<T>` para configuración tipada

### 6. VISTAS SQL ACTUALIZADAS
- Modificar `vSyncJobLogUnificado` para incluir campos nuevos
- Agregar `vSyncMetricasUnificadas` con métricas de snapshots
- Considerar vistas indexadas para rendimiento
- Implementar filtros parametrizados para evitar SQL injection

### 7. CONFIGURACIÓN (.NET 10)
- Usar `appsettings.json` y `appsettings.{Environment}.json`
- Implementar `IOptionsSnapshot` para configuración tipada
- Configurar Serilog para logging estructurado
- Considerar Azure Key Vault para secrets en producción
- Implementar health checks para servicios de snapshot

### 8. TESTING (.NET 10)
- Crear tests unitarios con xUnit o NUnit
- Implementar tests de integración con Testcontainers
- Tests de rendimiento para serialización JSON
- Mocking con Moq o NSubstitute
- Cobertura de código con Coverlet o similar

### 9. DOCUMENTACIÓN SWAGGER (.NET 10)
- Configurar Swashbuckle/OpenAPI para documentación automática
- Agregar ejemplos de uso para endpoints de snapshots
- Incluir esquemas JSON para DTOs de snapshots
- Documentar políticas de retención y límites

### 10. MONITOREO Y OBSERVABILIDAD
- Implementar Application Insights o similar
- Configurar contadores personalizados para snapshots
- Logging de métricas de rendimiento
- Dashboard para visualización de snapshots
- Alertas para errores en captura de snapshots

## TECNOLOGÍA ESPECÍFICA

### .NET 10 Features
- Usar `System.Text.Json` para serialización (recomendado para .NET 8+)
- `record types` para DTOs inmutables
- `nullable reference types` donde aplique
- `global using` statements para imports comunes
- `IAsyncEnumerable` para colecciones grandes
- `Task<T>` con `ConfigureAwait(false)` donde sea apropiado

### Entity Framework Core 10
- Migraciones con código-first approach
- Consultas compiladas con EF Core 10
- Soporte para columnas JSON con `ToJson()` method
- Cambios de tracking de entidades
- Optimización de consultas con `AsNoTracking()`

### ASP.NET Core 10
- Minimal APIs donde sea apropiado
- Controllers tradicionales para APIs complejas
- Middleware personalizado si se requiere
- Configuración de CORS y seguridad
- Health checks para servicios críticos

## PATRONES A SEGUIR

### Clean Architecture (.NET 10)
- Mantener separación clara de responsabilidades
- Inyección de dependencias con constructor injection
- Interfaces en capa de dominio
- Implementaciones en capa de infraestructura
- DTOs en capa de aplicación

### Repository Pattern Mejorado
- Repositorios genéricos con `IRepository<T>`
- Unidad de trabajo con `IUnitOfWork`
- Manejo de transacciones con `IDbContextTransaction`
- Soporte para consultas asíncronas

### Error Handling (.NET 10)
- `ExceptionFilterAttribute` para MVC/APIs
- `IExceptionHandler` para manejo global
- Logging estructurado con Serilog
- Custom exceptions con códigos de error específicos

### Logging (.NET 10)
- Serilog con sinks múltiples (Console, File, Seq, etc.)
- Logging estructurado con `{@PropertyName}`
- Enrichers para información de contexto
- Correlation IDs para seguimiento de peticiones

## CONSIDERACIONES DE RENDIMIENTO

### Serialización JSON
- Usar `System.Text.Json` con opciones optimizadas
- Considerar `JsonSerializerOptions.DefaultIgnoreCondition`
- Comprimir JSON si se almacenan snapshots grandes
- Cache de serializers reutilizables

### Base de Datos
- Indexación de columnas usadas en filtros
- Particionamiento por fecha para tablas grandes
- Considerar Archive Tables para datos históricos
- Mantenimiento automatizado de índices

### Almacenamiento
- Políticas de retención basadas en regulaciones
- Compresión de snapshots antiguos
- Costo de almacenamiento vs. utilidad
- Backup y recovery strategies

## SEGURIDAD

### Protección de Datos
- Sanitización de datos sensibles en snapshots
- Encriptación de datos confidenciales si es requerido
- Control de acceso basado en roles
- Audit logging de accesos a snapshots

### API Security
- Autenticación y autorización si es requerida
- Rate limiting para prevenir abuse
- Input validation exhaustiva
- HTTPS obligatorio en producción

## MIGRACIÓN DESDE .NET 8

### Cambios Requeridos
- Actualizar paquetes NuGet a .NET 10
- Actualizar sintaxis de C# a features de .NET 10
- Migración de JSON.NET a System.Text.Json si aplica
- Actualizar dependencias obsoletas

### Compatibilidad
- Asegurar backward compatibility con APIs existentes
- Estrategia de migración incremental
- Testing de compatibilidad con clientes
- Rollback plan si es necesario

Este es un roadmap completo. Por favor implementa siguiendo el orden establecido y manteniendo comunicación constante sobre el progreso. Cada fase debe estar completamente probada antes de pasar a la siguiente.