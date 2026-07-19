// ─────────────────────────────────────────────────────────────────────────────
// HealthResponse.cs
//
// AMAÇ: GET /health yanıt zarfı.
// NEDEN: Herhangi bir domain'e (Auth/Icerik/...) ait olmadığı için DTOs/ altında
//        flat durur (Auth/ gibi alt klasör açılmadı — YAGNI, tek endpoint).
// BAĞIMLILIKLAR: Yok.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs;

public record HealthResponse(string Status, bool DatabaseConnected, DateTime TimestampUtc);
