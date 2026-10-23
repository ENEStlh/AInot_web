using System.ComponentModel.DataAnnotations;

namespace AiNotes.Api.Dtos.Notes;

public sealed record UpdateNoteRequest
{
    [Required, MaxLength(200)]
    public required string Title { get; init; }

    [Required, MaxLength(50000)]
    public required string Content { get; init; }
}