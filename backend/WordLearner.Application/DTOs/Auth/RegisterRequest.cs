namespace WordLearner.Application.DTOs.Auth;

// RegisterCommand'ın istemciden gelen alanları — Language JSON'da YOK, Controller Accept-Language'dan okur.
public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
