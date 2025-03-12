using AutoMapper;
using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;

namespace RealtimeMeetingAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Source => Des
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.UserName));

            CreateMap<Room, RoomDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.AppUser.FullName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.AppUser.UserName));
        }
    }
}
