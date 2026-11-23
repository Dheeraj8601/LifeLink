using LifeLink.Application.DTOs.Admin;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace LifeLink.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly LifeLinkDbContext _context;

    public AdminRepository(LifeLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PendingDonorResponse>> GetPendingDonorsAsync(
        CancellationToken cancellationToken = default)
    {
        var pendingDonors = await _context.Database
            .SqlQueryRaw<PendingDonorResponse>(
                "EXEC dbo.usp_Admin_GetPendingDonors")
            .ToListAsync(cancellationToken);

        return pendingDonors;
    }

    public async Task<bool> VerifyDonorAsync(
        int donorId,
        CancellationToken cancellationToken = default)
    {
        var parameter = new SqlParameter("@DonorId", donorId);

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.usp_Admin_VerifyDonor @DonorId",
            new[] { parameter },
            cancellationToken);

        return true;
    }

    public async Task<bool> RejectDonorAsync(
        int donorId,
        string rejectionReason,
        CancellationToken cancellationToken = default)
    {
        var donorIdParameter = new SqlParameter("@DonorId", donorId);

        var reasonParameter = new SqlParameter(
            "@RejectionReason",
            rejectionReason);

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.usp_Admin_RejectDonor @DonorId, @RejectionReason",
            new[] { donorIdParameter, reasonParameter },
            cancellationToken);

        return true;
    }
}