using AutoMapper;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.Auth;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, RegisterResponse>();
        CreateMap<User, AuthUserDto>();
    }
}
