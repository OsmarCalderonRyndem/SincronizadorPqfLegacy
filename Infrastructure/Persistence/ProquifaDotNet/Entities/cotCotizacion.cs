using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ProquifaDotNet.Entities;

[Table("cotCotizacion")]
[Index("FechaRegistro", Name = "IX_CotCotizacion_Fecha")]
[Index("FechaRegistro", Name = "IX_Cotizacion_Fecha")]
[Index("IdDatosFacturacionCliente", "IdAjOfEstrategiaCotizacion", "IdArchivoPDF", "IdCliente", "IdContactoCliente", "IdCorreoRecibidoCliente", "IdCorreoRecibidoClienteReferencia", "IdDireccion", "IdEmpresa", "IdFlete", "IdPPPedidoIntramitable", "IdUsuarioTramita", Name = "IX_cotCotizacion")]
[Index("IdCatCondicionesDePago", "IdCatCondicionesDePagoDeOrigen", "IdCatEstadoCotizacion", "IdCatMoneda", "IdCatTipoCotizacion", "IdcatEstadoCotizacionVD", "IdCatZona", Name = "IX_cotCotizacion_Catalogos")]
[Index("IdCotCotizacionOriginal", Name = "IX_cotCotizacion_Original")]
[Index("IdCotCotizacion", "Folio", "FolioPublicaciones", Name = "NonClusteredIndex-cotCotizacion")]
public partial class cotCotizacion
{
    [Key]
    public Guid IdCotCotizacion { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Folio { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? FolioPublicaciones { get; set; }

    public Guid IdCliente { get; set; }

    public Guid IdContactoCliente { get; set; }

    public Guid? IdDatosFacturacionCliente { get; set; }

    public Guid IdCatCondicionesDePagoDeOrigen { get; set; }

    public int DiasDePagoDeOrigen { get; set; }

    public int DiasDePagoAdicionalesDeOrigen { get; set; }

    public Guid IdCatCondicionesDePago { get; set; }

    public int DiasDePagoAdicionales { get; set; }

    public Guid IdUsuarioTramita { get; set; }

    public Guid? IdEmpresa { get; set; }

    public Guid IdCatEstadoCotizacion { get; set; }

    public Guid? IdCatTipoCotizacion { get; set; }

    public Guid? IdCatZona { get; set; }

    public Guid? IdFlete { get; set; }

    public bool? Enviado { get; set; }

    public bool Ajustada { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaCotizacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaCaducidad { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaEnvio { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaRegistro { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaUltimaActualizacion { get; set; }

    public Guid? IdSolicitudAutorizacionCambio { get; set; }

    public bool Activo { get; set; }

    public Guid IdCatMoneda { get; set; }

    public bool AgregarDatosFacturacion { get; set; }

    [StringLength(200)]
    public string? ComentarioFlete { get; set; }

    public bool? FleteDesglosado { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal? montoFlete { get; set; }

    public int? Consecutivo { get; set; }

    public bool? Caducada { get; set; }

    public Guid? IdAjOfEstrategiaCotizacion { get; set; }

    public Guid? IdArchivoPDF { get; set; }

    public Guid? IdCorreoRecibidoCliente { get; set; }

    public Guid? IdCorreoRecibidoClienteReferencia { get; set; }

    public Guid? IdEmpresaPublicaciones { get; set; }

    public Guid? IdCotCotizacionOriginal { get; set; }

    public bool? EntregaUnica { get; set; }

    public Guid? IdDireccion { get; set; }

    public Guid? IdPPPedidoIntramitable { get; set; }

    public Guid? IdEmpleadoRepresentanteLegal { get; set; }

    [Required]
    public bool? CotizacionDeInvestigacion { get; set; }

    [Required]
    public bool? EnviadaConInvestigacion { get; set; }

    [Required]
    public bool? SeGuardanPartidasInvestigacion { get; set; }

    public bool OrigenVentaDigital { get; set; }

    public Guid? IdcatEstadoCotizacionVD { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? QuienFacturaAlias { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? MonedaCotizacion { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? MonedaCotizacionClave { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? CondicionesDePago { get; set; }

    [Required]
    public bool? TipoCambioEsDOF { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal TipoCambioMonto { get; set; }

    public string? CodigoDeFormatoServicios { get; set; }

    [ForeignKey("IdCotCotizacionOriginal")]
    [InverseProperty("InverseIdCotCotizacionOriginalNavigation")]
    public virtual cotCotizacion? IdCotCotizacionOriginalNavigation { get; set; }

    [InverseProperty("IdCotCotizacionOriginalNavigation")]
    public virtual ICollection<cotCotizacion> InverseIdCotCotizacionOriginalNavigation { get; set; } = new List<cotCotizacion>();
}
