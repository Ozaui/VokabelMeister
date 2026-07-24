// ─────────────────────────────────────────────────────────────────────────────
// IEncryptionService.cs
//
// AMAÇ: Hassas bir değeri (SMTP şifresi) DB'ye yazmadan önce şifrelemek/okurken
//       çözmek için sözleşme (REFERENCE/SECURITY.md §3.2).
// NEDEN: Handler'lar somut AES implementasyonuna değil bu arayüze bağımlı olmalı —
//        testlerde gerçek algoritma kullanılabilir (kripto saf/deterministik değil
//        ama dış bir I/O da değil, bu yüzden PasswordService gibi doğrudan
//        örneklenip test edilir — CODING_STANDARDS.md §7.4'ün "dış servisler mock'lanır"
//        kuralı ağ/SMTP/OAuth gibi GERÇEK dış çağrılar içindir, yerel kriptografi için değil).
// BAĞIMLILIKLAR: Yok — saf sözleşme.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Interfaces.Services;

public interface IEncryptionService
{
    // AMAÇ: Düz metni şifreler.
    // NASIL: Sonuç Base64(rastgele IV + AES-256-CBC cipher) — her çağrıda farklı bir IV
    //        üretilir, aynı düz metin iki kez şifrelense bile FARKLI bir sonuç üretir.
    string Encrypt(string plainText);

    // AMAÇ: Encrypt ile üretilmiş bir şifreli metni çözer.
    // NASIL: Base64 çözülür, ilk 16 bayt IV olarak ayrılır, kalanı AES-256-CBC ile çözülür.
    string Decrypt(string cipherText);
}
