using AutoMapper;
using Infrastructure.Persistence.PConnect.Entities;
using SincronizadorPqfLegacy.Domain.DTOs;

namespace SincronizadorPqfLegacy.Infrastructure.Mappers
{
    public class CotizaMappingProfile : Profile
    {
        public CotizaMappingProfile()
        {
            CreateMap<CotizacionOrigenDto, CotizacionLegacyDto>()
            // =============================================
            // IGNORAR CAMPOS QUE NO SE DEBEN MAPEAR
            // =============================================
            .ForMember(dest => dest.PK_Folio, opt => opt.Ignore()) // IDENTITY - no mapear

            // =============================================
            // MAPEO AUTOMÁTICO (mismo nombre de propiedad)
            // =============================================
            // AutoMapper mapea automáticamente propiedades con el mismo nombre:


            // =============================================
            // CAMPOS QUE VAN NULL (no existen en origen)
            // =============================================
            .ForMember(dest => dest.ObservaC, opt => opt.MapFrom(src => (string?)null))
            .ForMember(dest => dest.Confirmo, opt => opt.MapFrom(src => (string?)null))
            .ForMember(dest => dest.CanceladaDesde, opt => opt.MapFrom(src => (string?)null))
            .ForMember(dest => dest.Lugar, opt => opt.MapFrom(src => (string?)null))
            .ForMember(dest => dest.Orden, opt => opt.MapFrom(src => (int?)null))

            // Foreign Keys
            .ForMember(dest => dest.FK01_idCliente, opt => opt.MapFrom(src => (int?)null))
            .ForMember(dest => dest.FK02_DoctosR, opt => opt.MapFrom(src => (int?)null))
            .ForMember(dest => dest.FK03_idVisita, opt => opt.MapFrom(src => (int?)null))

            // =============================================
            // LÓGICA POST-MAPEO (Reglas de negocio)
            // =============================================
            .AfterMap((src, dest, context) =>
            {
                // Regla 1: Vigencia por default
                if (string.IsNullOrEmpty(dest.Vigencia))
                {
                    dest.Vigencia = "30 días";
                }

                // Regla 2: Cliente por default
                if (string.IsNullOrWhiteSpace(dest.Cliente))
                {
                    dest.Cliente = "CLIENTE NO ESPECIFICADO";
                }

                // Regla 3: Moneda por default
                if (string.IsNullOrWhiteSpace(dest.Moneda))
                {
                    dest.Moneda = "MXN";
                }

                // Regla 4: Estado por default
                if (string.IsNullOrWhiteSpace(dest.Estado))
                {
                    dest.Estado = "Finalizada";
                }
            });
        }
    }
}
