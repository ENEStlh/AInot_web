using AiNotes.Api.Dtos.Notes;
using AiNotes.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiNotes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class NotesController(
    INoteService noteService,
    IAiService aiService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<NoteResponse>> Create(
        CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var response = await noteService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NoteResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var response = await noteService.GetByIdAsync(userId, id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(NoteListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NoteListResponse>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var response = await noteService.GetAllAsync(
            userId,
            search,
            page,
            pageSize,
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NoteResponse>> Update(
        Guid id,
        UpdateNoteRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var response = await noteService.UpdateAsync(
            userId,
            id,
            request,
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var deleted = await noteService.DeleteAsync(userId, id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/summarize")]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<NoteResponse>> Summarize(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var note = await noteService.GetByIdAsync(userId, id, cancellationToken);
        if (note is null)
        {
            return NotFound();
        }

        try
        {
            var summary = await aiService.Summarize(note.Content, cancellationToken);
            var response = await noteService.UpdateSummaryAsync(
                userId,
                id,
                summary,
                cancellationToken);

            return response is null ? NotFound() : Ok(response);
        }
        catch (AiServiceException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "AI summary service unavailable",
                Detail = exception.Message,
                Status = StatusCodes.Status502BadGateway
            });
        }
    }

    [HttpPost("{id:guid}/suggest-tags")]
    [ProducesResponseType(typeof(TagSuggestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<TagSuggestionResponse>> SuggestTags(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var note = await noteService.GetByIdAsync(userId, id, cancellationToken);
        if (note is null)
        {
            return NotFound();
        }

        try
        {
            var suggestedTags = await aiService.SuggestTags(note.Content, cancellationToken);
            var savedTags = await noteService.AddTagsAsync(
                userId,
                id,
                suggestedTags,
                cancellationToken);

            return savedTags is null
                ? NotFound()
                : Ok(new TagSuggestionResponse(savedTags));
        }
        catch (AiServiceException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "AI tag service unavailable",
                Detail = exception.Message,
                Status = StatusCodes.Status502BadGateway
            });
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out userId);
    }
}