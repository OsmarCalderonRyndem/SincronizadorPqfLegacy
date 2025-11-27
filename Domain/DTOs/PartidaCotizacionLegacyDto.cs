namespace SincronizadorPqfLegacy.Domain.DTOs;

public class PartidaCotizacionLegacyDto
{
    public int idPCotiza { get; set; }
    public string? Clave { get; set; }
    public int? Partida { get; set; }
    public float? Cant { get; set; }
    public string? Codigo { get; set; }
    public float? Precio { get; set; }
    public string? Concepto { get; set; }
    public string? Estado { get; set; }
    public float? IVA { get; set; }
    public float? Costo { get; set; }
    public string? Fabrica { get; set; }
    public string? Nota { get; set; }
    public string? Clasif { get; set; }
    public string? Destino { get; set; }
    public string? HEnvio { get; set; }
    public string? MEnvio { get; set; }
    public short? Folio { get; set; }
    public string? ObservaE { get; set; }
    public DateTime? HCancelacion { get; set; }
    public DateTime? FGeneracion { get; set; }
    public short? IndicePrecio { get; set; }
    public string? Presentacion { get; set; }
    public string? Unidades { get; set; }
    public string? TEntrega { get; set; }
    public bool? FS { get; set; }
    public float? PrecioI { get; set; }
    public string? NotasCancelacion { get; set; }
    public bool Recotizar { get; set; }
    public int? FK01_PCotizaOrigen { get; set; }
    public int? FK03_idProducto { get; set; }
    public int? FK04_Fabricante { get; set; }
    public int? FK02_Cotiza { get; set; }
    public string? NotasFExpress { get; set; }
    public int? FK05_idAutorizacion { get; set; }
}