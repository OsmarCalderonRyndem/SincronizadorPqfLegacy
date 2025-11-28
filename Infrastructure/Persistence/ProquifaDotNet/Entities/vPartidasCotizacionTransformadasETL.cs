using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ProquifaDotNet.Entities;

[Keyless]
public partial class vPartidasCotizacionTransformadasETL
{
    public int? CotizacionLegacy { get; set; }

    public Guid IdCotCotizacion { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Clave { get; set; }

    public int? Partida { get; set; }

    public int Cant { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Codigo { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal PrecioLista { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal Precio { get; set; }

    [StringLength(700)]
    [Unicode(false)]
    public string? Concepto { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Estado { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal? IVA { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal Costo { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Fabrica { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? Clasif { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Destino { get; set; }

    [StringLength(16)]
    [Unicode(false)]
    public string? HEnvio { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? MEnvio { get; set; }

    public int Folio { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? ObservaE { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? HCancelacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FGeneracion { get; set; }

    public int? IndicePrecio { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Presentacion { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Unidades { get; set; }

    public bool? FS { get; set; }

    public int PrecioI { get; set; }

    public bool? Recotizar { get; set; }

    public int FK01PCotizaOrigen { get; set; }

    public int? FK03_idProducto { get; set; }

    public int FK04_Fabricante { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Nota { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? TEngrega { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? EstadoCotizacion { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Notas { get; set; }

    public int? idPCotiza { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? UnidadLegacy { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? PresentacionLegacy { get; set; }
}
