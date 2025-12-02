using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SincronizadorPqfLegacy.Domain.Models;

[Table("SyncJobLog")]
public partial class SyncJobLog
{
    [Key]
    public Guid IdSyncJobLog { get; set; }

    [StringLength(100)]
    public string NombreEntidad { get; set; } = null!;

    public Guid IdentificadorRegistro { get; set; }

    [StringLength(50)]
    public string Estado { get; set; } = null!;

    public string? MensajeError { get; set; }

    public DateTime FechaProcesamiento { get; set; }

    public DateTime? FechaRegistro { get; set; }
}
