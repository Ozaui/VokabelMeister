namespace Zausel.Application.DTOs.Words;

// ExampleType nullable — boş bırakılırsa Handler "Normal" varsayar (WordExample.ExampleType'ın DB varsayılanıyla aynı).
public record WordExampleRequest(string SentenceText, string Level, string? ExampleType);
