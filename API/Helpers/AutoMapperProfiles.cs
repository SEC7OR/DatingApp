using API.DTOs;
using API.Extensions;
using API.Models;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDTO>()
        .ForMember(dto=>dto.Age, u=>u.MapFrom(u=>u.DateOfBirth.CalculateAge()))
        .ForMember(dto=>dto.PhotoUrl,u=>u.MapFrom(u => u.Photos.FirstOrDefault(p=>p.IsMain)!.Url));
        CreateMap<Photo, PhotoDTO>();
        CreateMap<MemberUpdateDTO, AppUser>();
        CreateMap<RegisterDTO,AppUser>();
        CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s));
    }
}
