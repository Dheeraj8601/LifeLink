
using LifeLink.Application.Interfaces.Repositories;
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
    private readonly RequestResponseService _service;

    public RequestResponseServiceTests()
    {
        _responseRepositoryMock = new Mock<IRequestResponseRepository>();
        _bloodRequestRepositoryMock = new Mock<IBloodRequestRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // Execute the supplied operation directly in unit tests.
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
            _unitOfWorkMock.Object);
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

    private void SetupEligibleDonor(int userId = 10, int donorProfileId = 5)
    {
        _responseRepositoryMock
            .Setup(r => r.GetEligibleDonorProfileIdAsync(
                userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(donorProfileId);
    }

    [Fact]
    public async Task SubmitResponseAsync_ValidRequest_CreatesPendingResponse()
    {
        // Arrange
        _bloodRequestRepositoryMock
            .Setup(r => r.GetByIdAsync(
                101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOpenRequest());

        SetupEligibleDonor();

        _responseRepositoryMock
            .Setup(r => r.ExistsAsync(
                101, 5, It.IsAny<CancellationToken>()))
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
            r => r.GetByIdAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()),
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
        _bloodRequestRepositoryMock
            .Setup(r => r.GetByIdAsync(
                999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BloodRequest?)null);

        // Act
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.SubmitResponseAsync(999, 10));

        // Assert
        _responseRepositoryMock.Verify(
            r => r.GetEligibleDonorProfileIdAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(BloodRequestStatus.Fulfilled)]
    [InlineData(BloodRequestStatus.Cancelled)]
    [InlineData(BloodRequestStatus.PartiallyFulfilled)]
    public async Task SubmitResponseAsync_RequestNotOpen_ThrowsInvalidOperationException(
        BloodRequestStatus status)
    {
        // Arrange
        var request = CreateOpenRequest();
        request.Status = status;

        _bloodRequestRepositoryMock
            .Setup(r => r.GetByIdAsync(
                101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitResponseAsync(101, 10));

        // Assert
        _responseRepositoryMock.Verify(
            r => r.GetEligibleDonorProfileIdAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitResponseAsync_DonorProfileNotEligible_ThrowsInvalidOperationException()
    {
        // Arrange
        _bloodRequestRepositoryMock
            .Setup(r => r.GetByIdAsync(
                101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOpenRequest());

        _responseRepositoryMock
            .Setup(r => r.GetEligibleDonorProfileIdAsync(
                10, It.IsAny<CancellationToken>()))
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
    public async Task SubmitResponseAsync_DonorAlreadyResponded_ThrowsInvalidOperationException()
    {
        // Arrange
        _bloodRequestRepositoryMock
            .Setup(r => r.GetByIdAsync(
                101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOpenRequest());

        SetupEligibleDonor();

        _responseRepositoryMock
            .Setup(r => r.ExistsAsync(
                101, 5, It.IsAny<CancellationToken>()))
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
