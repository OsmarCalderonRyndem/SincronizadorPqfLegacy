namespace SincronizadorPqfLegacy.Domain.DTOs;

public class PartidaCotizacionOrigenDto
{
    public int? CotizacionLegacy { get; set; }
    public Guid IdCotCotizacion { get; set; }
    public string? Clave { get; set; }
    public int? Partida { get; set; }
    public int Cant { get; set; }
    public string? Codigo { get; set; }
    public decimal PrecioLista { get; set; }
    public decimal Precio { get; set; }
    public string? Concepto { get; set; }
    public string? Estado { get; set; }
    public decimal? IVA { get; set; }
    public decimal Costo { get; set; }
    public string? Fabrica { get; set; }
    public string? Clasif { get; set; }
    public string? Destino { get; set; }
    public string? HEnvio { get; set; }
    public string? MEnvio { get; set; }
    public int Folio { get; set; }
    public string? ObservaE { get; set; }
    public DateTime? HCancelacion { get; set; }
    public DateTime FGeneracion { get; set; }
    public int? IndicePrecio { get; set; }
    public string? Presentacion { get; set; }
    public string? Unidades { get; set; }
    public bool? FS { get; set; }
    public int PrecioI { get; set; }
    public bool? Recotizar { get; set; }
    public int FK01PCotizaOrigen { get; set; }
    public int? FK03_idProducto { get; set; }
    public int FK04_Fabricante { get; set; }
    public string? Nota { get; set; }
    public string? TEngrega { get; set; }
    public string? EstadoCotizacion { get; set; }
    public string? Notas { get; set; }
    public int? idPCotiza { get; set; }
    public string? UnidadLegacy { get; set; }
    public string? PresentacionLegacy { get; set; }
}