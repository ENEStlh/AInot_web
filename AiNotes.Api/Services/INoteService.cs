using AiNotes.Api.Dtos.Notes;

namespace AiNotes.Api.Services;

public interface INoteService
{
    Task<NoteResponse> CreateAsync(
        Guid userId,
        CreateNoteRequest request,
        CancellationToken cancellationToken);

    Task<NoteResponse?> GetByIdAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken);

    Task<NoteListResponse> GetAllAsync(
        Guid userId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<NoteResponse?> UpdateAsync(
        Guid userId,
        Guid noteId,
        UpdateNoteRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken);

    Task<NoteResponse?> UpdateSummaryAsync(
        Guid userId,
        Guid noteId,
        string summary,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>?> AddTagsAsync(
        Guid userId,
        Guid noteId,
        IReadOnlyList<string> tagNames,
        CancellationToken cancellationToken);
}