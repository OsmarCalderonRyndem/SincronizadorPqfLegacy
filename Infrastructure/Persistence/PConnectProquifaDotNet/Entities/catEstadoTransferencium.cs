using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.PConnectProquifaDotNet.Entities;

public partial class catEstadoTransferencium
{
    [Key]
    public Guid IdCatEstadoTransferencia { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string EstadoTransferencia { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Clave { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime FechaRegistro { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaUltimaActualizacion { get; set; }

    public bool Activo { get; set; }
}
