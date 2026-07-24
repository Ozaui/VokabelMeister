// ─────────────────────────────────────────────────────────────────────────────
// MediaUploadResponse.cs
//
// AMAÇ: POST /media/images/upload yanıt zarfı.
// NEDEN: Herhangi bir domain'e (Auth/Icerik/...) ait olmadığı için DTOs/ altında
//        flat durur (HealthResponse ile aynı gerekçe — YAGNI, tek endpoint).
// BAĞIMLILIKLAR: Yok.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs;

public record MediaUploadResponse(string Url);
