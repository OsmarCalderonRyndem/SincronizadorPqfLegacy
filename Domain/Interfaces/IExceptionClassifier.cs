using SincronizadorPqfLegacy.Domain.Models;

namespace SincronizadorPqfLegacy.Domain.Interfaces
{
    /// <summary>
    /// Clasifica excepciones en categorías para determinar su manejo (reintentos, códigos HTTP, etc.)
    /// </summary>
    public interface IExceptionClassifier
    {
        /// <summary>
        /// Clasifica una excepción como Transient (transitoria, reintentable) o Permanent (permanente, no reintentable).
        /// </summary>
        ErrorCategory Classify(Exception ex);

        /// <summary>
        /// Obtiene los detalles HTTP apropiados (código de estado y título) para una excepción.
        /// </summary>
        (int StatusCode, string Title) GetHttpDetails(Exception ex);
    }
}
