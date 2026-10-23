namespace AiNotes.Api.Dtos.Notes;

public sealed record NoteResponse(
    Guid Id,
    string Title,
    string Content,
    string? Summary,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);