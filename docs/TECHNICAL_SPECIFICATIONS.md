# TEKNİK SPESİFİKASYONLAR

## 1. NuGet Paket Listesi (Backend)

```xml
<!-- WordLearner.API -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />

<!-- WordLearner.Application -->
<PackageReference Include="MediatR" Version="12.1.1" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="FluentValidation" Version="11.9.2" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.2" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.1.0" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="7.1.0" />

<!-- WordLearner.Infrastructure -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
```

## 2. npm Paket Listesi (Mobile)

```bash
# Temel
npx create-expo-app mobile --template expo-template-blank-typescript

# State ve HTTP
npm install @reduxjs/toolkit react-redux axios

# Navigasyon
npx expo install @react-navigation/native @react-navigation/bottom-tabs @react-navigation/stack
npx expo install react-native-screens react-native-safe-area-context react-native-gesture-handler

# Form ve validasyon
npm install react-hook-form

# Güvenli depolama
npx expo install expo-secure-store

# Medya
npx expo install expo-av expo-image-picker

# Çok dil
npm install i18next react-i18next

# UI
npm install react-native-paper
```

## 3. npm Paket Listesi (Admin Panel)

```bash
npm create vite@latest admin -- --template react-ts
cd admin
npm install @reduxjs/toolkit react-redux axios
npm install react-hook-form
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

## 4. appsettings.json Yapısı

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WordLearnerDB;Trusted_Connection=true;"
  },
  "Jwt": {
    "SecretKey": "BURAYA_MIN_32_KARAKTER_GIZLI_ANAHTAR",
    "Issuer": "WordLearnerApp",
    "Audience": "WordLearnerApp",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  }
}
```

## 5. BaseEntity

```csharp
// Domain/Common/BaseEntity.cs
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
```

## 6. JWT Token Servisi

```csharp
// Application/Services/JwtTokenService.cs
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
        => _configuration = configuration;

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("firstName", user.FirstName)
        };

        var token = new JwtSecurityToken(
            issuer:   _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        // Kriptografik güvenli 64 byte → Base64
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false   // Süresi dolmuş token'ı okumak için
        };

        try
        {
            return new JwtSecurityTokenHandler()
                .ValidateToken(token, parameters, out _);
        }
        catch { return null; }
    }
}
```

## 7. Şifre Servisi (BCrypt)

```csharp
// Application/Services/PasswordService.cs
public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public class PasswordService : IPasswordService
{
    // Work factor 12 → güvenlik / performans dengesi
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
```

## 8. Generic Repository

```csharp
// Infrastructure/Repositories/Repository.cs
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

    public Repository(WordLearnerDbContext db)
    {
        _db  = db;
        _set = db.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => _set.ToListAsync(ct).ContinueWith(t => (IEnumerable<T>)t.Result, ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _set.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public virtual async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"{typeof(T).Name} {id} bulunamadı");
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await UpdateAsync(entity, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
```

## 9. Redux Toolkit — Auth Slice

```typescript
// store/authSlice.ts
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { authService } from '../services/authService';
import * as SecureStore from 'expo-secure-store';

export const login = createAsyncThunk(
  'auth/login',
  async (credentials: { email: string; password: string }, { rejectWithValue }) => {
    try {
      const res = await authService.login(credentials);
      await SecureStore.setItemAsync('accessToken',  res.data.accessToken);
      await SecureStore.setItemAsync('refreshToken', res.data.refreshToken);
      return res.data.user;
    } catch (err: any) {
      return rejectWithValue(err.response?.data?.error?.message ?? 'Giriş başarısız');
    }
  }
);

export const logout = createAsyncThunk('auth/logout', async () => {
  await authService.logout();
  await SecureStore.deleteItemAsync('accessToken');
  await SecureStore.deleteItemAsync('refreshToken');
});

const authSlice = createSlice({
  name: 'auth',
  initialState: { user: null as any, isAuthenticated: false, isLoading: false, error: null as string | null },
  reducers: {
    clearError: (state) => { state.error = null; }
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending,   (s) => { s.isLoading = true; s.error = null; })
      .addCase(login.fulfilled, (s, a) => { s.isLoading = false; s.isAuthenticated = true; s.user = a.payload; })
      .addCase(login.rejected,  (s, a) => { s.isLoading = false; s.error = a.payload as string; })
      .addCase(logout.fulfilled,(s) => { s.user = null; s.isAuthenticated = false; });
  }
});

export const { clearError } = authSlice.actions;
export default authSlice.reducer;
```

## 10. Axios Interceptor

```typescript
// services/api.ts
import axios from 'axios';
import * as SecureStore from 'expo-secure-store';

const api = axios.create({
  baseURL: process.env.EXPO_PUBLIC_API_URL,
  timeout: 10000,
});

// Her isteğe token ekle
api.interceptors.request.use(async (config) => {
  const token = await SecureStore.getItemAsync('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// 401 gelirse token yenile
api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const original = error.config;
    if (error.response?.status === 401 && !original._retry) {
      original._retry = true;
      try {
        const refresh = await SecureStore.getItemAsync('refreshToken');
        const { data } = await api.post('/auth/refresh', { refreshToken: refresh });
        await SecureStore.setItemAsync('accessToken',  data.data.accessToken);
        await SecureStore.setItemAsync('refreshToken', data.data.refreshToken);
        original.headers.Authorization = `Bearer ${data.data.accessToken}`;
        return api(original);
      } catch {
        await SecureStore.deleteItemAsync('accessToken');
        await SecureStore.deleteItemAsync('refreshToken');
        // TODO: navigateTo('Login')
      }
    }
    return Promise.reject(error);
  }
);

export default api;
```

## 11. SM-2 SRS Algoritması

### Kullanıcı Öz Değerlendirme (4'lü Sistem)

Mobil/Web UI'da her kartın arkasında 4 buton gösterilir:

| Buton | UI Etiketi | SelfRating (quality) | Sonuç |
|-------|-----------|----------------------|-------|
| 🔴 | Bilmedim | 0 | Level 0 reset, EF düşer |
| 🟠 | Zor | 2 | Interval kısalır, EF düşer |
| 🟢 | İyi | 4 | Normal ilerleme |
| 🔵 | Çok Kolay | 5 | Interval uzar, EF yükselir |

### Algoritma Kodu

```csharp
// Application/Services/SrsService.cs
public static class SrsCalculator
{
    // SM-2 algoritması — quality: 0-5 (kullanıcı öz değerlendirmesi)
    // 🔴 Bilmedim=0  🟠 Zor=2  🟢 İyi=4  🔵 Çok Kolay=5
    public static (int intervalDays, int newLevel, decimal newEF) Calculate(
        int currentLevel,
        int repetitionNumber,
        decimal easinessFactor,
        int quality)  // 0-5: kullanıcının öz değerlendirmesi
    {
        // Yanlış veya çok zor (quality < 3) → başa dön
        if (quality < 3)
        {
            return (intervalDays: 1, newLevel: 0, newEF: Math.Max(1.3m, easinessFactor - 0.2m));
        }

        // Doğru → SM-2 interval hesapla
        int interval;
        if (repetitionNumber == 0)
            interval = 1;           // İlk başarılı tekrar → 1 gün
        else if (repetitionNumber == 1)
            interval = 3;           // İkinci başarılı tekrar → 3 gün
        else
            interval = (int)Math.Round((repetitionNumber - 1) * easinessFactor);

        // Easiness Factor güncelle: EF = EF + (0.1 - (5-q)*(0.08+(5-q)*0.02))
        // q = quality (0-5)
        decimal newEF = easinessFactor + (0.1m - (5 - quality) * (0.08m + (5 - quality) * 0.02m));
        newEF = Math.Max(1.3m, newEF);  // Minimum 1.3 — hiç sıfırlanmaz

        int newLevel = Math.Min(currentLevel + 1, 5);
        return (intervalDays: interval, newLevel: newLevel, newEF: newEF);
    }
}
```

## 12. MediatR Command Örneği

```csharp
// Application/Features/Words/Commands/CreateWord/CreateWordCommand.cs
public record CreateWordCommand(
    string GermanWord,
    string TurkishTranslation,
    string PartOfSpeech,
    string DifficultyLevel,
    List<int> CategoryIds
) : IRequest<WordDto>;

public class CreateWordCommandHandler : IRequestHandler<CreateWordCommand, WordDto>
{
    private readonly IWordRepository _words;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateWordCommandHandler> _logger;

    public CreateWordCommandHandler(IWordRepository words, IMapper mapper,
        ILogger<CreateWordCommandHandler> logger)
    {
        _words  = words;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WordDto> Handle(CreateWordCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("Yeni kelime oluşturuluyor: {Kelime}", cmd.GermanWord);

        var word = _mapper.Map<Word>(cmd);
        await _words.AddAsync(word, ct);

        return _mapper.Map<WordDto>(word);
    }
}
```

## 13. Program.cs Temel Yapısı

```csharp
var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((ctx, cfg) => cfg
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));

// Temel servisler
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabanı
builder.Services.AddDbContext<WordLearnerDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI kayıtları (extension metodlar ile)
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateWordCommand).Assembly));

// AutoMapper
builder.Services.AddAutoMapper(typeof(CreateWordCommand).Assembly);

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateWordCommand).Assembly);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ValidateIssuer   = true,
            ValidIssuer      = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience    = builder.Configuration["Jwt:Audience"],
            ClockSkew        = TimeSpan.Zero
        };
    });

// CORS
builder.Services.AddCors(opt => opt.AddPolicy("Default", p =>
    p.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
     .AllowAnyMethod()
     .AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```
