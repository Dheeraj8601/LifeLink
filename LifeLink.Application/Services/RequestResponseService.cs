
using LifeLink.Application.DTOs.RequestResponses;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Services;
using LifeLink.Domain.Entities;
using LifeLink.Domain.Enums;
using LifeLink.Domain.Services;

namespace LifeLink.Application.Services;

public class RequestResponseService : IRequestResponseService
{
    private readonly IRequestResponseRepository _responseRepository;
    private readonly IBloodRequestRepository _bloodRequestRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public RequestResponseService(
        IRequestResponseRepository responseRepository,
        IBloodRequestRepository bloodRequestRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _responseRepository = responseRepository;
        _bloodRequestRepository = bloodRequestRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<int> SubmitResponseAsync(
        int bloodRequestId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (bloodRequestId <= 0)
            throw new ArgumentException("Invalid blood request ID.");

        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        int responseId = 0;
        int requesterId = 0;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            var bloodRequest =
                await _bloodRequestRepository.GetByIdForUpdateAsync(
                    bloodRequestId,
                    transactionToken);

            if (bloodRequest is null)
                throw new KeyNotFoundException("Blood request not found.");

            requesterId = bloodRequest.RequesterId;

            if (bloodRequest.Status != BloodRequestStatus.Open &&
                bloodRequest.Status != BloodRequestStatus.PartiallyFulfilled)
            {
                throw new InvalidOperationException(
                    "This blood request is no longer accepting responses.");
            }

            var donorProfileId =
                await _responseRepository.GetEligibleDonorProfileIdAsync(
                    userId,
                    transactionToken);

            if (donorProfileId is null)
            {
                throw new InvalidOperationException(
                    "An active and verified donor profile is required.");
            }

            var donorBloodGroup =
                await _responseRepository.GetEligibleDonorBloodGroupAsync(
                    userId,
                    transactionToken);

            if (donorBloodGroup is null)
            {
                throw new InvalidOperationException(
                    "Unable to retrieve the donor's blood group.");
            }

            if (!BloodGroupCompatibility.IsCompatible(
                    donorBloodGroup,
                    bloodRequest.BloodGroup))
            {
                throw new InvalidOperationException(
                    $"Your blood group ({donorBloodGroup}) is not compatible " +
                    $"with the requested blood group ({bloodRequest.BloodGroup}).");
            }

            var alreadyResponded =
                await _responseRepository.ExistsAsync(
                    bloodRequestId,
                    donorProfileId.Value,
                    transactionToken);

            if (alreadyResponded)
            {
                throw new InvalidOperationException(
                    "You have already responded to this blood request.");
            }

            var response = new RequestResponse
            {
                BloodRequestId = bloodRequestId,
                DonorId = donorProfileId.Value
            };

            await _responseRepository.AddAsync(
                response,
                transactionToken);

            responseId = response.Id;

        }, cancellationToken);

        await _notificationService.NotifyUserAsync(
            requesterId,
            "NewBloodRequestResponse",
            new
            {
                BloodRequestId = bloodRequestId,
                ResponseId = responseId,
                DonorUserId = userId,
                Message = "A donor has responded to your blood request.",
                CreatedAt = DateTime.UtcNow
            },
            cancellationToken);

        return responseId;
    }

    public async Task<List<RequestResponse>> GetResponsesForRequesterAsync(
        int bloodRequestId,
        int requesterId,
        CancellationToken cancellationToken = default)
    {
        if (bloodRequestId <= 0)
            throw new ArgumentException("Invalid blood request ID.");

        if (requesterId <= 0)
            throw new ArgumentException("Invalid requester ID.");

        var bloodRequest =
            await _bloodRequestRepository.GetByIdAsync(
                bloodRequestId,
                cancellationToken);

        if (bloodRequest is null ||
            bloodRequest.RequesterId != requesterId)
        {
            throw new KeyNotFoundException("Blood request not found.");
        }

        return await _responseRepository.GetByBloodRequestIdAsync(
            bloodRequestId,
            cancellationToken);
    }

    public async Task UpdateResponseStatusAsync(
        int responseId,
        int requesterId,
        RequestResponseStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        if (responseId <= 0)
            throw new ArgumentException("Invalid response ID.");

        if (requesterId <= 0)
            throw new ArgumentException("Invalid requester ID.");

        if (newStatus != RequestResponseStatus.Accepted &&
            newStatus != RequestResponseStatus.Rejected)
        {
            throw new ArgumentException(
                "Only Accepted or Rejected status is allowed.");
        }

        var initialResponse =
            await _responseRepository.GetByIdAsync(
                responseId,
                cancellationToken);

        if (initialResponse is null)
            throw new KeyNotFoundException("Response not found.");

        var bloodRequestId = initialResponse.BloodRequestId;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            var bloodRequest =
                await _bloodRequestRepository.GetByIdForUpdateAsync(
                    bloodRequestId,
                    transactionToken);

            if (bloodRequest is null)
                throw new KeyNotFoundException("Blood request not found.");

            if (bloodRequest.RequesterId != requesterId)
            {
                throw new ArgumentException(
                    "You are not authorized to manage this request.");
            }

            if (bloodRequest.Status != BloodRequestStatus.Open &&
                bloodRequest.Status != BloodRequestStatus.PartiallyFulfilled)
            {
                throw new InvalidOperationException(
                    "This blood request is no longer accepting responses.");
            }

            var response =
                await _responseRepository.GetByIdAsync(
                    responseId,
                    transactionToken);

            if (response is null ||
                response.BloodRequestId != bloodRequestId)
            {
                throw new KeyNotFoundException("Response not found.");
            }

            if (newStatus == RequestResponseStatus.Accepted)
            {
                // Revalidate compatibility at acceptance time.
                var donorBloodGroup =
                    await _responseRepository.GetDonorBloodGroupByProfileIdAsync(
                        response.DonorId,
                        transactionToken);

                if (donorBloodGroup is null)
                {
                    throw new InvalidOperationException(
                        "The donor's blood group could not be found.");
                }

                if (!BloodGroupCompatibility.IsCompatible(
                        donorBloodGroup,
                        bloodRequest.BloodGroup))
                {
                    throw new InvalidOperationException(
                        $"This donor's blood group ({donorBloodGroup}) is not " +
                        $"compatible with the requested blood group " +
                        $"({bloodRequest.BloodGroup}).");
                }

                // Lock donor before checking active commitments.
                await _responseRepository.LockDonorForCommitmentAsync(
                    response.DonorId,
                    transactionToken);

                var hasOtherActiveCommitment =
                    await _responseRepository.HasOtherActiveCommitmentAsync(
                        response.DonorId,
                        responseId,
                        transactionToken);

                if (hasOtherActiveCommitment)
                {
                    throw new InvalidOperationException(
                        "This donor already has an active accepted commitment.");
                }
            }

            response.UpdateStatus(newStatus);

            await _responseRepository.UpdateAsync(
                response,
                transactionToken);

            var acceptedCount =
                await _responseRepository.GetAcceptedResponseCountAsync(
                    bloodRequestId,
                    transactionToken);

            var newRequestStatus = acceptedCount == 0
                ? BloodRequestStatus.Open
                : acceptedCount >= bloodRequest.UnitsRequired
                    ? BloodRequestStatus.Fulfilled
                    : BloodRequestStatus.PartiallyFulfilled;

            if (bloodRequest.Status != newRequestStatus)
            {
                bloodRequest.UpdateStatus(newRequestStatus);

                await _bloodRequestRepository.UpdateAsync(
                    bloodRequest,
                    transactionToken);
            }

        }, cancellationToken);
    }

    public async Task<List<DonorResponseResult>> GetResponsesForDonorAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        return await _responseRepository.GetResponsesByDonorIdAsync(
            userId,
            cancellationToken);
    }
}
