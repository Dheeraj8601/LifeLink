
using LifeLink.Api.DTOs.Auth;
using LifeLink.Application.DTOs.Auth;
using LifeLink.Application.Interfaces.Security;
using LifeLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LifeLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService,ITokenService tokenService,IConfiguration configuration)
    {
        _authService = authService;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request,CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _authService.LoginAsync(
            command, cancellationToken);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var token = _tokenService.GenerateAccessToken(
            result.UserId,
            result.Email,
            result.Role);

        var expirationMinutes = _configuration.GetValue<int>(
            "JwtSettings:ExpirationMinutes");

        var cookieOptions = GetAuthCookieOptions();

        cookieOptions.Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);

        Response.Cookies.Append(
            "LifeLinkAuth",
            token,
            cookieOptions);

        return Ok(new
        {
            message = "Login successful.",
            user = new
            {
                result.UserId,
                result.FullName,
                result.Email,
                result.Role
            }
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(
                "LifeLinkAuth",
                GetAuthCookieOptions());

        return Ok(new
        {
            message = "Logout successful."
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub"),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    [Authorize(Policy = "DonorOnly")]
    [HttpGet("donor-access")]
    public IActionResult DonorAccess()
    {
        return Ok(new
        {
            message = "Donor access granted."
        });
    }

    private CookieOptions GetAuthCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };
    }
}
