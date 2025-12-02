namespace SincronizadorPqfLegacy.Domain.Models
{
    public enum ErrorCategory
    {
        /// <summary>
        /// Errores de infraestructura, red, timeouts, bloqueos de BD.
        /// ACCIÓN: Reintentar automáticamente.
        /// </summary>
        Transient,

        /// <summary>
        /// Errores de integridad de datos, validación, formato, FKs, reglas de negocio.
        /// ACCIÓN: Detener proceso para este registro y marcar para revisión manual.
        /// </summary>
        Permanent
    }
}
