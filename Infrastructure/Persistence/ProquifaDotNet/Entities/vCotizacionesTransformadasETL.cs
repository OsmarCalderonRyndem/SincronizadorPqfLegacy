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
    public string? Param_0 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Param_1 { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Param_2 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Param_3 { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Param_4 { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? Param_5 { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Param_6 { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Param_7 { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Param_8 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Param_9 { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Param_10 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Param_11 { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Param_12 { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? Param_13 { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? Param_14 { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? Param_15 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Param_16 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Param_17 { get; set; }

    public bool? Param_18 { get; set; }

    public bool? Param_19 { get; set; }

    public bool? Param_20 { get; set; }

    public bool? Param_21 { get; set; }

    public bool? Param_22 { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Param_23 { get; set; }

    public bool? Param_24 { get; set; }

    [Unicode(false)]
    public string? Param_25 { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? Param_26 { get; set; }

    public int? Param_27 { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? Param_28 { get; set; }
}
