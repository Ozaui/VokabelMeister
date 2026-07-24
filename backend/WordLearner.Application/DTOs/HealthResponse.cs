namespace WordLearner.Application.DTOs;

public record HealthResponse(string Status, bool DatabaseConnected, DateTime TimestampUtc);
