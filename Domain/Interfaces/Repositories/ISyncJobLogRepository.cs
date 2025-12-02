using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz para el repositorio de logs de trabajos de sincronización
/// </summary>
public interface ISyncJobLogRepository
{
    /// <summary>
    /// Inserta un nuevo registro de log
    /// </summary>
    Task<SyncJobLogDto> InsertarAsync(SyncJobLogDto dto);

    /// <summary>
    /// Obtiene un log por ID
    /// </summary>
    Task<SyncJobLogDto?> ObtenerPorIdAsync(Guid idSyncJobLog);

    /// <summary>
    /// Obtiene todos los logs de un registro específico
    /// </summary>
    Task<IEnumerable<SyncJobLogDto>> ObtenerPorRegistroAsync(
        string nombreEntidad,
        Guid identificadorRegistro);

    /// <summary>
    /// Elimina logs antiguos (para limpieza)
    /// </summary>
    Task<int> EliminarAntiguosAsync(int diasRetencion);

    /// <summary>
    /// Obtiene el último log de un registro específico
    /// </summary>
    Task<SyncJobLogDto?> ObtenerUltimoLogAsync(
        string nombreEntidad,
        Guid identificadorRegistro);
}
