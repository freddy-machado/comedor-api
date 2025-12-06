using AutoMapper;
using Comedor.Core.Dtos;
using Comedor.Core.Dtos.Auth;
using Comedor.Core.Entities;

namespace Comedor.Infrastructure.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Auth Mappings
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<ApplicationUser, UserListDto>(); 
        CreateMap<RegisterDto, ApplicationUser>(); 
        CreateMap<ApplicationUser, UpdateUserDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber))
            .ForMember(d => d.NormalizedUserName, o => o.MapFrom(s => s.NormalizedUserName));
        // Comensal Mappings
        CreateMap<Comensal, ComensalDto>().ReverseMap();
        CreateMap<ComensalCreateDto, Comensal>();
        CreateMap<Comensal, ComensalCreateDto>();
        
        // Area and Cargo Mappings
        CreateMap<AreaView, AreaDto>();
        CreateMap<CargoView, CargoDto>();
        
        // Despacho Mappings
        CreateMap<Despacho, DespachoDto>()
            .ForMember(dest => dest.NombreColaborador, opt => opt.MapFrom(src => src.Comensal != null ? src.Comensal.NombreColaborador : string.Empty))
            .ForMember(dest => dest.TurnoTiempo, opt => opt.MapFrom(src => src.Turno != null ? $"{src.Turno.Desde} - {src.Turno.Hasta}" : string.Empty))
            .ForMember(dest => dest.TiempoComida, opt => opt.MapFrom(src => src.Turno != null ? src.Turno.TiempoComida : string.Empty));
            
        CreateMap<CreateDespachoDto, Despacho>(); 
        CreateMap<UpdateDespachoDto, Despacho>(); 
        
        // Turno Mappings
        CreateMap<Turno, TurnoDto>(); // New

        CreateMap<ApplicationUser, UserDto>();
        CreateMap<ApplicationUser, UserListDto>();
        CreateMap<ApplicationUser, UpdateUserDto>();
    }
}
