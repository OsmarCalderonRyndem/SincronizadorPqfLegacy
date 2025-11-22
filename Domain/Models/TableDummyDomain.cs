namespace SincronizadorPqfLegacy.Domain.Models
{
    /// <summary>
    /// Represents a domain entity with an identifier, name, and description.
    /// </summary>
    /// <remarks>This class is typically used to model a table entity in a database or similar data storage
    /// system. It provides properties for uniquely identifying the entity, as well as storing its name and
    /// description.</remarks>
    public class TableDummyDomain
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
