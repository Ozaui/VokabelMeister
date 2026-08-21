using AutoMapper;
using Zausel.Application.DTOs.UserCategories;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Application.Features.UserCategories;

// Projenin İLK gerçek AutoMapper kullanımı — CLAUDE.md §3 koşulu: Handler doğrudan
// `new UserCategoryResponse(entity.Id, entity.Alan…)` yapacaktı (CategoryMapping/WordMapping'in
// AKSİNE, tek entity → tek DTO, aggregate/çeviri birleştirmesi YOK), bu yüzden elle yazmak yerine Profile.
public class UserCategoryProfile : Profile
{
    public UserCategoryProfile() => CreateMap<UserCategory, UserCategoryResponse>();
}
