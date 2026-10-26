using AutoMapper;
using TMS.Api.Dtos;
using TMS.Data.Entity;

namespace TMS.Api.Mapping
{
    // A Profile is where we tell AutoMapper which classes can be converted into which.
    // CreateMap<Source, Destination>(): properties with the SAME NAME are copied automatically.
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // Input DTO -> Entity (Create / Update).
            // The system owns these three fields, so AutoMapper must never fill them.
            CreateMap<TravelRequestInputDto, TravelRequest>()
                .ForMember(dest => dest.TravelRequestId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore());

            // Entity -> Response DTO (every response).
            // TripDays has no matching property on the entity, so we say how to calculate it.
            CreateMap<TravelRequest, TravelRequestResponseDto>()
                .ForMember(dest => dest.TripDays,
                           opt => opt.MapFrom(src => (src.EndDate.Date - src.StartDate.Date).Days + 1));
        }
    }
}
