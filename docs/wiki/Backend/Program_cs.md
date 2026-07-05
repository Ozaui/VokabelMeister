# Program.cs

**Özet:** `WordLearner.API`'nin .NET 9 minimal hosting modeliyle yazılmış composition root'u — servisleri DI konteynerine kaydeder ve HTTP middleware pipeline'ını kurar. **A-02 tamamlandı** — artık tam yapılandırma: Serilog (konsol+dosya), `AddInfrastructureServices`+`AddApplicationServices`, JWT Bearer authentication, CORS ve 3 özel middleware (loglama → güvenlik başlıkları → exception handling) sırayla kurulu. Gerçek bir `/api/v1/*` isteğiyle (404 dahil) doğrulandı: güvenlik başlıkları her yanıtta mevcut, Türkçe istek/yanıt logları konsola düşüyor.
**Kütüphaneler:** ASP.NET Core, Swashbuckle.AspNetCore (Swagger UI), Serilog.AspNetCore, Microsoft.AspNetCore.Authentication.JwtBearer, MediatR, AutoMapper, FluentValidation
**Bağlantılar:** [[WordLearner_API]] · [[InfrastructureServiceExtensions]] · [[ApplicationServiceExtensions]] · [[Middleware]] · [[ApiErrorResponse]] · [[Gelistirme_Yol_Haritasi]] · [[Teknik_Ozellikler]]

## Konum
`backend/WordLearner.API/Program.cs`

## Tam Pipeline (adım adım — A-02 sonu)
```
1. builder.Host.UseSerilog(...)                         → konsol + dosya (logs/app-.txt, günlük rotasyon)
   NEDEN Override: appsettings.json'daki Logging:LogLevel:Microsoft.AspNetCore=Warning
   yalnızca ASP.NET Core'un builtin logger'ı içindir; Serilog kod tabanlı yapılandırıldığından
   aynı susturma MinimumLevel.Override("Microsoft.AspNetCore", Warning) ile elle tekrarlanır.
2. builder.Services.AddControllers()
3. builder.Services.AddEndpointsApiExplorer() + AddSwaggerGen(...)
4. builder.Services.AddInfrastructureServices(builder.Configuration)  → bkz. [[InfrastructureServiceExtensions]]
   builder.Services.AddApplicationServices()                          → bkz. [[ApplicationServiceExtensions]]
5. builder.Services.AddAuthentication(JwtBearerDefaults...).AddJwtBearer(...)  → ClockSkew = Zero
   builder.Services.AddAuthorization()
6. builder.Services.AddCors("Default", ... Cors:AllowedOrigins ...)
─────────────────────────────────────────
7. app.Environment.IsDevelopment() ise → UseSwagger()/UseSwaggerUI() (http://localhost:5001/swagger)
8. app.UseMiddleware<RequestResponseLoggingMiddleware>()   ← en dışta (bkz. [[Middleware]])
   app.UseMiddleware<SecurityHeadersMiddleware>()
   app.UseMiddleware<ExceptionHandlingMiddleware>()        ← en içte
9. app.UseHttpsRedirection()
10. app.UseCors("Default")
11. app.UseAuthentication() / app.UseAuthorization()
12. app.MapControllers()
13. app.Run()
```

## Middleware Sıralama Kararı
Loglama en dışta durur ki bir exception fırlasa bile (ExceptionHandlingMiddleware onu 500'e çevirse
bile) gerçek süre ve nihai durum kodu `try/finally` ile ölçülüp loglanabilsin. Güvenlik başlıkları,
hata yanıtı dâhil her yanıta eklenmesi için exception middleware'inden önce (dışında) durur —
başlıklar response gövdesi yazılmadan eklendiği için sorun çıkarmaz.

## Henüz Eksik (sonraki fazlarda gelecek)
- Serilog'un `Serilog.Sinks.MSSqlServer` (`ApplicationLog`) sink'i → **A-04** (tablo migration'ı henüz yok)
- `AddValidatorsFromAssembly` somut bir feature validator'ı bulamıyor henüz (assembly boş) → **A-03+** otomatik dolacak
- `UseStaticFiles()` (avatar/görsel) → **A-08**

Tam kod (JWT `TokenValidationParameters`, Serilog `WriteTo` zinciri dahil) → `docs/API_YOL_HARITASI/A-02_ortak-altyapi.html` adım 11, ayrıca [[Teknik_Ozellikler]] §10.
