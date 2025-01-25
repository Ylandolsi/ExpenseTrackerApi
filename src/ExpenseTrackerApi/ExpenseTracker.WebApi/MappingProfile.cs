using AutoMapper;
using ExpenseTrackerApi.Dto.Expense;
using Models.Entities;

namespace ExpenseTrackerApi ; 

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map UserCreateDto to AuthUser
        CreateMap<UserCreateDto, AuthUser>()
            .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.email))
            .ForMember(dest => dest.hashedPassword, opt => opt.MapFrom(src => LoginMethods.HashPassword(src.Password) ) )
            .ForMember(dest => dest.phoneNumber, opt => opt.MapFrom(src => src.phoneNumber));
        // Map UserCreateDto to UserProfile
        CreateMap<UserCreateDto, UserProfile>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        
        // Map ExpenseUpdateDto to Expense
        CreateMap<ExpenseManipulationDto, Expense>().ReverseMap();

    }

    
    
}
