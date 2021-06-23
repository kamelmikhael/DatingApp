using System.Linq;
using AutoMapper;
using DatingApp.API.DTOs;
using DatingApp.API.Entities;
using DatingApp.API.Extensions;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Photo, PhotoDto>();
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, 
                    opt => 
                        opt.MapFrom(
                            src => src.Photos.FirstOrDefault(p => p.IsMain).Url
                        )
                )
                .ForMember(dest => dest.Age, 
                    opt => 
                        opt.MapFrom(
                            src => src.DateOfBirth.CalculateAge()
                        )
                );
            CreateMap<MemberUpdateDto, AppUser>();
        }
    }
}