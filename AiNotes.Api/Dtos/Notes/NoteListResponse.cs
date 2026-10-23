namespace AiNotes.Api.Dtos.Notes;

public sealed record NoteListResponse(
    IReadOnlyList<NoteResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);