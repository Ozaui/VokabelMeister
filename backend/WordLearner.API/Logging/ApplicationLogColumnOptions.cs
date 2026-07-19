// ─────────────────────────────────────────────────────────────────────────────
// ApplicationLogColumnOptions.cs
//
// AMAÇ: Serilog'un MSSqlServer sink'ini ApplicationLog tablosunun GERÇEK şemasıyla
//       (ApplicationLogConfiguration + AddLoggingTables migration) birebir eşleştiren
//       ColumnOptions'ı üretir.
// NEDEN: Sink'in varsayılan kolon seti (Message + ayrı MessageTemplate + XML Properties)
//        bizim şemamızla uyuşmuyor — migration'da MessageTemplate hiç yok, Properties tek
//        bir JSON kolonu. AutoCreateSqlTable=false olduğu için (Program.cs) sink kendi
//        tablosunu oluşturmaz, var olan şemaya INSERT atmaya çalışır; ColumnOptions
//        şemadan saparsa (ör. olmayan bir MessageTemplate kolonuna yazmaya çalışırsa)
//        her log satırı sessizce/hata ile kaybolur. Bu yüzden bu eşleme ayrı, test
//        edilebilir bir metotta izole edildi (TECHNICAL_SPECIFICATIONS.md §9'daki
//        `ApplicationLogColumns()` referansının karşılığı).
// BAĞIMLILIKLAR: Serilog.Sinks.MSSqlServer.
// ─────────────────────────────────────────────────────────────────────────────

using System.Data;
using Serilog.Sinks.MSSqlServer;

namespace WordLearner.API.Logging;

public static class ApplicationLogColumnOptions
{
    // AMAÇ: ApplicationLog tablosuna yazılacak kolon eşlemesini döner.
    // NASIL: 1) Id BIGINT'e çevrilir (migration'daki tip). 2) MessageTemplate kaldırılır —
    //        şemada karşılığı yok, Message zaten şablonla doldurulmuş nihai metni tutuyor.
    //        3) Varsayılan XML "Properties" kolonu kaldırılıp yerine LogEvent kolonu
    //        "Properties" adıyla JSON olarak yazılır (DATABASE_SCHEMA/Loglama.md'nin
    //        istediği "Serilog yapılandırılmış özellikler (JSON)" formatı). 4) Şemadaki
    //        ekstra kolonlar (SourceContext otomatik enrichment, RequestPath/UserId
    //        RequestResponseLoggingMiddleware'in LogContext'e pushladığı özel alanlar —
    //        bkz. o dosyadaki NEDEN notu) AdditionalColumns'a eklenir.
    public static ColumnOptions Build()
    {
        var columnOptions = new ColumnOptions();

        columnOptions.Id.DataType = SqlDbType.BigInt;

        columnOptions.Store.Remove(StandardColumn.MessageTemplate);
        columnOptions.Store.Remove(StandardColumn.Properties);
        columnOptions.Store.Add(StandardColumn.LogEvent);
        columnOptions.LogEvent.ColumnName = "Properties";
        // NEDEN: SourceContext/RequestPath/UserId zaten kendi AdditionalColumns'larında
        //        ayrı ayrı saklanıyor — Properties JSON'unda tekrar etmelerine gerek yok.
        columnOptions.LogEvent.ExcludeAdditionalProperties = true;

        columnOptions.AdditionalColumns =
        [
            new SqlColumn
            {
                ColumnName = "SourceContext",
                PropertyName = "SourceContext",
                DataType = SqlDbType.NVarChar,
                DataLength = 255,
            },
            new SqlColumn
            {
                ColumnName = "RequestPath",
                PropertyName = "RequestPath",
                DataType = SqlDbType.NVarChar,
                DataLength = 500,
            },
            new SqlColumn
            {
                ColumnName = "UserId",
                PropertyName = "UserId",
                DataType = SqlDbType.Int,
            },
        ];

        return columnOptions;
    }
}
