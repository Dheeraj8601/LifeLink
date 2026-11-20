using LifeLink.Application.DTOs.BloodRequests;
using LifeLink.Application.DTOs.Common;

namespace LifeLink.Application.Interfaces.Services;

public interface IBloodRequestService
{
    Task<BloodRequestResult> CreateBloodRequestAsync(
        CreateBloodRequestCommand command,
        CancellationToken cancellationToken = default);

    Task<BloodRequestResult?> GetBloodRequestByIdAsync(
        int id,
        int currentUserId,
        string currentUserRole,
        CancellationToken cancellationToken = default);

    Task<List<BloodRequestResult>> GetMyBloodRequestsAsync(
        int requesterId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<BloodRequestResult>> GetOpenRequestsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);
}