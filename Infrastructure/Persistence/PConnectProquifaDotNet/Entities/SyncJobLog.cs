using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.PConnectProquifaDotNet.Entities;

[Table("SyncJobLog")]
[Index("NombreEntidad", "IdentificadorRegistro", Name = "IX_SyncJobLog_Entidad_Registro")]
[Index("Estado", Name = "IX_SyncJobLog_Estado")]
[Index("FechaProcesamiento", Name = "IX_SyncJobLog_FechaProcesamiento")]
public partial class SyncJobLog
{
    [Key]
    public Guid IdSyncJobLog { get; set; }

    [StringLength(100)]
    public string NombreEntidad { get; set; } = null!;

    [StringLength(255)]
    public string IdentificadorRegistro { get; set; } = null!;

    [StringLength(50)]
    public string Estado { get; set; } = null!;

    public string? MensajeError { get; set; }

    public DateTime FechaProcesamiento { get; set; }

    public DateTime? FechaRegistro { get; set; }
}
