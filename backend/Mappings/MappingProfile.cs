using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using backend.Dtos;
using backend.Models;
using backend.Requests;

namespace backend.Mappings;

[ExcludeFromCodeCoverage]
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<UserRequest, User>(); // request → entity
        CreateMap<User, UserDto>(); // entity → response
        CreateMap<UserRequest, User>() // merge request and entity 
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));

        // Event mappings
        CreateMap<EventRequest, Event>();
        CreateMap<Event, EventDto>()
            .ForMember(dest => dest.StartDateTime,
                opt => opt.MapFrom(src =>
                    src.StartDate.ToDateTime(TimeOnly.MinValue).Add(src.StartTime.ToTimeSpan()).ToString("o")
                ))
            .ForMember(dest => dest.EndDateTime,
                opt => opt.MapFrom(src =>
                    src.EndDate.ToDateTime(TimeOnly.MinValue).Add(src.EndTime.ToTimeSpan()).ToString("o")
                ));

        CreateMap<EventRequest, Event>()
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

/*
CreateMap<UserRequest, User>()
            .ForMember(dest => dest.Name,
                opt =>
                    opt.MapFrom(src => src.Name));
*/