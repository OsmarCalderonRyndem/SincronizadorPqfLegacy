using AutoMapper;
using Infrastructure.Persistence.PConnectProquifaDotNet.Entities;
using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Infrastructure.Mappers;

/// <summary>
/// Perfil de AutoMapper para SyncJobLog
/// </summary>
public class SyncJobLogMappingProfile : Profile
{
    public SyncJobLogMappingProfile()
    {
        // Entity -> DTO
        CreateMap<SyncJobLog, SyncJobLogDto>();

        // DTO -> Entity
        CreateMap<SyncJobLogDto, SyncJobLog>()
            .ForMember(dest => dest.IdSyncJobLog, opt => opt.Condition(src => src.IdSyncJobLog != Guid.Empty));
    }
}
