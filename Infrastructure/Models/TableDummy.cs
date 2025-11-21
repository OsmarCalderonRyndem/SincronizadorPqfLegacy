using System.ComponentModel.DataAnnotations;

namespace Microservicio.Infrastructure.Models
{
    public class TableDummy
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(100)]
        public required string Name { get; set; }
        [StringLength(250)]
        public required string Description { get; set; }
    }
}
