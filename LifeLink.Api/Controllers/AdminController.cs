using LifeLink.Application.DTOs.Admin;
using LifeLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LifeLink.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("donors/pending")]
    public async Task<IActionResult> GetPendingDonors(
        CancellationToken cancellationToken)
    {
        var donors = await _adminService.GetPendingDonorsAsync(cancellationToken);

        return Ok(donors);
    }

    [HttpPut("donors/{donorId:int}/verify")]
    public async Task<IActionResult> VerifyDonor(
        int donorId,
        CancellationToken cancellationToken)
    {
        if (donorId <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid donor ID."
            });
        }

        try
        {
            await _adminService.VerifyDonorAsync(
                donorId,
                cancellationToken);

            return Ok(new
            {
                message = "Donor verified successfully.",
                donorId
            });
        }
        catch (SqlException ex) when (ex.Number == 50001)
        {
            return Conflict(new
            {
                message = "Donor does not exist or is not pending verification."
            });
        }
    }

    [HttpPut("donors/{donorId:int}/reject")]
    public async Task<IActionResult> RejectDonor(
        int donorId,
        [FromBody] RejectDonorRequest request,
        CancellationToken cancellationToken)
    {
        if (donorId <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid donor ID."
            });
        }

        try
        {
            await _adminService.RejectDonorAsync(
                donorId,
                request.RejectionReason,
                cancellationToken);

            return Ok(new
            {
                message = "Donor rejected successfully.",
                donorId
            });
        }
        catch (SqlException ex) when (ex.Number == 50002)
        {
            return BadRequest(new
            {
                message = "Rejection reason is required."
            });
        }
        catch (SqlException ex) when (ex.Number == 50003)
        {
            return Conflict(new
            {
                message = "Donor does not exist or is not pending verification."
            });
        }
    }
}