using AutoMapper;
using backend.Dtos;
using backend.Models;
using backend.Requests;

namespace backend.Mappings;

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
        CreateMap<Event, EventDto>();
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