# 📄 Microservicio de Generación de Reportes PDF — DocumentBuilder

Este microservicio permite generar reportes PDF a partir de datos estructurados, aplicando plantillas HTML y encabezados personalizados. Está diseñado con arquitectura limpia, trazabilidad completa y validaciones desacopladas para entornos productivos.

### 🧱 Patrones de diseño implementados
- **Clean Architecture**: Separación lógica por capas (Domain, Application, Infrastructure, Presentation/API).
- **Repository Pattern**: Para persistencia desacoplada.
- **UnitOfWork**: Coordinación transaccional entre múltiples repositorios.


### ⚙️ Características técnicas
- Manejo de errores con `ProblemDetails` (RFC 7807) vía middleware personalizado.
- Logging estructurado con Serilog y propagación de `traceId`.
- Validación desacoplada de DTOs con FluentValidation.


### 🛠️ Tecnologías
- 🟦 .NET 8
- ✅ FluentValidation
- 🔄 AutoMapper
- 📊 Serilog
- 🗄️ Entity Framework Core
- 📚 Swagger / OpenAPI


### 🧠 Decisiones técnicas
- El flujo referencial sigue el principio de Clean Architecture: las capas externas dependen de las internas, nunca al revés.
- Se utiliza `traceId` para correlación entre logs, errores y métricas.
- Se implementa `UnitOfWork` para garantizar consistencia transaccional entre múltiples repositorios.
- Se usa `ProblemDetails` (RFC 7807) para estructurar errores de forma legible y estandarizada.
- Se aplica AutoMapper para desacoplar la lógica de persistencia entre Dominio e Infrastructure.


### 📘 Documentación
- [📐 Arquitectura técnica](https://docs.google.com/document/d/18LwJ80sOFCafvSzJIKVJ-GSRiIFg4qq5Vku48nvQW_M/edit?usp=drive_link)
- [📦 Repositorio en GitHub](https://github.com/ryndem/DocumentBuilder.git)

### 🧩 Actualizar modelo de base de datos mediante Scaffold
Para sincronizar los modelos de Entity Framework con los cambios realizados en la base de datos, se utiliza el comando Scaffold-DbContext ejecutandolo desde PowerShell.
- Actualizar el modelo de base de datos con los cambios requeridos.
- Validar que la cadena de conexión del proyecto 'DocumentBuilder' esté apuntando al la Base de Datos correcta.
- Ejecutar el archivo 'scaffold.cmd' que se encuentra en el directorio del proyecto
- Se verifican los cambios en el archivo 'DocumentBuilderContext.cs' del Proyecto 'DocumentBuilder.Infrastructure'
- Se elimina la linea de codigo que se genera en 'DocumentBuilderContext' de la cadena de conexion.
