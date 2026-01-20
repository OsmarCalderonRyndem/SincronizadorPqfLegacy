# Exportación de Diagramas para Draw.io (Formato C4 Mejorado)

Este archivo contiene el código Mermaid utilizando la sintaxis específica **C4** para alinearse con el estilo visual de `Proquifa_C4_Ecosistema.drawio`.

## Instrucciones de Importación
1. Abra **Draw.io**.
2. Cree un **nuevo diagrama** o abra uno existente.
3. Para cada "Hoja" o "Nivel":
   - Añada una nueva página en Draw.io.
   - Vaya al menú: **Arrange > Insert > Advanced > Mermaid...**.
   - **Copie y Pegue** el bloque de código correspondiente.
   - Haga click en **Insert**.

> **Nota:** La sintaxis C4 de Mermaid genera automáticamente las cajas azules (Sistemas), grises (Personas) y azules claros (Contenedores) que coinciden con el estándar C4.

---

## Hoja 1: Nivel 1 - Context (C1)

```mermaid
C4Context
    title C1 - System Context Diagram - ProquifaSync

    %% Personas
    Person(User1, "Usuario Manual", "Administrador del Sistema<br/>Ejecuta sincronizaciones manuales")
    Person(User2, "Usuarios del Sistema", "Generan datos de negocio (Cotizaciones, Pedidos)")

    %% Sistemas Externos
    System_Ext(API1, "APIs Externas", "Ecosistema ProquifaNet 2")
    System_Ext(OriginDB, "ProquifaDotNet DB", "SQL Server - Sistema Origen (Nuevo)")
    System_Ext(LegacyDB, "PConnect DB", "SQL Server - Sistema Legacy (Antiguo)")
    
    %% Sistema Principal
    System(SystemETL, "ProquifaSync", "Sistema ETL de Sincronización<br/>.NET 8 Ecosystem", "Sincroniza entidades entre ProquifaDotNet y PConnect")

    %% Componentes de Soporte Visibles en Contexto
    System_Ext(Scheduler, "Scheduler Service", "Hangfire", "Ejecuta jobs recurrentes")
    System_Ext(Dashboard, "Hangfire Dashboard", "Web UI", "Monitoreo de jobs")

    %% Relaciones
    Rel(User1, SystemETL, "Ejecuta manual / Monitorea")
    Rel(User1, Dashboard, "Administra jobs")
    Rel(User2, OriginDB, "Genera datos")
    Rel(API1, SystemETL, "Solicita sync")
    
    Rel(SystemETL, OriginDB, "Lee datos (Extract)", "SQL/EF Core")
    Rel(SystemETL, LegacyDB, "Escribe datos (Load)", "SQL/EF Core")
    
    Rel(Scheduler, SystemETL, "Dispara jobs")
    Rel(SystemETL, Dashboard, "Publica estado")
```

---

## Hoja 2: Nivel 2 - Container (C2)

```mermaid
C4Container
    title C2 - Container Diagram - ProquifaSync System

    Person(User, "Usuario/Cliente", "Usa el sistema vía HTTPS")
    System_Ext(ExternalAPI, "APIs Externas", "Sistemas terceros")

    Container_Boundary(c1, "ProquifaSync") {
        Container(WebAPI, "API Layer", "ASP.NET Core 8.0 Web API", "Endpoints REST, Enrutamiento, Middleware")
        Container(HangfireDashboard, "Hangfire Dashboard", "Hangfire.AspNetCore", "Visualización y gestión de jobs")
        Container(AppServices, "Application Layer", ".NET 8 Class Library", "Lógica de negocio ETL, Orquestación, Validaciones")
        Container(DomainCore, "Domain Layer", ".NET 8 Class Library", "Entidades, DTOs, Interfaces, Reglas de negocio")
        Container(InfrastructureLayer, "Infrastructure Layer", ".NET 8 Class Library", "EF Core, Repositorios, Background Jobs")
    }

    ContainerDb(OriginDB, "ProquifaDotNet DB", "SQL Server", "Origen de datos (Read Only)")
    ContainerDb(LegacyDB, "PConnect DB", "SQL Server", "Destino Legacy (Read/Write)")
    ContainerDb(ControlDB, "PConnectProquifaDotNet DB", "SQL Server", "Control de sincronización, Logs, Hangfire")

    %% Relaciones
    Rel(User, WebAPI, "Usa", "HTTPS/JSON")
    Rel(User, HangfireDashboard, "Monitorea", "HTTPS")
    Rel(ExternalAPI, WebAPI, "Usa", "HTTPS/JSON")

    Rel(WebAPI, AppServices, "Usa servicios")
    Rel(WebAPI, InfrastructureLayer, "Encola jobs")
    
    Rel(AppServices, DomainCore, "Implementa reglas")
    Rel(InfrastructureLayer, DomainCore, "Implementa interfaces")
    Rel(AppServices, InfrastructureLayer, "Usa repositorios")

    Rel(InfrastructureLayer, OriginDB, "SELECT", "EF Core")
    Rel(InfrastructureLayer, LegacyDB, "INSERT/UPDATE", "EF Core")
    Rel(InfrastructureLayer, ControlDB, "READ/WRITE", "EF Core")

    Rel(HangfireDashboard, ControlDB, "Lee estado jobs", "SQL")
```

---

## Hoja 3: Nivel 3 - Component Layer: Application (C3.1)

```mermaid
C4Component
    title C3 - Component Diagram - Application Services

    Container_Boundary(app, "Application Services Layer") {
        
        Component(SincJob, "SincronizacionJobService", "Service", "Coordinador general, Wrapper Hangfire")
        Component(SincMultiple, "SincronizacionMultipleService", "Service", "Orquestador de procesos masivos")
        
        %% Servicios Genéricos y de Soporte
        Component(ExceptionClassifier, "ExceptionClassifier", "Utils", "Clasifica errores (Transitorios vs Permanentes)")
        Component(SyncLog, "SyncLogService", "Service", "Registra logs de auditoría estandarizados")
        Component(Metadata, "ProcesoEtlMetadataService", "Service", "Catálogo de procesos ETL disponibles")

        %% Servicios de Entidad (Patrón)
        Component(EntidadService, "Sincronizar{Entidad}Service", "Specific Service", "Implementa ETL para una entidad (ej. Cotizacion)")
        Component(DetalleService, "Sincronizar{Detalle}Service", "Specific Service", "Maneja partidas/detalles de la entidad")
        
        Component(Validators, "FluentValidation", "Validator", "Valida DTOs de entrada")
    }

    Container(Domain, "Domain Layer", "Library", "Interfaces y DTOs")
    
    %% Relaciones
    Rel(SincJob, EntidadService, "Ejecuta strategy")
    Rel(SincJob, ExceptionClassifier, "Clasifica error")
    Rel(SincJob, SyncLog, "Registra resultado")
    
    Rel(SincMultiple, EntidadService, "Itera y ejecuta")
    
    Rel(EntidadService, DetalleService, "Coordina detalles")
    Rel(EntidadService, Validators, "Valida datos")
    
    Rel(EntidadService, Domain, "Usa interfaces")
```

---

## Hoja 4: Nivel 3 - Component Layer: Infrastructure (C3.2)

```mermaid
C4Component
    title C3 - Component Diagram - Infrastructure Layer

    Container_Boundary(inf, "Infrastructure Layer") {
        
        Component(GenericRepo, "GenericRepository<T>", "Base Class", "CRUD genérico reutilizable")
        Component(UOW, "UnitOfWork", "Pattern", "Coordina transacciones en múltiples contextos")

        %% Repositorios Específicos
        Component(OrigenRepo, "EntidadOrigenRepository", "Repository", "Lee de ProquifaDotNet")
        Component(LegacyRepo, "EntidadLegacyRepository", "Repository", "Escribe en PConnect")
        Component(ControlRepo, "EntidadControlRepository", "Repository", "Control de estado ETL")
        Component(SyncJobLogRepo, "SyncJobLogRepository", "Repository", "Logs de sincronización")

        %% Contextos EF
        Component(OrigenCtx, "ProquifaDotNetContext", "EF Core DbContext", "Mapeo BD Origen")
        Component(LegacyCtx, "PConnectContext", "EF Core DbContext", "Mapeo BD Legacy")
        Component(ControlCtx, "ControlContext","EF Core DbContext", "Mapeo BD Control")
        
        %% Mappers
        Component(AutoMapper, "AutoMapper Profiles", "Mapping", "Transformación DTOs")
    }

    %% Relaciones de Herencia/Uso
    Rel(OrigenRepo, GenericRepo, "Hereda")
    Rel(LegacyRepo, GenericRepo, "Hereda")
    Rel(ControlRepo, GenericRepo, "Hereda")

    Rel(OrigenRepo, OrigenCtx, "Usa")
    Rel(LegacyRepo, LegacyCtx, "Usa")
    Rel(ControlRepo, ControlCtx, "Usa")

    Rel(UOW, OrigenCtx, "Commit/Rollback")
    Rel(UOW, LegacyCtx, "Commit/Rollback")
    Rel(UOW, ControlCtx, "Commit/Rollback")

    Rel(OrigenRepo, AutoMapper, "Proyecta datos")
    Rel(LegacyRepo, AutoMapper, "Mapea inserts")
```

---

## Hoja 5: Nivel 3 - Component Layer: API (C3.3)

```mermaid
C4Component
    title C3 - Component Diagram - API Layer

    Container_Boundary(api, "API Layer") {
        Component(EtlController, "EtlController", "Controller", "Expone endpoints de sincronización")
        Component(Middleware, "Manejo de Errores & Logging", "Middleware", "Global Exception Handler, Serilog")
        Component(Filters, "Hangfire Filters", "Filter", "Captura fallos en jobs")
        Component(ServiceExt, "ServiceExtensions", "Config", "Inyección de Dependencias")
    }

    Container(AppContainer, "Application Layer", "Lib", "Servicios de negocio")
    Container(HangfireServer, "Hangfire Server", "Lib", "Procesamiento Background")

    %% Relaciones
    Rel(Middleware, EtlController, "Pasa request")
    Rel(EtlController, AppContainer, "Llama servicios")
    Rel(EtlController, HangfireServer, "Encola jobs")
    
    Rel(ServiceExt, EtlController, "Configura")
    Rel(Filters, HangfireServer, "Intercepta jobs")
```

---

## Hoja 6: Nivel 4 - Code: Domain (UML)

> **Nota:** Para diagramas de clases (Nivel 4), se mantiene la sintaxis UML estándar de Mermaid (`classDiagram`) ya que C4 no cubre este nivel de detalle.

```mermaid
classDiagram
    class IGenericRepository~T~ {
        <<interface>>
        +AddOrUpdate(T entity) Task~Guid~
        +GetById(Guid id) Task~T~
    }

    class EntidadOrigenDto {
        <<abstract>>
        +IdEntidad Guid
        +Folio string
    }

    class EntidadLegacyDto {
        <<abstract>>
        +PK_Folio int
        +Folio string
    }

    class ISincronizarEntidad {
        <<interface>>
        +SincronizarEntidad(Guid id) Task~Guid~
    }
```

---

## Hoja 7: Vista de Despliegue (Deployment)

```mermaid
graph TB
    subgraph Azure["☁️ Microsoft Azure Cloud"]
        style Azure fill:#e6f2ff,stroke:#0066cc

        subgraph AppSvc["Azure App Service"]
            style AppSvc fill:#ffffff,stroke:#0078d4
            WebApp[("🌐 ProquifaSync API<br/>(.NET 8)")]
            style WebApp fill:#0078d4,color:#fff
        end

        subgraph Data["Data Persistence"]
            style Data fill:#ffffff,stroke:#5c2d91
            SQL1[(🗄️ Origin DB)]
            SQL2[(🗄️ Legacy DB)]
            SQL3[(🗄️ Control DB)]
            style SQL1 fill:#5c2d91,color:#fff
            style SQL2 fill:#5c2d91,color:#fff
            style SQL3 fill:#5c2d91,color:#fff
        end

        subgraph Monitor["Monitoring"]
            style Monitor fill:#ffffff,stroke:#b7472a
            AppInsights[📊 App Insights]
            style AppInsights fill:#b7472a,color:#fff
        end
    end

    subgraph OnPrem["🏢 On-Premise"]
        style OnPrem fill:#f0f0f0,stroke:#666
        LegacySystems[Sistemas Legacy]
        style LegacySystems fill:#666,color:#fff
    end

    WebApp -->|TDS| SQL1
    WebApp -->|TDS| SQL2
    WebApp -->|TDS| SQL3
    WebApp -->|HTTPS| AppInsights
    SQL2 -.->|Replication| LegacySystems
```
