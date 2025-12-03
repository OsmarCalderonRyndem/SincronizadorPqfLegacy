namespace SincronizadorPqfLegacy.Domain.Enums
{
    /// <summary>
    /// Define los tipos de procesos ETL disponibles en el sistema.
    /// </summary>
    public enum TipoProcesoEtl
    {
        /// <summary>
        /// Sincronización de cotizaciones desde el sistema origen al sistema PQDF 2.
        /// </summary>
        Cotizacion = 1,

        /// <summary>
        /// Sincronización de pedidos confirmados desde el sistema origen al sistema PQDF 2.
        /// </summary>
        Pedido = 2
    }
}
