# MİMARİ TASARIM

## 1. Genel Sistem Mimarisi

```
┌─────────────────────┐   ┌─────────────────────┐
│  React Native App   │   │  React Admin Panel  │
│  iOS & Android      │   │  Web (React + Vite) │
└──────────┬──────────┘   └──────────┬──────────┘
           └─────────────────────────┘
                          │ HTTPS / TLS 1.3
                          ▼
           ┌────────────────────────────────┐
           │        .NET 9 Web API          │
           │  Controllers / Middleware       │
           │  Services / Repositories        │
           └──────────────┬─────────────────┘
                          │
                          ▼
           ┌────────────────────────────────┐
           │        MS SQL Server           │
           └────────────────────────────────┘
```

**İki İstemci, Tek API:**
- **React Native (Expo):** Kullanıcıların kelime öğrendiği mobil uygulama
- **React Admin Panel:** Admin/Instructor'ın sistem içeriğini yönettiği web arayüzü — aynı API, sadece Admin/Instructor rolüyle erişilen endpoint'ler. Giriş yalnızca e-posta + şifre ile yapılır (Google/Apple girişi yoktur).

---

## 2. Geliştirme Sırası ve Gerekçesi

```
AŞAMA 1 ── Backend (Faz 1)
  Tüm entity'ler, veritabanı, servisler, endpoint'ler
  Neden önce: Frontend'in bağlanacağı sözleşmeyi belirler

AŞAMA 2 ── Admin Panel (Faz 3)
  Kelime, kategori, sınıf içeriği girilir
  Neden admin önce: Mobil test için gerçek içerik lazım

AŞAMA 3 ── Mobil Uygulama (Faz 2)
  Backend ve içerik hazır → gerçekçi geliştirme ve test
```

---

## 3. Kullanım Akışı — Kullanıcı Perspektifi

### 3.1 İlk Kayıt ve Giriş

```
Kullanıcı uygulamayı indirir
        │
        ▼
Kayıt olur (e-posta + şifre)
        │
        ▼
Seviye seçimi (A1 / Seviyemi bilmiyorum)
        │
        ▼
Ana ekrana yönlendirilir
        │
        ├── Öğren         → Sistem kelimelerini SRS ile öğren
        ├── Kartlarım     → Kişisel kart oluştur / yönet
        ├── Kategoriler   → Sistem kategorilerini gör
        ├── Sınıf         → Sınıfa katıl / sınıf oluştur
        └── Profil        → İstatistikler, ayarlar
```

### 3.2 Sistem Kelimelerini Öğrenme

```
Kullanıcı "Öğren" sekmesine gider
        │
        ▼
Filtre seçer:
  ├── Seviye (A1, A2, ...)
  ├── Kategori (birden fazla seçilebilir)
  └── Sınav türü (Flashcard, Quiz, Artikel vb.)
        │
        ▼
SRS algoritması sıralamayı belirler
(en çok review zamanı gelmiş kelimeler önce)
        │
        ▼
Kelime kartı gösterilir
  ├── Sistem kartı: artikel + kelime + cinsiyet rengi + örnek cümle
  └── Kullanıcı seviyesine uygun örnek cümle gösterilir
        │
        ▼
Kullanıcı cevap verir → Geri bildirim → XP kazanır
        │
        ▼
SRS sonraki tekrar zamanını hesaplar
```

### 3.3 Kişisel Kart Oluşturma

```
Kullanıcı "Kartlarım" sekmesine gider
        │
        ▼
"Yeni Kart" butonuna basar
        │
        ▼
Kart formu doldurur:
  ├── Ön yüz   (örn: "der Hund")
  ├── Arka yüz (örn: "Köpek")
  ├── Notlar   (opsiyonel)
  └── Örnek cümle ekle (opsiyonel)
        │
        ▼
Kategori bağlar:
  ├── Sistem kategorisi seç (örn: "Hayvanlar")
  └── Kendi kategorisini seç / oluştur (örn: "Favori Kelimelerim")
        │
        ▼
Kart kaydedilir
        │
        ├── Sadece bu kullanıcı görür (varsayılan: Özel)
        │
        ├── "Herkese Aç" yapılırsa → Sistem kelimeleri gibi görünür
        │   (Admin onayı gerekebilir)
        │
        └── Paylaşım linki oluşturulabilir →
            Arkadaş linki açarsa kartı/kategoriyi kendi listesine ekleyebilir
```

### 3.4 Paylaşım Sistemi

```
Kullanıcı bir kartı veya kategoriyi paylaşmak ister
        │
        ▼
"Paylaş" butonuna basar
        │
        ▼
Benzersiz paylaşım linki oluşturulur
  Örn: app.wordlearner.com/share/abc123xyz
        │
        ▼
Link arkadaşa gönderilir
        │
        ▼
Arkadaş linki açar:
  ├── Uygulamada giriş yapmışsa → Önizleme gösterilir
  │   └── "Kendi listememe ekle" butonu
  │       └── Kart/kategori kendi UserCards/UserCategories'ine kopyalanır
  └── Giriş yapmamışsa → Giriş/Kayıt sayfasına yönlendirilir
```

### 3.5 Sınıf Sistemi

```
Öğretmen/Kullanıcı sınıf oluşturur
  └── Sınıf adı, açıklama, davet kodu
        │
        ▼
Sınıf içeriği belirlenir:
  ├── Sistem kategorileri eklenir
  └── Kişisel kartlar/kategoriler eklenir
        │
        ▼
Davet kodu veya link paylaşılır
        │
        ▼
Öğrenci koda girerek sınıfa katılır
        │
        ▼
Sınıf içeriği öğrencinin "Kartlarım" ve "Kategoriler"
ekranında görünür
        │
        ▼
Öğretmen sınıf istatistiklerini görebilir:
  ├── Kim kaç kelime öğrendi
  ├── Ortalama başarı oranı
  └── En çok sorun çıkan kelimeler
```

### 3.6 Arkadaş Sistemi

```
Kullanıcı arkadaş ekler (kullanıcı adı veya e-posta ile)
        │
        ▼
Arkadaşlık isteği gönderilir → Onaylanır
        │
        ▼
Arkadaşların profili görülebilir:
  ├── Seviye, XP, streak
  └── Paylaşıma açık kategoriler/kartlar
        │
        ▼
Arkadaşın paylaşıma açık kategorisini kendi listene ekleyebilirsin
```

---

## 4. Kullanım Akışı — Admin Perspektifi

```
Admin panel'e giriş yapar (Admin/Instructor rolü)
        │
        ├── Kelime Yönetimi
        │     ├── Yeni kelime ekle (Almanca + çeviri + gramer detayları)
        │     ├── Örnek cümle ekle (seviye + tür)
        │     ├── Kategoriye bağla (birden fazla)
        │     └── Yayın durumu (Aktif/Pasif)
        │
        ├── Kategori Yönetimi
        │     ├── Yeni kategori oluştur
        │     ├── Hiyerarşi düzenle (alt/üst kategori)
        │     └── Görünürlük (Aktif/Pasif)
        │
        ├── Kullanıcı Yönetimi
        │     ├── Kullanıcıları listele
        │     ├── Rol değiştir (User → Instructor)
        │     └── Hesap dondur/aktif et
        │
        ├── İçerik Moderasyonu
        │     ├── "Herkese Açık" yapılan kullanıcı kartlarını incele
        │     ├── Onayla veya reddet
        │     └── Uygunsuz içerik bildirimleri
        │
        └── İstatistikler
              ├── En çok öğrenilen kelimeler
              ├── En çok sorun çıkan kelimeler
              └── Aktif kullanıcı sayısı
```

---

## 5. İçerik Görünürlük Matrisi

| İçerik Türü | Varsayılan | Herkese Açık | Paylaşım Linki | Sınıf/Arkadaş |
|-------------|-----------|--------------|----------------|----------------|
| Sistem Kelimeleri | ✅ Herkes | — | — | — |
| Kullanıcı Kartı | 🔒 Sadece sahibi | ✅ Admin onayıyla | ✅ Linki olan | ✅ Sınıf üyeleri |
| Kullanıcı Kategorisi | 🔒 Sadece sahibi | ✅ Admin onayıyla | ✅ Linki olan | ✅ Sınıf üyeleri |
| Sınıf İçeriği | 🔒 Sınıf üyeleri | — | ✅ Davet linki | — |

---

## 6. Backend Katmanlı Mimari

```
WordLearner.API              → HTTP katmanı (Controllers, Middleware)
WordLearner.Application      → İş mantığı (Services, DTOs, Validators, MediatR)
WordLearner.Infrastructure   → Veri erişimi (Repositories, DbContext)
WordLearner.Domain           → Entities, Interfaces
```

---

## 7. Entity İlişkileri

```
User
 ├── 1:N ──► RefreshToken
 ├── 1:N ──► UserProgress          (sistem kelimesi SRS)
 ├── 1:N ──► LearningHistory
 ├── 1:N ──► LearningSession
 ├── 1:N ──► UserAchievement
 ├── 1:N ──► UserCard              (kişisel kartlar)
 ├── 1:N ──► UserCardProgress      (kişisel kart SRS)
 ├── 1:N ──► UserCategory          (kişisel kategoriler)
 ├── 1:N ──► ClassMembership       (sınıf üyelikleri)
 ├── 1:N ──► Friendship            (arkadaşlıklar)
 └── 1:N ──► SharedContent        (paylaşım linkleri)

Word  (Sistem Kelimeleri)
 ├── 1:1 ──► WordDetail
 ├── 1:N ──► WordExample
 ├── M:N ──► Category (WordCategory)
 └── 1:N ──► UserProgress

UserCard  (Kişisel Kartlar)
 ├── N:1 ──► User
 ├── 1:N ──► UserCardExample
 ├── M:N ──► Category (UserCardCategory)
 ├── M:N ──► UserCategory (UserCardUserCategory)
 ├── 1:N ──► UserCardProgress
 └── 1:N ──► SharedContent

Category  (Sistem Kategorileri)
 ├── Self-referencing (ParentCategoryId)
 ├── M:N ──► Word
 └── M:N ──► UserCard

UserCategory  (Kişisel Kategoriler)
 ├── N:1 ──► User
 ├── M:N ──► UserCard
 └── 1:N ──► SharedContent

Class  (Sınıflar)
 ├── N:1 ──► User (owner)
 ├── 1:N ──► ClassMembership
 ├── M:N ──► Category (ClassCategory)
 └── M:N ──► UserCategory (ClassUserCategory)

Friendship
 ├── N:1 ──► User (requester)
 └── N:1 ──► User (receiver)

SharedContent  (Paylaşım Linkleri)
 ├── N:1 ──► User (owner)
 ├── ContentType: UserCard | UserCategory | Class
 └── ContentId: ilgili kaydın ID'si
```

### 7.1 Entity Özeti

| Entity | Tür | Açıklama |
|--------|-----|----------|
| User | Temel | Profil, kimlik doğrulama, istatistikler |
| RefreshToken | Auth | JWT refresh token |
| Word | Sistem | Admin tarafından eklenen kelimeler |
| WordDetail | Sistem | Almanca gramer (cinsiyet, artikeller, çekimler) |
| WordExample | Sistem | Seviyeli + türlü örnek cümleler |
| Category | Sistem | Admin kategorileri (hiyerarşik) |
| WordCategory | Sistem | Word ↔ Category M:N |
| UserProgress | Öğrenme | Sistem kelimesi SRS takibi |
| LearningHistory | Öğrenme | Her girişim kaydı |
| LearningSession | Öğrenme | Oturum özeti |
| Achievement | Gamification | Rozet tanımları |
| UserAchievement | Gamification | Kazanılan rozetler |
| UserCard | Kişisel | Kullanıcı kartları |
| UserCardExample | Kişisel | Kart örnek cümleleri |
| UserCardProgress | Kişisel | Kişisel kart SRS takibi |
| UserCategory | Kişisel | Kullanıcı kategorileri |
| UserCardCategory | Kişisel | UserCard ↔ Category M:N |
| UserCardUserCategory | Kişisel | UserCard ↔ UserCategory M:N |
| Class | Sosyal | Sınıf/grup |
| ClassMembership | Sosyal | Sınıf üyelikleri |
| ClassCategory | Sosyal | Sınıf ↔ Category M:N |
| ClassUserCategory | Sosyal | Sınıf ↔ UserCategory M:N |
| Friendship | Sosyal | Arkadaşlık ilişkileri |
| SharedContent | Sosyal | Paylaşım linkleri |

---

## 8. Yetkilendirme Rolleri

| Rol | Kimler | Yetkiler |
|-----|--------|---------|
| User | Kayıtlı kullanıcılar | Kendi verileri, öğrenme, kişisel kart/kategori, sınıf, arkadaş |
| Instructor | İçerik editörleri | User + sistem kelimesi/kategori ekleme |
| Admin | Yöneticiler | Tüm yetkiler + kullanıcı yönetimi, moderasyon |

---

## 9. Klasör Yapısı

```
WordLearner/
├── CLAUDE.md
├── docs/
├── backend/
│   ├── WordLearner.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Program.cs
│   ├── WordLearner.Application/
│   │   ├── DTOs/
│   │   ├── Features/
│   │   ├── Services/
│   │   ├── Validators/
│   │   └── Mappings/
│   ├── WordLearner.Infrastructure/
│   │   ├── Data/
│   │   │   ├── WordLearnerDbContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   └── Repositories/
│   └── WordLearner.Domain/
│       ├── Entities/
│       └── Interfaces/
├── mobile/
├── admin/
└── WordLearner.sln
```

---

## 10. Kullanılan Teknolojiler

### Backend
| Teknoloji | Versiyon | Amaç |
|-----------|----------|------|
| .NET | 9 | Web API |
| EF Core | 9 | ORM |
| MediatR | 12 | CQRS |
| AutoMapper | 13 | DTO mapping |
| FluentValidation | 11 | Validasyon |
| BCrypt.Net-Next | 4 | Şifre hashleme |
| Serilog | 3 | Loglama |

### Mobile
| Teknoloji | Versiyon | Amaç |
|-----------|----------|------|
| React Native | 0.73+ | iOS & Android |
| Expo | SDK 50+ | Dev araçları |
| TypeScript | 5+ | Tip güvenliği |
| Redux Toolkit + RTK Query | 2+ | State + server state |
| Axios | 1+ | HTTP client |
| React Navigation | 6+ | Navigasyon |
| React Hook Form | 7+ | Form yönetimi |
| i18next | 23+ | Çok dil |
| Expo Secure Store | — | Token saklama |

### Admin Panel
| Teknoloji | Versiyon | Amaç |
|-----------|----------|------|
| React + Vite | 18+ / 5+ | UI + build |
| TypeScript | 5+ | Tip güvenliği |
| TailwindCSS | 3+ | Stil |
| RTK Query | 2+ | Server state |
| React Hook Form | 7+ | Form yönetimi |
