using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.PConnectProquifaDotNet.Entities;

public partial class Cotizacione
{
    public int? CotizacionLegacy { get; set; }

    public Guid? CotizacionPQF { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Folio { get; set; } = null!;

    public bool Insertado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaRegistro { get; set; }

    public bool Actualizado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaUltimaActualizacion { get; set; }

    public bool? RegistroCompleto { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaUltimaActualizacionLegacy { get; set; }

    [Key]
    public Guid IdMapeoProquifaLegacy { get; set; }
}
