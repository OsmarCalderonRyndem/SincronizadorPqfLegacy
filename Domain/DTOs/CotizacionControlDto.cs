namespace SincronizadorPqfLegacy.Domain.DTOs;

/// <summary>
/// DTO para la tabla de control de sincronización
/// Registra el estado de sincronización de cada cotización
/// </summary>
public class CotizacionControlDto
{
    /// <summary>
    /// ID único del registro de control
    /// </summary>
    public Guid? IdMapeoProquifaLegacy { get; set; }

    /// <summary>
    /// ID de la cotización en ProquifaDotNet
    /// </summary>
    public Guid CotizacionPQF { get; set; }

    /// <summary>
    /// PK_Folio de la cotización insertada en Legacy
    /// </summary>
    public int? CotizacionLegacy { get; set; }

    /// <summary>
    /// Folio de la cotización
    /// </summary>
    public string Folio { get; set; } = null!;

    /// <summary>
    /// PK_Folio (referencia al registro legacy)
    /// </summary>
    public int? PK_Folio { get; set; }

    /// <summary>
    /// Indica si fue insertado en la tabla de control
    /// </summary>
    public bool Insertado { get; set; }

    /// <summary>
    /// Indica si fue actualizado
    /// </summary>
    public bool Actualizado { get; set; }

    /// <summary>
    /// Indica si el registro está completo
    /// </summary>
    public bool RegistroCompleto { get; set; }

    /// <summary>
    /// Fecha de registro inicial
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Última actualización en la tabla de control
    /// </summary>
    public DateTime FechaUltimaActualizacion { get; set; }

    /// <summary>
    /// Última actualización en Legacy
    /// </summary>
    public DateTime? FechaUltimaActualizacionLegacy { get; set; }
}