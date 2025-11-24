using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ProquifaDotNet.Entities;

[Keyless]
public partial class vCotizacionesTransformadasETL
{
    public Guid? IdCotCotizacion { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? Clave { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Cliente { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Contacto { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Vendedor { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Moneda { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? Parciales { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? CPago { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Zona { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FEnvio { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? IMoneda { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Cotizo { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Factura { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? HEntrada { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? MEntrada { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? MSalida { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaClasif { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCierre { get; set; }

    public bool? InfoFacturacion { get; set; }

    public bool? Abierto { get; set; }

    public bool? FS { get; set; }

    public bool? GravaIVA { get; set; }

    public bool? Generada { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Tipo { get; set; }

    public bool? DeSistema { get; set; }

    [Unicode(false)]
    public string? Nombre { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? HSalida { get; set; }

    public int? idContacto { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? CotizacionLegacy { get; set; }
}
