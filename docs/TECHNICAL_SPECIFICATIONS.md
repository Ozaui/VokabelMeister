# TEKNİK SPESİFİKASYONLAR

## 1. NuGet Paketleri (Backend)

```xml
<!-- WordLearner.API -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.MSSqlServer" Version="6.6.0" />   <!-- ApplicationLog tablosu -->

<!-- WordLearner.Application -->
<PackageReference Include="MediatR" Version="12.1.1" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="FluentValidation" Version="11.9.2" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.2" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.1.0" />
<PackageReference Include="Google.Apis.Auth" Version="1.67.0" />

<!-- WordLearner.Infrastructure -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
<PackageReference Include="MailKit" Version="4.3.0" />                     <!-- SMTP e-posta -->

<!-- WordLearner.Tests (xUnit) -->
<PackageReference Include="xunit" Version="2.7.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
<PackageReference Include="Moq" Version="4.20.70" />                        <!-- Repository/dış servis mock -->
<PackageReference Include="FluentAssertions" Version="6.12.0" />           <!-- Okunabilir assertion sözdizimi -->
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />  <!-- Repository<T> testleri -->
<PackageReference Include="coverlet.collector" Version="6.0.2" />          <!-- Coverage raporu (F-01) -->

```

## 2. npm Paketleri

```bash
# Web (/web)
npm create vite@latest web -- --template react-ts && cd web
npm i @reduxjs/toolkit react-redux axios react-hook-form react-router-dom @react-oauth/google
npm i -D tailwindcss postcss autoprefixer && npx tailwindcss init -p

# Admin (/admin) — Google/Apple yok
npm create vite@latest admin -- --template react-ts && cd admin
npm i @reduxjs/toolkit react-redux axios react-hook-form react-router-dom
npm i -D tailwindcss postcss autoprefixer && npx tailwindcss init -p

# Mobil (/mobile)
npx create-expo-app mobile --template expo-template-blank-typescript && cd mobile
npm i @reduxjs/toolkit react-redux axios react-hook-form i18next react-i18next
npx expo install expo-secure-store expo-av expo-image-picker expo-apple-authentication
npx expo install @react-navigation/native @react-navigation/bottom-tabs @react-navigation/stack
npm i @react-native-google-signin/google-signin
```

## 3. appsettings.json

> Hassas değerler (`SecretKey`, bağlantı dizesi) dev'de `appsettings.Development.json`'da (`.gitignore`'da),
> prod'da ortam değişkeninde. Tam liste → `ENV.md`.

```json
{
  "ConnectionStrings": { "DefaultConnection": "Server=.;Database=WordLearnerDB;Trusted_Connection=true;TrustServerCertificate=true;" },
  "Jwt": { "SecretKey": "MIN_32_KARAKTER", "Issuer": "WordLearnerApp", "Audience": "WordLearnerApp",
           "ExpirationMinutes": 15, "RefreshTokenExpirationDays": 7 },
  "Cors": { "AllowedOrigins": ["http://localhost:5173", "http://localhost:5174", "http://localhost:8081"] }
}
```

## 4. BaseEntity
```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

## 5. JWT Token Servisi
```csharp
public record RefreshTokenResult(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshTokenResult GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _cfg;
    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:SecretKey"]!));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("firstName", user.FirstName)
        };
        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"], audience: _cfg["Jwt:Audience"], claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshTokenResult GenerateRefreshToken()
    {
        var bytes = new byte[64]; RandomNumberGenerator.Fill(bytes);
        var days = _cfg.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
        return new RefreshTokenResult(Convert.ToBase64String(bytes), DateTime.UtcNow.AddDays(days));
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:SecretKey"]!));
        var p = new TokenValidationParameters {
            ValidateIssuerSigningKey = true, IssuerSigningKey = key,
            ValidateIssuer = false, ValidateAudience = false, ValidateLifetime = false };
        try {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, p, out var validated);
            // JWT Algorithm Confusion Attack önlemi:
            if (validated is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                return null;
            return principal;
        } catch { return null; }
    }
}
```

## 6. Şifre Servisi (BCrypt + SHA-256 token hash)
```csharp
public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
    string HashToken(string token);   // SHA-256 — refresh/OTP token hash
}

public class PasswordService : IPasswordService
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    public string HashToken(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
    }
}
```

## 7. Generic Repository
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task SoftDeleteAsync(int id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly WordLearnerDbContext _db;
    protected readonly DbSet<T> _set;
    public Repository(WordLearnerDbContext db) { _db = db; _set = db.Set<T>(); }

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(e => e.Id == id, ct);
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.ToListAsync(ct);
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    { await _set.AddAsync(entity, ct); await _db.SaveChangesAsync(ct); return entity; }
    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    { entity.UpdatedAt = DateTime.UtcNow; _set.Update(entity); await _db.SaveChangesAsync(ct); }
    public virtual async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await GetByIdAsync(id, ct) ?? throw new EntityNotFoundException($"{typeof(T).Name} {id} bulunamadı");
        e.IsDeleted = true; e.DeletedAt = DateTime.UtcNow; await UpdateAsync(e, ct);
    }
    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
```

## 8. SM-2 SRS Algoritması

Öz değerlendirme (4'lü): 🔴 Bilmedim=0 · 🟠 Zor=2 · 🟢 İyi=4 · 🔵 Çok Kolay=5.

```csharp
public static class SrsCalculator
{
    // SM-2 — quality: 0-5 (kullanıcı öz değerlendirmesi)
    public static (int intervalDays, int newLevel, decimal newEF) Calculate(
        int currentLevel, int repetitionNumber, decimal easinessFactor, int quality)
    {
        // quality < 3 → yanlış/çok zor → başa dön (EF düşer ama 1.3'ün altına inmez)
        if (quality < 3)
            return (1, 0, Math.Max(1.3m, easinessFactor - 0.2m));

        int interval = repetitionNumber == 0 ? 1
                     : repetitionNumber == 1 ? 3
                     : (int)Math.Round((repetitionNumber - 1) * easinessFactor);

        // EF = EF + (0.1 - (5-q)*(0.08 + (5-q)*0.02))
        decimal newEF = easinessFactor + (0.1m - (5 - quality) * (0.08m + (5 - quality) * 0.02m));
        newEF = Math.Max(1.3m, newEF);
        return (interval, Math.Min(currentLevel + 1, 5), newEF);
    }
}
```

## 9. Serilog + ApplicationLog (DB sink)

```csharp
// Program.cs — _logger çağrıları konsol + dosya + ApplicationLog tablosuna gider
builder.Host.UseSerilog((ctx, cfg) => cfg
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.MSSqlServer(
        connectionString: ctx.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new MSSqlServerSinkOptions { TableName = "ApplicationLog", AutoCreateSqlTable = false },
        columnOptions: ApplicationLogColumns()));   // SourceContext, RequestPath, UserId ek kolonlar
```
> `ActivityLog` ve `SecurityLog` Serilog ile DEĞİL, özel `IActivityLogger`/`ISecurityLogger`
> servisleriyle yazılır (yapılandırılmış sütunlar + admin filtresi için). Bkz. `DATABASE_SCHEMA.md §3`.

## 10. Program.cs (özet)
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(/* §9 */);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<WordLearnerDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddInfrastructureServices();   // repo + logging + DbContext yardımcıları
builder.Services.AddApplicationServices();       // servisler
builder.Services.AddValidatorsFromAssembly(typeof(CreateWordCommand).Assembly);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
    opt.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero });
builder.Services.AddCors(o => o.AddPolicy("Default", p =>
    p.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
     .AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();              // avatar/görsel
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```
