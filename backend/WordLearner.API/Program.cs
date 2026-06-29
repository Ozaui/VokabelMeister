// ─────────────────────────────────────────────────────────────────────────────
// Program.cs
//
// AMAÇ: Uygulamanın giriş noktası (composition root). Servisleri DI konteynerine
//       kaydeder ve HTTP istek hattını (middleware pipeline) kurar.
// NEDEN: .NET 9 minimal hosting modelinde tüm başlangıç yapılandırması tek dosyada
//        toplanır; her API/altyapı parçası buraya bağlanır.
// BAĞIMLILIKLAR: ASP.NET Core, Swashbuckle (Swagger UI).
//
// NOT (A-01 — Proje İskeleti): Bu dosya şimdilik yalnızca TEMEL yapıdır
//   (controller + Swagger + sağlık kontrolü için iskelet). A-02 "Ortak Altyapı"
//   adımında genişletilecek: DbContext, JWT auth, CORS, Serilog, FluentValidation,
//   MediatR, AutoMapper kayıtları ve global exception / güvenlik başlıkları
//   middleware'leri eklenecek (bkz. TECHNICAL_SPECIFICATIONS.md §10).
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ADIM 1: Controller'ları ekle — API uç noktaları controller sınıflarında tanımlanır.
builder.Services.AddControllers();

// ADIM 2: Swagger/OpenAPI — yazılan endpoint'ler geliştirme ortamında otomatik
// belgelenir ve http://localhost:5001/swagger adresinden test edilir.
// NEDEN: Junior geliştirici ve manuel test için canlı API dokümantasyonu sağlar.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "VokabelMeister API",
            Version = "v1",
            Description = "Almanca-Türkçe kelime öğrenme uygulaması Web API'si.",
        }
    );
});

var app = builder.Build();

// ADIM 3: İstek hattı (pipeline) — yalnızca geliştirmede Swagger arayüzü açılır.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VokabelMeister API v1"));
}

app.UseHttpsRedirection();

// NEDEN: Auth henüz A-03'te yapılandırılacak; iskelet aşamasında yetkilendirme
// middleware'i hattın doğru yerinde durması için eklenir.
app.UseAuthorization();

app.MapControllers();

app.Run();
