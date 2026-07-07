// ─────────────────────────────────────────────────────────────────────────────
// AuthProfile.cs
//
// AMAÇ: Auth feature'ının Entity ↔ DTO dönüşümlerini AutoMapper'a tanıtan profil.
// NEDEN: ApplicationServiceExtensions.AddAutoMapper(applicationAssembly) bu tip
//        Profile sınıflarını otomatik tarar; RegisterCommand ve RefreshCommand/
//        LoginCompletionService'teki elle mapping (new RegisterResponse(user.Id, ...))
//        yerini _mapper.Map<T>(user)'a bırakır — alan adları User entity'siyle
//        birebir eşleştiği için ek ForMember konfigürasyonu gerekmez.
// BAĞIMLILIKLAR: AutoMapper, WordLearner.Domain.Entities.User, DTOs/Auth.
// ─────────────────────────────────────────────────────────────────────────────

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
