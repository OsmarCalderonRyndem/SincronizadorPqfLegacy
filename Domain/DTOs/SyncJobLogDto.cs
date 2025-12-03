namespace SincronizadorPqfLegacy.Domain.DTOs;

/// <summary>
/// DTO para registro de logs de trabajos de sincronización
/// </summary>
public class SyncJobLogDto
{
    public Guid IdSyncJobLog { get; set; }

    public string NombreEntidad { get; set; } = null!;

    public Guid IdentificadorRegistro { get; set; }

    public string Estado { get; set; } = null!;

    public string? MensajeError { get; set; }

    public DateTime FechaProcesamiento { get; set; }

    public DateTime? FechaRegistro { get; set; }
}
