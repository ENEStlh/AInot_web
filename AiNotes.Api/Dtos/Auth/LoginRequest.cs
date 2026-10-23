using System.ComponentModel.DataAnnotations;

namespace AiNotes.Api.Dtos.Auth;

public sealed record LoginRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}