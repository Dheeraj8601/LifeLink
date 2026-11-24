
using LifeLink.Application.DTOs.BloodRequests;
using LifeLink.Application.DTOs.Common;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Services;
using LifeLink.Domain.Entities;
using LifeLink.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LifeLink.Application.Services;

public class BloodRequestService : IBloodRequestService
{
    private readonly IBloodRequestRepository _repository;
    private readonly IBloodRequestNotificationScheduler _notificationScheduler;
    private readonly ILogger<BloodRequestService> _logger;

    private static readonly HashSet<string> ValidBloodGroups =
    [
        "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"
    ];

    public BloodRequestService(
        IBloodRequestRepository repository,
        IBloodRequestNotificationScheduler notificationScheduler,
        ILogger<BloodRequestService> logger)
    {
        _repository = repository;
        _notificationScheduler = notificationScheduler;
        _logger = logger;
    }

    public async Task<BloodRequestResult> CreateBloodRequestAsync(
        CreateBloodRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.RequesterId <= 0 ||
            !await _repository.IsActiveRequesterAsync(
                command.RequesterId,
                cancellationToken))
        {
            throw new ArgumentException(
                "Invalid requester. An active requester account is required.");
        }

        if (!await _repository.CityExistsAsync(
                command.CityId,
                cancellationToken))
        {
            throw new ArgumentException(
                $"City with ID {command.CityId} does not exist.");
        }

        if (string.IsNullOrWhiteSpace(command.BloodGroup) ||
            !ValidBloodGroups.Contains(
                command.BloodGroup.Trim().ToUpperInvariant()))
        {
            throw new ArgumentException("Invalid blood group.");
        }

        if (string.IsNullOrWhiteSpace(command.HospitalName))
        {
            throw new ArgumentException("Hospital name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.HospitalAddress))
        {
            throw new ArgumentException("Hospital address is required.");
        }

        if (command.Latitude.HasValue != command.Longitude.HasValue)
        {
            throw new ArgumentException(
                "Both latitude and longitude must be provided together.");
        }

        if (command.Latitude.HasValue &&
            (command.Latitude < -90 || command.Latitude > 90))
        {
            throw new ArgumentException(
                "Latitude must be between -90 and 90 degrees.");
        }

        if (command.Longitude.HasValue &&
            (command.Longitude < -180 || command.Longitude > 180))
        {
            throw new ArgumentException(
                "Longitude must be between -180 and 180 degrees.");
        }

        if (command.UnitsRequired <= 0)
        {
            throw new ArgumentException(
                "Units required must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(command.Urgency) ||
            !Enum.TryParse<RequestUrgency>(
                command.Urgency.Trim(),
                true,
                out var urgency) ||
            !Enum.IsDefined(urgency))
        {
            throw new ArgumentException("Invalid request urgency.");
        }

        var bloodRequest = new BloodRequest
        {
            RequesterId = command.RequesterId,
            CityId = command.CityId,
            BloodGroup = command.BloodGroup.Trim().ToUpperInvariant(),
            HospitalName = command.HospitalName.Trim(),
            HospitalAddress = command.HospitalAddress.Trim(),
            Latitude = command.Latitude,
            Longitude = command.Longitude,
            UnitsRequired = command.UnitsRequired,
            Urgency = urgency,
            Status = BloodRequestStatus.Open,
            Description = command.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // 1. Save the blood request first.
        await _repository.AddAsync(
            bloodRequest,
            cancellationToken);

        // 2. Enqueue donor matching as a background job.
        // A scheduling failure must not make the requester believe
        // that the already-saved blood request was not created.
        if (bloodRequest.Latitude.HasValue &&
            bloodRequest.Longitude.HasValue)
        {
            try
            {
                var jobId =
                    _notificationScheduler.EnqueueDonorNotification(
                        bloodRequest.Id);

                _logger.LogInformation(
                    "Donor notification job {JobId} enqueued for " +
                    "blood request {BloodRequestId}.",
                    jobId,
                    bloodRequest.Id);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to enqueue donor notification job for " +
                    "blood request {BloodRequestId}.",
                    bloodRequest.Id);
            }
        }
        else
        {
            _logger.LogWarning(
                "Blood request {BloodRequestId} has no coordinates. " +
                "Donor notification job was not enqueued.",
                bloodRequest.Id);
        }

        return MapToResult(bloodRequest);
    }

    public async Task<BloodRequestResult?> GetBloodRequestByIdAsync(
        int id,
        int currentUserId,
        string currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var bloodRequest = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (bloodRequest is null)
        {
            return null;
        }

        bool hasAccess = currentUserRole switch
        {
            "Requester" => bloodRequest.RequesterId == currentUserId,

            "Donor" => bloodRequest.Status == BloodRequestStatus.Open,

            "Admin" => true,

            _ => false
        };

        if (!hasAccess)
        {
            return null;
        }

        return MapToResult(bloodRequest);
    }

    private static BloodRequestResult MapToResult(
        BloodRequest bloodRequest)
    {
        return new BloodRequestResult
        {
            Id = bloodRequest.Id,
            RequesterId = bloodRequest.RequesterId,
            CityId = bloodRequest.CityId,
            BloodGroup = bloodRequest.BloodGroup,
            HospitalName = bloodRequest.HospitalName,
            HospitalAddress = bloodRequest.HospitalAddress,
            Latitude = bloodRequest.Latitude,
            Longitude = bloodRequest.Longitude,
            UnitsRequired = bloodRequest.UnitsRequired,
            Urgency = bloodRequest.Urgency.ToString(),
            Status = bloodRequest.Status.ToString(),
            CreatedAt = bloodRequest.CreatedAt,
            Description = bloodRequest.Description
        };
    }

    public async Task<List<BloodRequestResult>> GetMyBloodRequestsAsync(
        int requesterId,
        CancellationToken cancellationToken = default)
    {
        if (requesterId <= 0)
        {
            throw new ArgumentException("Invalid requester ID.");
        }

        var requests = await _repository.GetByRequesterIdAsync(
            requesterId,
            cancellationToken);

        return requests
            .Select(MapToResult)
            .ToList();
    }

    public async Task<PagedResult<BloodRequestResult>> GetOpenRequestsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        if (pagination.PageNumber < 1)
        {
            throw new ArgumentException(
                "Page number must be greater than zero.");
        }

        if (pagination.PageSize < 1 || pagination.PageSize > 50)
        {
            throw new ArgumentException(
                "Page size must be between 1 and 50.");
        }

        var (requests, totalCount) =
            await _repository.GetOpenRequestsAsync(
                pagination.PageNumber,
                pagination.PageSize,
                cancellationToken);

        return new PagedResult<BloodRequestResult>
        {
            Items = requests.Select(MapToResult).ToList(),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }
}
