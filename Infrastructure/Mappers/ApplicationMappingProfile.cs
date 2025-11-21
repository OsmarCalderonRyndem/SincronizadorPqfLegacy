using AutoMapper;
using Microservicio.Domain.Models;
using Microservicio.Infrastructure.Models;

namespace Microservicio.Infrastructure.Mappers
{
    /// <summary>
    /// Provides mapping configurations between domain models and data transfer objects (DTOs) for the application.
    /// </summary>
    /// <remarks>This class inherits from <see cref="Profile"/> and is used to define mapping rules for
    /// AutoMapper. It ensures that objects of type <see cref="TableDummyDomain"/> are mapped to <see
    /// cref="TableDummy"/>.</remarks>
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            CreateMap<TableDummyDomain, TableDummy>();
        }
    }
}
