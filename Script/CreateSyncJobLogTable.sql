-- =============================================
-- Script: Crear tabla SyncJobLog
-- Descripción: Tabla para auditoría de trabajos de sincronización ETL
-- Base de Datos: PConnectProquifaDotNet
-- =============================================

USE [PConnectProquifaDotNet]
GO

-- Verificar si la tabla ya existe y eliminarla para recrearla con el nuevo esquema
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SyncJobLog]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[SyncJobLog]
    PRINT 'Tabla anterior SyncJobLog eliminada.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SyncJobLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SyncJobLog](
        [IdSyncJobLog] [uniqueidentifier] NOT NULL DEFAULT NEWID(),
        [NombreEntidad] [nvarchar](100) NOT NULL,
        [IdentificadorRegistro] [nvarchar](255) NOT NULL,
        [Estado] [nvarchar](50) NOT NULL, -- 'Pendiente', 'Sincronizado', 'FalloPersistente'
        [MensajeError] [nvarchar](max) NULL,
        [FechaProcesamiento] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [FechaRegistro] [datetime2](7) NULL, -- Fecha del registro original
        CONSTRAINT [PK_SyncJobLog] PRIMARY KEY CLUSTERED ([IdSyncJobLog] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    PRINT 'Tabla SyncJobLog creada exitosamente.'
END
ELSE
BEGIN
    PRINT 'La tabla SyncJobLog ya existe.'
END
GO

-- Crear índice para consultas por estado
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncJobLog_Estado' AND object_id = OBJECT_ID('SyncJobLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SyncJobLog_Estado]
    ON [dbo].[SyncJobLog] ([Estado])
    INCLUDE ([NombreEntidad], [IdentificadorRegistro], [FechaProcesamiento])
    PRINT 'Índice IX_SyncJobLog_Estado creado exitosamente.'
END
GO

-- Crear índice para consultas por fecha (útil para limpieza de logs antiguos)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncJobLog_FechaProcesamiento' AND object_id = OBJECT_ID('SyncJobLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SyncJobLog_FechaProcesamiento]
    ON [dbo].[SyncJobLog] ([FechaProcesamiento])
    PRINT 'Índice IX_SyncJobLog_FechaProcesamiento creado exitosamente.'
END
GO

-- Crear índice compuesto para búsquedas por entidad y registro
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SyncJobLog_Entidad_Registro' AND object_id = OBJECT_ID('SyncJobLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SyncJobLog_Entidad_Registro]
    ON [dbo].[SyncJobLog] ([NombreEntidad], [IdentificadorRegistro])
    INCLUDE ([Estado], [FechaProcesamiento])
    PRINT 'Índice IX_SyncJobLog_Entidad_Registro creado exitosamente.'
END
GO

PRINT 'Script completado exitosamente.'
