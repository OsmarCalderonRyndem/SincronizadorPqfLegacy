using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.PConnect.Entities;

[Index("Clave", Name = "CLIENTECONTACTOMONEDA")]
[Index("FEnvio", Name = "COTIZAENVIO")]
[Index("Fecha", Name = "FNVIOS")]
[Index("Fecha", Name = "FechaCotizacion")]
[Index("Clave", Name = "indexPrueba13")]
public partial class Cotiza
{
    [StringLength(11)]
    [Unicode(false)]
    public string? Clave { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Fecha { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Cliente { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Contacto { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Vendedor { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Vigencia { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Moneda { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? Parciales { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? CPago { get; set; }

    [StringLength(400)]
    [Unicode(false)]
    public string? Lugar { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Zona { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FEnvio { get; set; }

    [Column(TypeName = "text")]
    public string? Observa { get; set; }

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

    [StringLength(5)]
    [Unicode(false)]
    public string? HSalida { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? MSalida { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Confirmo { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? ObservaC { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaClasif { get; set; }

    public int? idContacto { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? CanceladaDesde { get; set; }

    public bool? InfoFacturacion { get; set; }

    [Key]
    public int PK_Folio { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCierre { get; set; }

    public bool? Abierto { get; set; }

    public int? FK01_idCliente { get; set; }

    public bool? FS { get; set; }

    public bool? GravaIVA { get; set; }

    public int? FK02_DoctosR { get; set; }

    public bool? Generada { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Tipo { get; set; }

    public int? Orden { get; set; }

    public bool? DeSistema { get; set; }

    [Unicode(false)]
    public string? Nombre { get; set; }

    public int? FK03_idVisita { get; set; }
}
