namespace SincronizadorPqfLegacy.Domain.Models
{
    /// <summary>
    /// Metadatos descriptivos de un proceso ETL.
    /// </summary>
    public class ProcesoEtlMetadata
    {
        /// <summary>
        /// Clave numérica del proceso.
        /// </summary>
        public int Clave { get; set; }

        /// <summary>
        /// Nombre del proceso (ej. "Cotizacion", "Partida").
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del proceso.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el proceso requiere parámetros adicionales.
        /// </summary>
        public bool RequiereParametrosAdicionales { get; set; }

        /// <summary>
        /// Ejemplo de parámetros adicionales en formato JSON (si aplica).
        /// </summary>
        public string? EjemploParametros { get; set; }

        /// <summary>
        /// Ejemplo de llamada al endpoint.
        /// </summary>
        public string EjemploLlamada { get; set; } = string.Empty;
    }
}
