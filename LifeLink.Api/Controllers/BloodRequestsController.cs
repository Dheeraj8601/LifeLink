
using Azure;
using LifeLink.Api.DTOs.BloodRequests;
using LifeLink.Api.DTOs.Donors;
using LifeLink.Api.DTOs.RequestResponses;
using LifeLink.Application.DTOs.BloodRequests;
using LifeLink.Application.DTOs.Common;
using LifeLink.Application.Interfaces.Services;
using LifeLink.Application.Services;
using LifeLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace LifeLink.Api.Controllers;

[ApiController]
[Route("api/blood-requests")]
public class BloodRequestsController : ControllerBase
{
    private readonly IBloodRequestService _bloodRequestService;
    private readonly IRequestResponseService _requestResponseService;
    private readonly IDonorService _donorService;

    public BloodRequestsController(
        IBloodRequestService bloodRequestService,
        IRequestResponseService requestResponseService,
        IDonorService donorService)
    {
        _bloodRequestService = bloodRequestService;
        _requestResponseService = requestResponseService;
        _donorService = donorService;
    }

    [HttpPost]
    [Authorize(Policy = "RequesterOnly")]
    public async Task<IActionResult> CreateBloodRequest(
        [FromBody] CreateBloodRequestRequest request,
        CancellationToken cancellationToken)
    {
        var requesterIdValue = User.FindFirstValue("sub")
                               ?? User.FindFirstValue(
                                   ClaimTypes.NameIdentifier);

        if (!int.TryParse(requesterIdValue, out var requesterId))
        {
            return Unauthorized();
        }

        var command = new CreateBloodRequestCommand
        {
            RequesterId = requesterId,
            CityId = request.CityId,
            BloodGroup = request.BloodGroup,
            HospitalName = request.HospitalName,
            HospitalAddress = request.HospitalAddress,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            UnitsRequired = request.UnitsRequired,
            Urgency = request.Urgency,
            Description = request.Description
        };

        try
        {
            var result = await _bloodRequestService
                .CreateBloodRequestAsync(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetBloodRequestById),
                new { id = result.Id },
                result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }


    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetBloodRequestById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid blood request ID."
            });
        }

        var userIdValue = User.FindFirstValue("sub")
                          ?? User.FindFirstValue(
                              ClaimTypes.NameIdentifier);

        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (!int.TryParse(userIdValue, out var currentUserId) ||
            string.IsNullOrWhiteSpace(userRole))
        {
            return Unauthorized();
        }

        var result = await _bloodRequestService.GetBloodRequestByIdAsync(
            id,
            currentUserId,
            userRole,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new
            {
                message = $"Blood request with ID {id} was not found."
            });
        }

        return Ok(result);
    }

    [HttpGet("my-requests")]
    [Authorize(Policy = "RequesterOnly")]
    public async Task<IActionResult> GetMyBloodRequests(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue("sub")
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var requesterId))
        {
            return Unauthorized();
        }

        var requests = await _bloodRequestService.GetMyBloodRequestsAsync(
            requesterId,
            cancellationToken);

        return Ok(requests);
    }

    [HttpGet("open")]
    [Authorize(Policy = "DonorOnly")]
    public async Task<IActionResult> GetOpenRequests(
    [FromQuery] PaginationRequest pagination,
    CancellationToken cancellationToken)
    {
        try
        {
            var result = await _bloodRequestService.GetOpenRequestsAsync(
                pagination,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }


    [HttpPost("{id:int}/responses")]
    [Authorize(Policy = "DonorOnly")]
    public async Task<IActionResult> SubmitResponse(
    int id,
    CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue("sub")
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var responseId = await _requestResponseService.SubmitResponseAsync(
                id,
                userId,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new
            {
                id = responseId,
                bloodRequestId = id,
                status = "Pending",
                message = "Your response has been submitted successfully."
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }


    [HttpGet("{id:int}/responses")]
    [Authorize(Policy = "RequesterOnly")]
    public async Task<IActionResult> GetResponses(int id,CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue("sub")
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var requesterId))
        {
            return Unauthorized();
        }

        try
        {
            var responses = await _requestResponseService
                .GetResponsesForRequesterAsync(
                    id,
                    requesterId,
                    cancellationToken);

            return Ok(responses);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }


    [HttpPut("responses/{responseId:int}/status")]
    [Authorize(Policy = "RequesterOnly")]
    public async Task<IActionResult> UpdateResponseStatus(
        int responseId,
        [FromBody] UpdateResponseStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue("sub")
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var requesterId))
        {
            return Unauthorized();
        }

        var newStatus = request.Status.Equals(
            "Accepted",
            StringComparison.OrdinalIgnoreCase)
                ? RequestResponseStatus.Accepted
                : RequestResponseStatus.Rejected;

        try
        {
            await _requestResponseService.UpdateResponseStatusAsync(
                responseId,
                requesterId,
                newStatus,
                cancellationToken);

            return Ok(new
            {
                responseId,
                status = newStatus.ToString(),
                message = "Donor response status updated successfully."
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }


    [HttpGet("{id:int}/nearby-donors")]
    [Authorize(Policy = "RequesterOnly")]
    public async Task<IActionResult> GetNearbyDonors(
        int id,
        [FromQuery] double radiusKm = 20,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid blood request ID."
            });
        }

        var userIdValue = User.FindFirstValue("sub")
                          ?? User.FindFirstValue(
                              ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var requesterId))
        {
            return Unauthorized();
        }

        try
        {
            // Retrieve the request and verify ownership.
            var bloodRequest = await _bloodRequestService
                .GetBloodRequestByIdAsync(
                    id,
                    requesterId,
                    "Requester",
                    cancellationToken);

            if (bloodRequest is null)
            {
                return NotFound(new
                {
                    message = "Blood request was not found."
                });
            }

            if (bloodRequest.Latitude is null ||
                bloodRequest.Longitude is null)
            {
                return BadRequest(new
                {
                    message = "Hospital coordinates are not available."
                });
            }

            var result = await _donorService.GetNearbyDonorsAsync(
                bloodRequest.BloodGroup,
                (double)bloodRequest.Latitude.Value,
                (double)bloodRequest.Longitude.Value,
                radiusKm,
                pageNumber,
                pageSize,
                cancellationToken);

            var items = result.Items.Select(donor => new NearbyDonorResponse
            {
                DonorId = donor.DonorId,
                CityId = donor.CityId,
                BloodGroup = donor.BloodGroup,
                DistanceKm = Math.Round(donor.DistanceKm, 2)
            }).ToList();

            var pagedResponse = new NearbyDonorsPagedResponse
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = result.TotalCount,
                TotalPages = (int)Math.Ceiling(
                result.TotalCount / (double)pageSize)
            };

            return Ok(pagedResponse);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

}
