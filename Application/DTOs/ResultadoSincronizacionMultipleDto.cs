namespace SincronizadorPqfLegacy.Application.DTOs
{
    /// <summary>
    /// DTO para el resultado de la sincronización masiva
    /// </summary>
    public class ResultadoSincronizacionMultipleDto
    {
        /// <summary>
        /// Total de registros pendientes encontrados
        /// </summary>
        public int TotalPendientes { get; set; }

        /// <summary>
        /// Registros sincronizados exitosamente
        /// </summary>
        public int Exitosos { get; set; }

        /// <summary>
        /// Registros que fallaron
        /// </summary>
        public int Fallidos { get; set; }

        /// <summary>
        /// Fecha de inicio del proceso
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha de fin del proceso
        /// </summary>
        public DateTime FechaFin { get; set; }

        /// <summary>
        /// Tiempo total de ejecución
        /// </summary>
        public TimeSpan TiempoTotal => FechaFin - FechaInicio;

        /// <summary>
        /// Lista de folios que fallaron durante la sincronización
        /// </summary>
        public List<string> FoliosFallidos { get; set; } = new();
    }
}