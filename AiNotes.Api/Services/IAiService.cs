namespace AiNotes.Api.Services;

public interface IAiService
{
    Task<string> Summarize(string content, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> SuggestTags(string content, CancellationToken cancellationToken);
}