using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.PConnect.Entities;

[Index("Clasif", "Folio", Name = "CLASI_FOLIO")]
[Index("Clave", "Folio", Name = "Clave_Folio")]
[Index("Estado", Name = "PCOTIZAS_ESTADO")]
[Index("Folio", "Estado", Name = "Producto")]
[Index("Estado", "Folio", Name = "indexPrueba1")]
[Index("Clave", Name = "indexPrueba10")]
[Index("Cant", Name = "indexPrueba11")]
[Index("FGeneracion", Name = "indexPrueba8")]
[Index("Precio", Name = "indexPrueba9")]
[Index("Folio", "FK02_Cotiza", Name = "indiceIdCotizaFolio")]
[Index("Clasif", "Folio", "Estado", Name = "pcotizas_clasif_folio_estado")]
public partial class PCotiza
{
    [Key]
    public int idPCotiza { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? Clave { get; set; }

    public int? Partida { get; set; }

    public float? Cant { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Codigo { get; set; }

    public float? Precio { get; set; }

    [Column(TypeName = "text")]
    public string? Concepto { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Estado { get; set; }

    public float? IVA { get; set; }

    public float? Costo { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Fabrica { get; set; }

    [Unicode(false)]
    public string? Nota { get; set; }

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

    public short? Folio { get; set; }

    [StringLength(400)]
    [Unicode(false)]
    public string? ObservaE { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? HCancelacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FGeneracion { get; set; }

    public short? IndicePrecio { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Presentacion { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Unidades { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? TEntrega { get; set; }

    public bool? FS { get; set; }

    public float? PrecioI { get; set; }

    [Column(TypeName = "text")]
    public string? NotasCancelacion { get; set; }

    public bool Recotizar { get; set; }

    public int? FK01_PCotizaOrigen { get; set; }

    public int? FK03_idProducto { get; set; }

    public int? FK04_Fabricante { get; set; }

    public int? FK02_Cotiza { get; set; }

    [Column(TypeName = "text")]
    public string? NotasFExpress { get; set; }

    public int? FK05_idAutorizacion { get; set; }
}
