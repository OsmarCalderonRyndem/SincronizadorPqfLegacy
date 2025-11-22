using AutoMapper;
using SincronizadorPqfLegacy.Infrastructure.Models;
using SincronizadorPqfLegacy.Domain.Models;

namespace SincronizadorPqfLegacy.Infrastructure.Mappers
{
    /// <summary>
    /// Provides mapping configurations between domain entities and data transfer objects (DTOs).
    /// </summary>
    /// <remarks>This class inherits from the <see cref="Profile"/> class provided by AutoMapper and is used
    /// to define object-object mapping rules. Specifically, it maps between <see cref="TableDummy"/> and  <see
    /// cref="TableDummyDomain"/>.</remarks>
    public class DomainMappingProfile : Profile
    {
        public DomainMappingProfile()
        {
            // CreateMap<Source, Destination>();
            CreateMap<TableDummy, TableDummyDomain>();
        }
    }
}
