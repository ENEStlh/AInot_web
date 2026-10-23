namespace AiNotes.Api.Models;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
}