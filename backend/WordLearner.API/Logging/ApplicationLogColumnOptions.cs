using System.Data;
using Serilog.Sinks.MSSqlServer;

namespace WordLearner.API.Logging;

// Sink'in varsayılan kolon seti bizim şemamızla uyuşmuyor (MessageTemplate yok, Properties
// tek JSON kolonu) — AutoCreateSqlTable=false olduğu için sink var olan şemaya INSERT atar,
// ColumnOptions şemadan saparsa log satırları sessizce kaybolur.
public static class ApplicationLogColumnOptions
{
    public static ColumnOptions Build()
    {
        var columnOptions = new ColumnOptions();

        columnOptions.Id.DataType = SqlDbType.BigInt;

        columnOptions.Store.Remove(StandardColumn.MessageTemplate);
        columnOptions.Store.Remove(StandardColumn.Properties);
        columnOptions.Store.Add(StandardColumn.LogEvent);
        columnOptions.LogEvent.ColumnName = "Properties";

        // SourceContext/RequestPath/UserId kendi AdditionalColumns'larında ayrı saklanıyor —
        // Properties JSON'unda tekrar etmelerine gerek yok.
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
