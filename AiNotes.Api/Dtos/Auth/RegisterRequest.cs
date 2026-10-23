using System.ComponentModel.DataAnnotations;

namespace AiNotes.Api.Dtos.Auth;

public sealed record RegisterRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public required string Email { get; init; }

    [Required, MinLength(8), MaxLength(100)]
    public required string Password { get; init; }
}