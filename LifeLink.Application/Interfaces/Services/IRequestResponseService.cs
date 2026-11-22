using LifeLink.Application.DTOs.RequestResponses;
using LifeLink.Domain.Entities;
using LifeLink.Domain.Enums;

namespace LifeLink.Application.Interfaces.Services;

public interface IRequestResponseService
{
    Task<int> SubmitResponseAsync(
        int bloodRequestId,
        int donorId,
        CancellationToken cancellationToken = default);

    Task<List<RequestResponse>> GetResponsesForRequesterAsync(
        int bloodRequestId,
        int requesterId,
        CancellationToken cancellationToken = default);

    Task UpdateResponseStatusAsync(
        int responseId,
        int requesterId,
        RequestResponseStatus newStatus,
        CancellationToken cancellationToken = default);

    Task<List<DonorResponseResult>> GetResponsesForDonorAsync(
        int donorId,
        CancellationToken cancellationToken = default);
}