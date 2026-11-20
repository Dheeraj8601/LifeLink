
using LifeLink.Api.DTOs.Donors;
using LifeLink.Application.DTOs.Donors;
using LifeLink.Application.Exceptions;
using LifeLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LifeLink.Api.Controllers;

[ApiController]
[Route("api/donors")]
public class DonorController : ControllerBase
{
    private readonly IDonorService _donorService;

    public DonorController(IDonorService donorService)
    {
        _donorService = donorService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDonorById(
        int id,
        CancellationToken cancellationToken)
    {
        var donor = await _donorService.GetDonorByIdAsync(
            id,
            cancellationToken);

        if (donor is null)
        {
            return NotFound(new
            {
                Message = $"Donor with ID {id} was not found."
            });
        }

        return Ok(donor);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterDonor(
        [FromBody] RegisterDonorRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterDonorCommand
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password,
            PhoneNumber = request.PhoneNumber,
            CityId = request.CityId,
            BloodGroup = request.BloodGroup,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        try
        {
            var donor = await _donorService.RegisterDonorAsync(
                command,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetDonorById),
                new { id = donor.Id },
                new
                {
                    Message = "Donor registered successfully.",
                    DonorId = donor.Id,
                    UserId = donor.UserId
                });
        }
        catch (DuplicateEmailException)
        {
            return Conflict(new
            {
                Message = "This email is already registered."
            });
        }
        catch (CityNotFoundException ex)
        {
            return BadRequest(new
            {
                Message = ex.Message
            });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyDonorProfile(
    CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var donor = await _donorService.GetDonorByUserIdAsync(
            userId,
            cancellationToken);

        if (donor is null)
        {
            return NotFound(new
            {
                Message = "Donor profile was not found."
            });
        }

        return Ok(new
        {
            donor.Id,
            donor.BloodGroup,
            CityName = donor.CityName,
            CityState = donor.CityState,
            donor.IsAvailable,
            donor.IsVerified,
            donor.LastDonationDate,
            donor.Latitude,
            donor.Longitude
        });
    }

    [Authorize]
    [HttpPatch("me/availability")]
    public async Task<IActionResult> UpdateMyAvailability(
        [FromBody] UpdateDonorAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        await _donorService.UpdateAvailabilityAsync(
            userId,
            request.IsAvailable,
            cancellationToken);

        return Ok(new
        {
            Message = "Availability updated successfully.",
            IsAvailable = request.IsAvailable
        });
    }
}
