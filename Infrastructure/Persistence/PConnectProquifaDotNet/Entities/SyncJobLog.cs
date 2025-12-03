using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.PConnectProquifaDotNet.Entities;

/// <summary>
/// Entidad para registro de logs de trabajos de sincronización
/// </summary>
[Table("SyncJobLog")]
public class SyncJobLog
{
    [Key]
    public Guid IdSyncJobLog { get; set; }

    [Required]
    [StringLength(100)]
    public string NombreEntidad { get; set; } = null!;

    [Required]
    public Guid IdentificadorRegistro { get; set; }

    [Required]
    [StringLength(50)]
    public string Estado { get; set; } = null!;

    public string? MensajeError { get; set; }

    [Required]
    public DateTime FechaProcesamiento { get; set; }

    public DateTime? FechaRegistro { get; set; }
}
