namespace SincronizadorPqfLegacy.Domain.DTOs;

/// <summary>
/// DTO de cotización desde el sistema origen (ProquifaDotNet)
/// Representa los datos transformados listos para sincronizar
/// </summary>
public class CotizacionOrigenDto
{

    /// <summary>
    /// ID único de la cotización en ProquifaDotNet
    /// </summary>
    public Guid IdCotCotizacion { get; set; }

    /// <summary>
    /// Folio completo de la cotización
    /// </summary>
    public string Folio { get; set; } = null!;

    /// <summary>
    /// Clave truncada a 11 caracteres (para legacy)
    /// </summary>
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Fecha de la cotización
    /// </summary>
    public DateTime? Fecha { get; set; }

    /// <summary>
    /// Fecha de envío al cliente
    /// </summary>
    public DateTime? FEnvio { get; set; }

    /// <summary>
    /// Fecha de cierre de la cotización
    /// </summary>
    public DateTime? FechaCierre { get; set; }

    /// <summary>
    /// Fecha de clasificación
    /// </summary>
    public DateTime? FechaClasif { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string? Cliente { get; set; }

    /// <summary>
    /// Nombre del contacto
    /// </summary>
    public string? Contacto { get; set; }

    /// <summary>
    /// ID del contacto
    /// </summary>
    public int? idContacto { get; set; }

    /// <summary>
    /// Nombre del vendedor
    /// </summary>
    public string? Vendedor { get; set; }

    /// <summary>
    /// Moneda (MXN, USD, EUR)
    /// </summary>
    public string? Moneda { get; set; }

    /// <summary>
    /// Importe de la moneda
    /// </summary>
    public string? IMoneda { get; set; }

    /// <summary>
    /// Zona geográfica
    /// </summary>
    public string? Zona { get; set; }

    /// <summary>
    /// Estado de la cotización (NUEVA, ENVIADA, CERRADA, etc.)
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// Vigencia de la cotización
    /// </summary>
    public string? Vigencia { get; set; }

    /// <summary>
    /// Condiciones de pago
    /// </summary>
    public string? CPago { get; set; }

    /// <summary>
    /// Permite entregas parciales (SI/NO)
    /// </summary>
    public string? Parciales { get; set; }

    /// <summary>
    /// Número de factura asociada
    /// </summary>
    public string? Factura { get; set; }

    /// <summary>
    /// Quién realizó la cotización
    /// </summary>
    public string? Cotizo { get; set; }

    /// <summary>
    /// Observaciones generales
    /// </summary>
    public string? Observa { get; set; }

    /// <summary>
    /// Observaciones de confirmación
    /// </summary>
    public string? ObservaC { get; set; }

    /// <summary>
    /// Quién confirmó
    /// </summary>
    public string? Confirmo { get; set; }

    /// <summary>
    /// Lugar de entrega
    /// </summary>
    public string? Lugar { get; set; }

    /// <summary>
    /// Desde cuándo fue cancelada
    /// </summary>
    public string? CanceladaDesde { get; set; }

    /// <summary>
    /// Hora de entrada (formato: HH:mm)
    /// </summary>
    public string? HEntrada { get; set; }

    /// <summary>
    /// Minutos de entrada
    /// </summary>
    public string? MEntrada { get; set; }

    /// <summary>
    /// Hora de salida (formato: HH:mm)
    /// </summary>
    public string? HSalida { get; set; }

    /// <summary>
    /// Minutos de salida
    /// </summary>
    public string? MSalida { get; set; }

    /// <summary>
    /// Cotización abierta/activa
    /// </summary>
    public bool? Abierto { get; set; }

    /// <summary>
    /// Información de facturación agregada
    /// </summary>
    public bool? InfoFacturacion { get; set; }

    /// <summary>
    /// Grava IVA
    /// </summary>
    public bool? GravaIVA { get; set; }

    /// <summary>
    /// Cotización generada
    /// </summary>
    public bool? Generada { get; set; }

    /// <summary>
    /// Proviene del sistema
    /// </summary>
    public bool? DeSistema { get; set; }

    /// <summary>
    /// FS (flag específico)
    /// </summary>
    public bool? FS { get; set; }

    /// <summary>
    /// Tipo de cotización
    /// </summary>
    public string? Tipo { get; set; }

    /// <summary>
    /// Nombre adicional
    /// </summary>
    public string? Nombre { get; set; }

    /// <summary>
    /// Orden asociada
    /// </summary>
    public int? Orden { get; set; }
}