using AutoMapper;
using Infrastructure.Persistence.PConnect.Entities;
using Infrastructure.Persistence.PConnectProquifaDotNet.Entities;
using Infrastructure.Persistence.ProquifaDotNet.Entities;
using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Infrastructure.Mappers;

/// <summary>
/// Perfil de AutoMapper para mapear entre Entidades (EF Core) y DTOs (Domain)
/// </summary>
public class RepositoryMappingProfile : Profile
{
    public RepositoryMappingProfile()
    {
        // =====================================================
        // MAPEO: vCotizacionesTransformadasETL → CotizacionOrigenDto
        // =====================================================
        CreateMap<vCotizacionesTransformadasETL, CotizacionOrigenDto>()
            .ReverseMap(); // Permite mapeo bidireccional

        // =====================================================
        // MAPEO: Cotiza (Entity) ↔ CotizacionLegacyDto
        // =====================================================
        CreateMap<Cotiza, CotizacionLegacyDto>()
            .ReverseMap(); // DTO → Entity para inserts/updates

        // =====================================================
        // MAPEO: Cotizacione (Entity) ↔ CotizacionControlDto
        // =====================================================
        CreateMap<Cotizacione, CotizacionControlDto>()
            .ReverseMap(); // DTO → Entity para inserts/updates
    }
}