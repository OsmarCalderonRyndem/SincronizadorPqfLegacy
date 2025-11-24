using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ProquifaDotNet.Entities;

[Keyless]
public partial class vCotCotizacion
{
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

    public bool CotizacionDeInvestigacion { get; set; }

    public bool EnviadaConInvestigacion { get; set; }

    public bool SeGuardanPartidasInvestigacion { get; set; }

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

    public bool TipoCambioEsDOF { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal TipoCambioMonto { get; set; }

    public Guid IdRegion { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Prefijo { get; set; }

    public string? CodigoDeFormatoServicios { get; set; }

    [StringLength(800)]
    [Unicode(false)]
    public string? UsuarioTramita { get; set; }

    public bool? TieneEstrategia { get; set; }

    [StringLength(200)]
    public string? Estrategia { get; set; }

    public bool? Publicada { get; set; }

    [StringLength(300)]
    [Unicode(false)]
    public string? Nombre { get; set; }

    public Guid? IdCatNivelIngreso { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? NivelIngreso { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? EstadoCotizacion { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? TipoCotizacion { get; set; }

    public Guid? IdContacto { get; set; }

    public Guid? IdDireccionFacturacion { get; set; }

    public int? NumeroPartidas { get; set; }

    public int? TotalProductos { get; set; }

    public int? Piezas { get; set; }

    public int? TotalArchivos { get; set; }

    public int? TotalControlados { get; set; }

    public int? TotalNoControlados { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal? subtotalPartidas { get; set; }

    [Column(TypeName = "decimal(38, 6)")]
    public decimal? subtotalFlete { get; set; }

    [Column(TypeName = "decimal(38, 6)")]
    public decimal? IVAFlete { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal? subtotalIVA { get; set; }

    [Column(TypeName = "decimal(38, 6)")]
    public decimal? TotalCotizado { get; set; }

    [Column(TypeName = "decimal(38, 6)")]
    public decimal? TotalCotizadoUSD { get; set; }

    [Column(TypeName = "decimal(38, 20)")]
    public decimal? factorDeConversionActual { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal? TotalUSDPartidas { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaRecepcion { get; set; }

    public bool? Recibido { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaLectura { get; set; }

    public bool? Leido { get; set; }

    [StringLength(12)]
    [Unicode(false)]
    public string EstadoCorreoCotizacion { get; set; } = null!;

    public bool? ajustePrecio { get; set; }

    public bool? ajusteTiempoEntrega { get; set; }

    public bool? ajusteCondicionesPago { get; set; }

    public int? ProductoDisponible { get; set; }

    public int? Sugerencias { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal? SubtotalMailBot { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal? IvaMailBot { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal? TotalMailBot { get; set; }

    [StringLength(4)]
    [Unicode(false)]
    public string Categoria { get; set; } = null!;

    public bool? InvestigacionesFinalizadas { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Region { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string ClaveRegion { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string ClaveISORegion { get; set; } = null!;
}
