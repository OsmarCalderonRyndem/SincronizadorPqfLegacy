
namespace SincronizadorPqfLegacy.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object (DTO) for a table entry with a unique identifier.
    /// </summary>
    /// <remarks>This class is typically used to encapsulate data for transferring between application layers
    /// or services. The <see cref="IdRegister"/> property uniquely identifies the table entry.</remarks>
    public class TableDummyDto
    {
        public Guid IdRegister { get; set; }
    }
}
