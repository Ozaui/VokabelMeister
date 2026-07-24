using AutoMapper;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Tests.Common;

internal static class AuthTestMapper
{
    public static IMapper Create() =>
        new MapperConfiguration(cfg => cfg.AddProfile<AuthProfile>()).CreateMapper();
}
