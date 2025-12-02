namespace SincronizadorPqfLegacy.Domain.Enums
{
    /// <summary>
    /// Define los tipos de procesos ETL disponibles en el sistema.
    /// </summary>
    public enum TipoProcesoEtl
    {
        /// <summary>
        /// Sincronización de cotizaciones desde el sistema origen al sistema legacy.
        /// </summary>
        Cotizacion = 1,

        /// <summary>
        /// Sincronización de partidas de cotización desde el sistema origen al sistema legacy.
        /// </summary>
        Partida = 2
    }
}
