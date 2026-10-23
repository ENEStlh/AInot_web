namespace AiNotes.Api.Dtos.Auth;

public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt);