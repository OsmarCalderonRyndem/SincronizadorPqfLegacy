# 🔄 Sincronizador de Cotizaciones — SincronizadorPqfLegacy

Este microservicio sincroniza cotizaciones y sus partidas (líneas de detalle) desde el sistema moderno **ProquifaDotNet** hacia el sistema legacy **PConnect**. Implementa el patrón ETL (Extract-Transform-Load) para migrar datos críticos de negocio manteniendo integridad referencial y trazabilidad completa.

### 🧱 Patrones de diseño implementados
- **Clean Architecture**: Separación lógica por capas (Domain, Application, Infrastructure, Presentation/API).
- **Repository Pattern**: Para persistencia desacoplada.
- **UnitOfWork**: Coordinación transaccional entre múltiples repositorios.


### ⚙️ Características técnicas
- Manejo de errores con `ProblemDetails` (RFC 7807) vía middleware personalizado.
- Logging estructurado con Serilog y propagación de `traceId`.
- Validación desacoplada de DTOs con FluentValidation.


### 🎯 Funcionalidades principales
- **Sincronización individual**: Procesar una cotización específica por su ID
- **Sincronización en lote**: Procesar múltiples cotizaciones pendientes de sincronización
- **Servicio de fondo**: Sincronización automática a intervalos configurables
- **Control y trazabilidad**: Tabla de control que registra el estado de cada sincronización
- **Sincronización de partidas**: Migrar líneas de detalle asociadas a cada cotización

### 🛠️ Tecnologías
- 🟦 .NET 10.0
- ✅ FluentValidation 12.0.0
- 🔄 AutoMapper 15.0.1
- 📊 Serilog 4.3.0
- 🗄️ Entity Framework Core 8.0.8
- 📚 Swagger / OpenAPI


### 🧠 Decisiones técnicas
- **Patrón ETL**: Separación clara de Extract (lectura de origen), Transform (mapeo de datos) y Load (escritura en destino)
- **Múltiples contextos de base de datos**: Conexiones independientes a ProquifaDotNet, PConnect y bases de datos de soporte
- El flujo referencial sigue el principio de Clean Architecture: las capas externas dependen de las internas, nunca al revés.
- Se utiliza `traceId` para correlación entre logs, errores y métricas.
- Se implementa `UnitOfWork` para garantizar consistencia transaccional entre múltiples repositorios.
- Se usa `ProblemDetails` (RFC 7807) para estructurar errores de forma legible y estandarizada.
- Se aplica AutoMapper para desacoplar la lógica de transformación entre dominio e infraestructura.
- **Resiliencia en lotes**: El servicio de sincronización múltiple continúa procesando incluso si un registro falla.


### 📡 Endpoints API

#### POST `/api/sincronizarcotizacion/sincronizarCotizacion`
Sincroniza una cotización específica.
```
POST /api/sincronizarcotizacion/sincronizarCotizacion?idCotizacion={guid}
```
**Respuesta exitosa**: Cotización y partidas sincronizadas exitosamente

#### POST `/api/sincronizarcotizacion/sincronizar-pendientes`
Sincroniza todas las cotizaciones pendientes en la tabla de control.
```
POST /api/sincronizarcotizacion/sincronizar-pendientes
```
**Respuesta**: Reporte con cantidad de sincronizaciones exitosas y fallidas

### ⚙️ Configuración

En `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DocumentBuilder": "...",
    "ProquifaDotNet": "...",
    "PConnect": "...",
    "PConnectProquifaDotNet": "..."
  },
  "SincronizacionAutomatica": {
    "Habilitado": false,
    "IntervaloMinutos": 60
  }
}
```

**Sincronización automática**: Deshabilitada por defecto. Actívala estableciendo `Habilitado: true` e `IntervaloMinutos` al intervalo deseado.

### 🏗️ Arquitectura

La solución está organizada en 4 proyectos según Clean Architecture:

```
SincronizadorPqfLegacy.Domain
  └─ DTOs, interfaces de repositorios y modelos de dominio

SincronizadorPqfLegacy.Application
  └─ Servicios (SincronizarCotizacionService, SincronizarPartidasService, SincronizacionMultipleService)
  └─ Validators, Factories, Mapeos de dominio

SincronizadorPqfLegacy.Infrastructure
  └─ DbContexts (4 conexiones de base de datos)
  └─ Implementaciones de repositorios
  └─ Mapeos de AutoMapper (Profiles)
  └─ Modelos de persistencia

SincronizadorPqfLegacy.API
  └─ Controllers, Middleware, Servicios de fondo
  └─ Configuración de inyección de dependencias
```

### 🗄️ Bases de datos

El servicio interactúa con 4 bases de datos:

| BD | Propósito | Acceso |
|----|-----------|--------|
| **ProquifaDotNet** | Sistema origen (datos a sincronizar) | Lectura (vista ETL) |
| **PConnect** | Sistema destino legacy | Escritura/Lectura |
| **DocumentBuilder** | Control interno del microservicio | Escritura/Lectura |
| **PConnectProquifaDotNet** | Base de soporte | Lectura |

### 📝 Flujo de sincronización

```
1. EXTRACT: Lectura desde vCotizacionesTransformadasETL (ProquifaDotNet)
2. REGISTER: Se registra en tabla de control que comienza sincronización
3. TRANSFORM: Se mapea con AutoMapper a formato legacy
4. LOAD: Se inserta/actualiza en tabla Cotiza (PConnect)
5. PARTIDAS: Se sincronizan las líneas de detalle
6. UPDATE CONTROL: Se actualiza tabla de control con estado final
```

### 🧩 Actualizar modelos de base de datos mediante Scaffold

Para sincronizar los modelos de Entity Framework con cambios en las bases de datos:

- Actualizar el modelo en la base de datos correspondiente
- Validar cadenas de conexión en `appsettings.json`
- Ejecutar el archivo `scaffold.cmd` desde PowerShell en el directorio del proyecto
- Verificar cambios en los archivos `*Context.cs` del proyecto Infrastructure
- Eliminar la línea de cadena de conexión que se auto-genera en los Context (si es necesario)
