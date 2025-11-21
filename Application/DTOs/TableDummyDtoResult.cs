using Microservicio.Domain.Models;

namespace Microservicio.Application.DTOs
{
    /// <summary>
    /// Represents the result of a table dummy operation, containing the associated table dummy data.
    /// </summary>
    /// <remarks>This class is typically used to encapsulate the result of operations involving table dummy
    /// data.</remarks>
    public class TableDummyDtoResult
    {
        public required TableDummyDomain tableDummy { get; set; }
    }
}
