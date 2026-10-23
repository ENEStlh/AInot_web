using AiNotes.Api.Dtos.Auth;
using AiNotes.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiNotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var registered = await authService.RegisterAsync(request, cancellationToken);

        return registered
            ? NoContent()
            : Conflict(new ProblemDetails
            {
                Title = "Email already registered",
                Detail = "Bu email adresi zaten kayıtlı."
            });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);

        return response is null
            ? Unauthorized(new ProblemDetails
            {
                Title = "Invalid credentials",
                Detail = "Email veya şifre hatalı."
            })
            : Ok(response);
    }
}