
using LifeLink.Api.DTOs.Auth;
using LifeLink.Application.DTOs.Auth;
using LifeLink.Application.Interfaces.Security;
using LifeLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LifeLink.Api.DTOs.Requesters;
using LifeLink.Application.Commands.Requesters;
using LifeLink.Application.Exceptions;

namespace LifeLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IRequesterService _requesterService;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        IConfiguration configuration,
        IRequesterService requesterService)
    {
        _authService = authService;
        _tokenService = tokenService;
        _configuration = configuration;
        _requesterService = requesterService;
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

    [HttpPost("register/requester")]
    public async Task<IActionResult> RegisterRequester(
    [FromBody] RegisterRequesterRequest request,
    CancellationToken cancellationToken)
    {
        var command = new RegisterRequesterCommand
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password,
            PhoneNumber = request.PhoneNumber,
            CityId = request.CityId
        };

        try
        {
            var userId = await _requesterService.RegisterRequesterAsync(
                command,
                cancellationToken);

            return StatusCode(201, new
            {
                message = "Requester registration successful.",
                userId
            });
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
        catch (CityNotFoundException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}
