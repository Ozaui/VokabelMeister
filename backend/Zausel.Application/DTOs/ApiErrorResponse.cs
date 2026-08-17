namespace Zausel.Application.DTOs;

public record ApiErrorResponse(bool Success, ApiErrorDetail Error);

// Details yalnızca FluentValidation birden fazla kuralı aynı anda ihlal ettiğinde dolar (null
// olmayan diğer tüm exception tiplerinde Code/Message tek başına yeterli) — Code/Message HER
// zaman details[0] ile aynı, tek-hata okuyan eski/basit istemciler details'i hiç görmeden çalışır.
public record ApiErrorDetail(string Code, string Message, List<FieldError>? Details = null);

// Field camelCase (JSON gövde alan adıyla aynı) — Message Accept-Language'a göre çözülmüş, tr/de
// (CLAUDE.md §1 "kullanıcı/admin'in gördüğü her alan tr+de" ilkesiyle aynı, ErrorMessages sözlüğünden).
public record FieldError(string Field, string Code, string Message);
