namespace SincronizadorPqfLegacy.Domain.DTOs;

/// <summary>
/// DTO de cotización para el sistema legacy (PConnect)
/// Representa una cotización lista para insertar/actualizar en PConnect
/// </summary>
public class CotizacionLegacyDto
{
    public int PK_Folio { get; set; }
    public string? Clave { get; set; }
    public DateTime? Fecha { get; set; }
    public DateTime? FEnvio { get; set; }
    public DateTime? FechaClasif { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string? Cliente { get; set; }
    public string? Contacto { get; set; }
    public int? idContacto { get; set; }
    public string? Vendedor { get; set; }
    public string? Moneda { get; set; }
    public string? Parciales { get; set; }
    public string? CPago { get; set; }
    public string? Zona { get; set; }
    public string? Estado { get; set; }
    public string? IMoneda { get; set; }
    public string? Cotizo { get; set; }
    public string? Factura { get; set; }
    public string? HEntrada { get; set; }
    public string? MEntrada { get; set; }
    public string? MSalida { get; set; }
    public string? HSalida { get; set; }
    public bool? InfoFacturacion { get; set; }
    public bool? Abierto { get; set; }
    public bool? FS { get; set; }
    public bool? GravaIVA { get; set; }
    public bool? Generada { get; set; }
    public bool? DeSistema { get; set; }
    public string? Tipo { get; set; }
    public string? Nombre { get; set; }
    public string? Vigencia { get; set; }
    public string? Observa { get; set; }
    public string? ObservaC { get; set; }
    public string? Confirmo { get; set; }
    public string? CanceladaDesde { get; set; }
    public string? Lugar { get; set; }
    public int? Orden { get; set; }
    public int? FK01_idCliente { get; set; }
    public int? FK02_DoctosR { get; set; }
    public int? FK03_idVisita { get; set; }
}