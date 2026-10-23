using AiNotes.Api.Data;
using AiNotes.Api.Dtos.Notes;
using AiNotes.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiNotes.Api.Services;

public sealed class NoteService(AppDbContext dbContext) : INoteService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public async Task<NoteResponse> CreateAsync(
        Guid userId,
        CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Notes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(note);
    }

    public async Task<NoteResponse?> GetByIdAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken)
    {
        var note = await dbContext.Notes
            .Include(candidate => candidate.NoteTags)
                .ThenInclude(noteTag => noteTag.Tag)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == noteId && candidate.UserId == userId,
                cancellationToken);

        return note is null ? null : Map(note);
    }

    public async Task<NoteListResponse> GetAllAsync(
        Guid userId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var query = dbContext.Notes
            .AsNoTracking()
            .Include(note => note.NoteTags)
                .ThenInclude(noteTag => noteTag.Tag)
            .Where(note => note.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(note =>
                note.Title.Contains(searchTerm) || note.Content.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var notes = await query
            .OrderByDescending(note => note.UpdatedAt)
            .ThenByDescending(note => note.CreatedAt)
            .Skip((int)skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new NoteListResponse(
            notes.Select(Map).ToList(),
            page,
            pageSize,
            totalCount,
            totalPages);
    }

    public async Task<NoteResponse?> UpdateAsync(
        Guid userId,
        Guid noteId,
        UpdateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var note = await dbContext.Notes
            .Include(candidate => candidate.NoteTags)
                .ThenInclude(noteTag => noteTag.Tag)
            .SingleOrDefaultAsync(
                candidate => candidate.Id == noteId && candidate.UserId == userId,
                cancellationToken);

        if (note is null)
        {
            return null;
        }

        note.Title = request.Title.Trim();
        note.Content = request.Content.Trim();
        note.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(note);
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken)
    {
        var note = await dbContext.Notes
            .Include(candidate => candidate.NoteTags)
                .ThenInclude(noteTag => noteTag.Tag)
            .SingleOrDefaultAsync(
                candidate => candidate.Id == noteId && candidate.UserId == userId,
                cancellationToken);

        if (note is null)
        {
            return false;
        }

        dbContext.Notes.Remove(note);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<NoteResponse?> UpdateSummaryAsync(
        Guid userId,
        Guid noteId,
        string summary,
        CancellationToken cancellationToken)
    {
        var note = await dbContext.Notes
            .Include(candidate => candidate.NoteTags)
                .ThenInclude(noteTag => noteTag.Tag)
            .SingleOrDefaultAsync(
                candidate => candidate.Id == noteId && candidate.UserId == userId,
                cancellationToken);

        if (note is null)
        {
            return null;
        }

        note.Summary = summary.Trim();
        note.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(note);
    }

    public async Task<IReadOnlyList<string>?> AddTagsAsync(
        Guid userId,
        Guid noteId,
        IReadOnlyList<string> tagNames,
        CancellationToken cancellationToken)
    {
        var note = await dbContext.Notes
            .Include(candidate => candidate.NoteTags)
            .SingleOrDefaultAsync(
                candidate => candidate.Id == noteId && candidate.UserId == userId,
                cancellationToken);

        if (note is null)
        {
            return null;
        }

        var normalizedNames = tagNames
            .Select(tagName => tagName.Trim().ToLowerInvariant())
            .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        if (normalizedNames.Count == 0)
        {
            return Array.Empty<string>();
        }

        var existingTags = await dbContext.Tags
            .Where(tag => normalizedNames.Contains(tag.Name))
            .ToListAsync(cancellationToken);
        var existingNames = existingTags
            .Select(tag => tag.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newTags = normalizedNames
            .Where(tagName => !existingNames.Contains(tagName))
            .Select(tagName => new Tag { Id = Guid.NewGuid(), Name = tagName })
            .ToList();

        dbContext.Tags.AddRange(newTags);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var tag in existingTags.Concat(newTags))
        {
            if (note.NoteTags.All(noteTag => noteTag.TagId != tag.Id))
            {
                note.NoteTags.Add(new NoteTag { NoteId = note.Id, TagId = tag.Id });
            }
        }

        note.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return normalizedNames;
    }

    private static NoteResponse Map(Note note) => new(
        note.Id,
        note.Title,
        note.Content,
        note.Summary,
        note.NoteTags
            .Where(noteTag => noteTag.Tag is not null)
            .Select(noteTag => noteTag.Tag.Name)
            .OrderBy(tagName => tagName)
            .ToList(),
        ToUtcOffset(note.CreatedAt),
        ToUtcOffset(note.UpdatedAt));

    private static DateTimeOffset ToUtcOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}