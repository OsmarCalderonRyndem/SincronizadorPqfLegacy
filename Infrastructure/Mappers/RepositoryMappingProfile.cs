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

        // Mapeos de partidas(NUEVOS)
        CreateMap<vPartidasCotizacionTransformadasETL, PartidaCotizacionOrigenDto>()
            .ReverseMap();

        CreateMap<PCotiza, PartidaCotizacionLegacyDto>()
            .ReverseMap();

        CreateMap<PartidaCotizacionOrigenDto, PartidaCotizacionLegacyDto>()
            .ForMember(dest => dest.idPCotiza, opt => opt.Ignore())
            .ForMember(dest => dest.Cant, opt => opt.MapFrom(src => (float?)src.Cant))
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => (float?)src.Precio))
            .ForMember(dest => dest.PrecioI, opt => opt.MapFrom(src => (float?)src.PrecioI))
            .ForMember(dest => dest.IVA, opt => opt.MapFrom(src => (float?)src.IVA))
            .ForMember(dest => dest.Costo, opt => opt.MapFrom(src => (float?)src.Costo))
            .ForMember(dest => dest.Folio, opt => opt.MapFrom(src => (short?)src.Folio))
            .ForMember(dest => dest.IndicePrecio, opt => opt.MapFrom(src => (short?)src.IndicePrecio))
            .ForMember(dest => dest.Recotizar, opt => opt.MapFrom(src => src.Recotizar ?? false))
            .ForMember(dest => dest.TEntrega, opt => opt.MapFrom(src => src.TEngrega))
            .ForMember(dest => dest.NotasCancelacion, opt => opt.Ignore())
            .ForMember(dest => dest.NotasFExpress, opt => opt.Ignore())
            .ForMember(dest => dest.FK05_idAutorizacion, opt => opt.Ignore())
            .ForMember(dest => dest.FK04_Fabricante, opt => opt.Ignore());
    }
}
