namespace WordLearner.Application.Common.Models;

// code makine tarafından okunabilir (frontend buna göre özel davranış tetikleyebilir),
// message insan-okunur açıklama.
public record ApiErrorDetail(string Code, string Message);

public record ApiErrorResponse(ApiErrorDetail Error)
{
    public bool Success => false;

    public ApiErrorResponse(string code, string message)
        : this(new ApiErrorDetail(code, message)) { }
}
