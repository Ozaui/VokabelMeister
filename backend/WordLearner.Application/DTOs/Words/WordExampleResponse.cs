namespace WordLearner.Application.DTOs.Words;

public record WordExampleResponse(int Id, string SentenceText, string Level, string ExampleType, int? PairedExampleId, int DisplayOrder);
