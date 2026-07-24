// ─────────────────────────────────────────────────────────────────────────────
// IFileStorageService.cs
//
// AMAÇ: Bir görsel dosyasını kalıcı depolamaya (yerelde diske, ileride S3/Azure
//       Blob gibi bir buluta) yazıp herkese açık bir URL döndüren sözleşme.
// NEDEN: MediaController bu arayüzü çağırır — hangi somut depolamanın (yerel disk
//        vs. bulut) kullanıldığı controller'dan/Word Handler'larından gizlenir;
//        ileride yalnızca DI kaydı değişerek LocalFileStorageService bir
//        S3FileStorageService ile değiştirilebilir.
// BAĞIMLILIKLAR: Yok (soyut).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Interfaces.Services;

public interface IFileStorageService
{
    // AMAÇ: Bir görsel akışını (stream) doğrular (uzantı+boyut), benzersiz bir ad
    //       üretip depolamaya yazar ve herkese açık URL'ini döner.
    // NEDEN: Orijinal dosya adı asla olduğu gibi diske yazılmaz (çakışma + path
    //        traversal riski) — yalnızca uzantısı korunur, ad yeniden üretilir.
    // NASIL: originalFileName'den uzantı çıkarılır (izin verilen listede mi
    //        kontrol edilir), fileSizeBytes üst sınırla karşılaştırılır, ardından
    //        Guid tabanlı benzersiz bir ad ile içerik diske yazılır.
    Task<string> SaveImageAsync(
        Stream fileStream,
        string originalFileName,
        long fileSizeBytes,
        CancellationToken ct = default
    );
}
