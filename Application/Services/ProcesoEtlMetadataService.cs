using SincronizadorPqfLegacy.Domain.Enums;
using SincronizadorPqfLegacy.Domain.Models;

namespace SincronizadorPqfLegacy.Application.Services
{
    /// <summary>
    /// Servicio que proporciona metadatos descriptivos sobre los procesos ETL disponibles.
    /// </summary>
    public class ProcesoEtlMetadataService
    {
        private static readonly Dictionary<TipoProcesoEtl, ProcesoEtlMetadata> _metadatos = new()
        {
            {
                TipoProcesoEtl.Cotizacion,
                new ProcesoEtlMetadata
                {
                    Clave = (int)TipoProcesoEtl.Cotizacion,
                    Nombre = "Cotizacion",
                    Descripcion = "Sincroniza una cotización desde el sistema origen (ProquifaDotNet) al sistema legacy (PConnect)",
                    RequiereParametrosAdicionales = false,
                    EjemploParametros = null,
                    EjemploLlamada = "POST /api/etl/sincronizar?tipoProceso=Cotizacion&recordId={guid-cotizacion}"
                }
            }
        };

        /// <summary>
        /// Obtiene los metadatos de un proceso ETL específico.
        /// </summary>
        /// <param name="tipoProceso">Tipo de proceso ETL.</param>
        /// <returns>Metadatos del proceso.</returns>
        /// <exception cref="KeyNotFoundException">Si el proceso no existe.</exception>
        public ProcesoEtlMetadata ObtenerMetadata(TipoProcesoEtl tipoProceso)
        {
            if (!_metadatos.TryGetValue(tipoProceso, out var metadata))
            {
                throw new KeyNotFoundException($"No se encontraron metadatos para el proceso ETL: {tipoProceso}");
            }

            return metadata;
        }

        /// <summary>
        /// Obtiene el catálogo completo de procesos ETL disponibles.
        /// </summary>
        /// <returns>Lista de metadatos de todos los procesos ETL.</returns>
        public IEnumerable<ProcesoEtlMetadata> ObtenerCatalogo()
        {
            return _metadatos.Values.OrderBy(m => m.Clave);
        }

        /// <summary>
        /// Obtiene el catálogo en formato de diccionario (Clave -> Metadata).
        /// </summary>
        /// <returns>Diccionario con la clave numérica como key y los metadatos como value.</returns>
        public Dictionary<int, ProcesoEtlMetadata> ObtenerCatalogoDiccionario()
        {
            return _metadatos.ToDictionary(
                kvp => (int)kvp.Key,
                kvp => kvp.Value
            );
        }

        /// <summary>
        /// Verifica si un proceso ETL requiere parámetros adicionales.
        /// </summary>
        /// <param name="tipoProceso">Tipo de proceso ETL.</param>
        /// <returns>True si requiere parámetros adicionales, false en caso contrario.</returns>
        public bool RequiereParametrosAdicionales(TipoProcesoEtl tipoProceso)
        {
            return _metadatos.TryGetValue(tipoProceso, out var metadata) && metadata.RequiereParametrosAdicionales;
        }
    }
}
