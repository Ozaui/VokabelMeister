# Program.cs

**Özet:** `WordLearner.API`'nin .NET 9 minimal hosting modeliyle yazılmış composition root'u — servisleri DI konteynerine kaydeder ve HTTP middleware pipeline'ını kurar. Şu an yalnızca **iskelet** hâlinde: Controller + Swagger + `UseAuthorization()` yer tutucusu var; DbContext kaydı, JWT auth, CORS, Serilog, FluentValidation, MediatR, AutoMapper ve global exception middleware'i **A-02'de eklenecek**.
**Kütüphaneler:** ASP.NET Core, Swashbuckle.AspNetCore (Swagger UI)
**Bağlantılar:** [[WordLearner_API]] · [[InfrastructureServiceExtensions]] · [[Gelistirme_Yol_Haritasi]] · [[Teknik_Ozellikler]]

## Konum
`backend/WordLearner.API/Program.cs`

## Mevcut Pipeline (adım adım)
```
1. builder.Services.AddControllers()
2. builder.Services.AddEndpointsApiExplorer() + AddSwaggerGen(...)
   → SwaggerDoc "v1": Title "VokabelMeister API", Description "Almanca-Türkçe kelime öğrenme uygulaması Web API'si"
3. app.Environment.IsDevelopment() ise → UseSwagger() + UseSwaggerUI() (http://localhost:5001/swagger)
4. app.UseHttpsRedirection()
5. app.UseAuthorization()   ← auth henüz yapılandırılmadı (A-03'te JWT eklenecek); pipeline'daki
   doğru konumunu korumak için şimdiden eklendi
6. app.MapControllers()
7. app.Run()
```

## A-02'de Eklenecekler (henüz yok)
- `builder.Services.AddInfrastructureServices(builder.Configuration)` çağrısı (bkz. [[InfrastructureServiceExtensions]])
- JWT Bearer authentication kaydı
- CORS policy (`Cors:AllowedOrigins` — bkz. [[WordLearner_API]])
- Serilog host builder entegrasyonu
- FluentValidation, MediatR, AutoMapper DI kayıtları
- Global exception middleware, security headers middleware, request/response log middleware

## Hedef Yapılandırma (docs/REFERENCE/TECHNICAL_SPECIFICATIONS.md §10 — tam kod referansı)

`docs/REFERENCE/TECHNICAL_SPECIFICATIONS.md`'de A-02 sonunda ulaşılması gereken **tam** `Program.cs` kodu
tanımlı — sıralama önemli (middleware pipeline order):

```
builder.Host.UseSerilog(...)
builder.Services.AddControllers() / AddEndpointsApiExplorer() / AddSwaggerGen()
builder.Services.AddDbContext<WordLearnerDbContext>(...) / AddInfrastructureServices(...) / AddApplicationServices(...)
builder.Services.AddValidatorsFromAssembly(...)
builder.Services.AddAuthentication(JwtBearerDefaults...).AddJwtBearer(...)   // ClockSkew = Zero
builder.Services.AddCors(...)
─────────────────────────────────────────
app.UseSwagger()/UseSwaggerUI()   (yalnızca Development)
app.UseMiddleware<SecurityHeadersMiddleware>()
app.UseMiddleware<ExceptionHandlingMiddleware>()
app.UseHttpsRedirection()
app.UseStaticFiles()              // avatar/görsel — A-08
app.UseCors("Default")
app.UseAuthentication()
app.UseAuthorization()
app.MapControllers()
```

Tam kod (JWT `TokenValidationParameters`, Serilog `WriteTo` zinciri dahil) → [[Teknik_Ozellikler]] §7.
Bu, mevcut minimal `Program.cs`'in ulaşacağı hedef; adım adım A-02/A-03/A-04/A-08 task'larında
parça parça eklenecek — tek seferde yazılmayacak (dikey dilim kuralı).
