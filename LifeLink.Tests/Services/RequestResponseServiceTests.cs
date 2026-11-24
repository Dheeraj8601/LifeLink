
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Services;
using LifeLink.Application.Services;
using LifeLink.Domain.Entities;
using LifeLink.Domain.Enums;
using Moq;

namespace LifeLink.Tests.Services;

public class RequestResponseServiceTests
{
    private readonly Mock<IRequestResponseRepository> _responseRepositoryMock;
    private readonly Mock<IBloodRequestRepository> _bloodRequestRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<INotificationService> _notification;
    private readonly RequestResponseService _service;

    public RequestResponseServiceTests()
    {
        _responseRepositoryMock = new Mock<IRequestResponseRepository>();
        _bloodRequestRepositoryMock = new Mock<IBloodRequestRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notification = new Mock<INotificationService>();

        // Execute the supplied transaction operation directly in unit tests.
        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(
                async (operation, cancellationToken) =>
                    await operation(cancellationToken));

        _service = new RequestResponseService(
            _responseRepositoryMock.Object,
            _bloodRequestRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _notification.Object);
    }

    private static BloodRequest CreateOpenRequest()
    {
        return new BloodRequest
        {
            Id = 101,
            RequesterId = 20,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void SetupBloodRequest(
        BloodRequest? request,
        int requestId = 101)
    {
        _bloodRequestRepositoryMock
            .Setup(r => r.GetByIdForUpdateAsync(
                requestId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
    }

    private void SetupEligibleDonor(
        int userId = 10,
        int donorProfileId = 5,
        string bloodGroup = "O+")
    {
        _responseRepositoryMock
            .Setup(r => r.GetEligibleDonorProfileIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(donorProfileId);

        _responseRepositoryMock
            .Setup(r => r.GetEligibleDonorBloodGroupAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bloodGroup);
    }

    [Fact]
    public async Task SubmitResponseAsync_ValidRequest_CreatesPendingResponse()
    {
        // Arrange
        SetupBloodRequest(CreateOpenRequest());

        SetupEligibleDonor();

        _responseRepositoryMock
            .Setup(r => r.ExistsAsync(
                101,
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _responseRepositoryMock
            .Setup(r => r.AddAsync(
                It.IsAny<RequestResponse>(),
                It.IsAny<CancellationToken>()))
            .Callback<RequestResponse, CancellationToken>((response, _) =>
                response.Id = 201)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SubmitResponseAsync(101, 10);

        // Assert
        Assert.Equal(201, result);

        _responseRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<RequestResponse>(response =>
                    response.BloodRequestId == 101 &&
                    response.DonorId == 5 &&
                    response.Status == RequestResponseStatus.Pending),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _notification.Verify(
            n => n.NotifyUserAsync(
                20,
                "NewBloodRequestResponse",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(101, 0)]
    [InlineData(101, -1)]
    public async Task SubmitResponseAsync_InvalidIds_ThrowsArgumentException(
        int bloodRequestId,
        int userId)
    {
        // Act
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SubmitResponseAsync(bloodRequestId, userId));

        // Assert
        _bloodRequestRepositoryMock.Verify(
            r => r.GetByIdForUpdateAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _responseRepositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<RequestResponse>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitResponseAsync_RequestDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        SetupBloodRequest(null, 999);

        // Act
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.SubmitResponseAsync(999, 10));

        // Assert
        _responseRepositoryMock.Verify(
            r => r.GetEligibleDonorProfileIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(BloodRequestStatus.Fulfilled)]
    [InlineData(BloodRequestStatus.Cancelled)]
    public async Task SubmitResponseAsync_RequestNotAcceptingResponses_ThrowsInvalidOperationException(
        BloodRequestStatus status)
    {
        // Arrange
        var request = CreateOpenRequest();
        request.Status = status;

        SetupBloodRequest(request);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitResponseAsync(101, 10));

        // Assert
        _responseRepositoryMock.Verify(
            r => r.GetEligibleDonorProfileIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitResponseAsync_PartiallyFulfilledRequest_AllowsResponse()
    {
        // Arrange
        var request = CreateOpenRequest();
        request.Status = BloodRequestStatus.PartiallyFulfilled;

        SetupBloodRequest(request);
        SetupEligibleDonor();

        _responseRepositoryMock
            .Setup(r => r.ExistsAsync(
                101,
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _responseRepositoryMock
            .Setup(r => r.AddAsync(
                It.IsAny<RequestResponse>(),
                It.IsAny<CancellationToken>()))
            .Callback<RequestResponse, CancellationToken>((response, _) =>
                response.Id = 202)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SubmitResponseAsync(101, 10);

        // Assert
        Assert.Equal(202, result);

        _responseRepositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<RequestResponse>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SubmitResponseAsync_DonorProfileNotEligible_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupBloodRequest(CreateOpenRequest());

        _responseRepositoryMock
            .Setup(r => r.GetEligibleDonorProfileIdAsync(
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitResponseAsync(101, 10));

        // Assert
        _responseRepositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<RequestResponse>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitResponseAsync_DonorBloodGroupIncompatible_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupBloodRequest(CreateOpenRequest());

        // B+ cannot donate red blood cells to an O+ recipient.
        SetupEligibleDonor(bloodGroup: "B+");

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitResponseAsync(101, 10));

        // Assert
        Assert.Contains("not compatible", exception.Message);

        _responseRepositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<RequestResponse>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitResponseAsync_DonorAlreadyResponded_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupBloodRequest(CreateOpenRequest());
        SetupEligibleDonor();

        _responseRepositoryMock
            .Setup(r => r.ExistsAsync(
                101,
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitResponseAsync(101, 10));

        // Assert
        _responseRepositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<RequestResponse>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
