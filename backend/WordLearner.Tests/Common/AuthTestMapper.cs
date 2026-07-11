// ─────────────────────────────────────────────────────────────────────────────
// AuthTestMapper.cs
//
// AMAÇ: Testlerde gerçek AuthProfile'dan kurulmuş bir IMapper örneği üretir.
// NEDEN: CODING_STANDARDS.md §7.4 — IMapper mock'lanmaz, gerçek Profile'dan kurulur.
//        Bu satır 3 test dosyasında (RegisterCommandHandlerTests, RefreshCommandHandlerTests,
//        LoginCompletionServiceTests) birebir kopyaydı (kod denetiminde bulunan DRY ihlali).
// BAĞIMLILIKLAR: AutoMapper, WordLearner.Application.Features.Auth.AuthProfile.
// ─────────────────────────────────────────────────────────────────────────────

using AutoMapper;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Tests.Common;

internal static class AuthTestMapper
{
    public static IMapper Create() =>
        new MapperConfiguration(cfg => cfg.AddProfile<AuthProfile>()).CreateMapper();
}
