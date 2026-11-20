
using LifeLink.Application.DTOs.BloodRequests;
using LifeLink.Application.DTOs.Common;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Services;
using LifeLink.Domain.Entities;
using LifeLink.Domain.Enums;
using Moq;

namespace LifeLink.Tests.Services;

public class BloodRequestServiceTests
{
    private readonly Mock<IBloodRequestRepository> _repositoryMock;
    private readonly BloodRequestService _service;

    public BloodRequestServiceTests()
    {
        _repositoryMock = new Mock<IBloodRequestRepository>();

        // Default: requester is valid and active.
        _repositoryMock
            .Setup(r => r.IsActiveRequesterAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _service = new BloodRequestService(
            _repositoryMock.Object);
    }

    // Test 1: Valid blood request
    [Fact]
    public async Task CreateBloodRequestAsync_WithValidCommand_ShouldCreateRequest()
    {
        // Arrange
        var command = CreateValidCommand();

        _repositoryMock
            .Setup(r => r.CityExistsAsync(
                command.CityId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<BloodRequest, CancellationToken>(
                (request, _) => request.Id = 101)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateBloodRequestAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(101, result.Id);
        Assert.Equal(command.RequesterId, result.RequesterId);
        Assert.Equal(command.CityId, result.CityId);
        Assert.Equal("O+", result.BloodGroup);
        Assert.Equal("KMC Hospital", result.HospitalName);
        Assert.Equal(2, result.UnitsRequired);
        Assert.Equal("Critical", result.Urgency);
        Assert.Equal("Open", result.Status);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Test 2: Invalid city
    [Fact]
    public async Task CreateBloodRequestAsync_WithInvalidCity_ShouldThrowArgumentException()
    {
        // Arrange
        var command = CreateValidCommand();

        _repositoryMock
            .Setup(r => r.CityExistsAsync(
                command.CityId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBloodRequestAsync(command));

        // Assert
        Assert.Contains("does not exist", exception.Message);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Test 3: Invalid blood group
    [Fact]
    public async Task CreateBloodRequestAsync_WithInvalidBloodGroup_ShouldThrowArgumentException()
    {
        // Arrange
        var command = CreateValidCommand();
        command.BloodGroup = "X+";

        SetupExistingCity(command.CityId);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBloodRequestAsync(command));

        // Assert
        Assert.Equal("Invalid blood group.", exception.Message);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Test 4: Zero units
    [Fact]
    public async Task CreateBloodRequestAsync_WithZeroUnits_ShouldThrowArgumentException()
    {
        // Arrange
        var command = CreateValidCommand();
        command.UnitsRequired = 0;

        SetupExistingCity(command.CityId);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBloodRequestAsync(command));

        // Assert
        Assert.Contains("greater than zero", exception.Message);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Test 5: Negative units
    [Fact]
    public async Task CreateBloodRequestAsync_WithNegativeUnits_ShouldThrowArgumentException()
    {
        // Arrange
        var command = CreateValidCommand();
        command.UnitsRequired = -2;

        SetupExistingCity(command.CityId);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBloodRequestAsync(command));

        // Assert
        Assert.Contains("greater than zero", exception.Message);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Test 6: Invalid urgency
    [Fact]
    public async Task CreateBloodRequestAsync_WithInvalidUrgency_ShouldThrowArgumentException()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Urgency = "SuperCritical";

        SetupExistingCity(command.CityId);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBloodRequestAsync(command));

        // Assert
        Assert.Equal("Invalid request urgency.", exception.Message);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Test 7: Existing blood request
    [Fact]
    public async Task GetBloodRequestByIdAsync_WhenRequestExists_ShouldReturnRequest()
    {
        // Arrange
        var bloodRequest = new BloodRequest
        {
            Id = 101,
            RequesterId = 10,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow,
            Description = "Emergency blood requirement"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                101,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bloodRequest);

        // Act
        var result = await _service.GetBloodRequestByIdAsync(101,10,"Admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(101, result.Id);
        Assert.Equal("O+", result.BloodGroup);
        Assert.Equal("Critical", result.Urgency);
        Assert.Equal("Open", result.Status);
        Assert.Equal("KMC Hospital", result.HospitalName);
    }

    // Test 8: Missing blood request
    [Fact]
    public async Task GetBloodRequestByIdAsync_WhenRequestDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BloodRequest?)null);

        // Act
        var result = await _service.GetBloodRequestByIdAsync(999,10,"Admin");

        // Assert
        Assert.Null(result);
    }

    // Helper method for creating a valid command
    private static CreateBloodRequestCommand CreateValidCommand()
    {
        return new CreateBloodRequestCommand
        {
            RequesterId = 10,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            Latitude = 12.8698m,
            Longitude = 74.8430m,
            UnitsRequired = 2,
            Urgency = "Critical",
            Description = "Emergency blood requirement"
        };
    }

    // Helper method for configuring an existing city
    private void SetupExistingCity(int cityId)
    {
        _repositoryMock
            .Setup(r => r.CityExistsAsync(
                cityId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }



    // Test 9: Invalid or inactive requester
    [Fact]
    public async Task CreateBloodRequestAsync_WithInvalidRequester_ShouldThrowArgumentException()
    {
        // Arrange
        var command = CreateValidCommand();

        _repositoryMock
            .Setup(r => r.IsActiveRequesterAsync(
                command.RequesterId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBloodRequestAsync(command));

        // Assert
        Assert.Contains("Invalid requester", exception.Message);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Test 10: Empty or whitespace hospital name
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateBloodRequestAsync_WithInvalidHospitalName_ShouldThrowArgumentException(
        string hospitalName)
    {
        // Arrange
        var command = CreateValidCommand();
        command.HospitalName = hospitalName;

        SetupExistingCity(command.CityId);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBloodRequestAsync(command));

        // Assert
        Assert.Equal("Hospital name is required.", exception.Message);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Test 11: Empty or whitespace hospital address
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateBloodRequestAsync_WithInvalidHospitalAddress_ShouldThrowArgumentException(
        string hospitalAddress)
    {
        // Arrange
        var command = CreateValidCommand();
        command.HospitalAddress = hospitalAddress;

        SetupExistingCity(command.CityId);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBloodRequestAsync(command));

        // Assert
        Assert.Equal("Hospital address is required.", exception.Message);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<BloodRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }



    [Fact]
    public async Task GetBloodRequestByIdAsync_RequesterViewsOwnRequest_ReturnsRequest()
    {
        // Arrange
        var bloodRequest = new BloodRequest
        {
            Id = 101,
            RequesterId = 10,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bloodRequest);

        // Act
        var result = await _service.GetBloodRequestByIdAsync(
            101,
            10,
            "Requester");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(101, result.Id);
        Assert.Equal(10, result.RequesterId);
    }


    [Fact]
    public async Task GetBloodRequestByIdAsync_RequesterViewsOthersRequest_ReturnsNull()
    {
        // Arrange
        var bloodRequest = new BloodRequest
        {
            Id = 101,
            RequesterId = 20, // Request belongs to user 20
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bloodRequest);

        // Act
        var result = await _service.GetBloodRequestByIdAsync(
            101,
            10, // Logged-in user is 10
            "Requester");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBloodRequestByIdAsync_DonorViewsOpenRequest_ReturnsRequest()
    {
        // Arrange
        var bloodRequest = new BloodRequest
        {
            Id = 101,
            RequesterId = 20,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bloodRequest);

        // Act
        var result = await _service.GetBloodRequestByIdAsync(
            101,
            30,
            "Donor");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(101, result.Id);
        Assert.Equal("Open", result.Status);
    }

    [Fact]
    public async Task GetBloodRequestByIdAsync_DonorViewsFulfilledRequest_ReturnsNull()
    {
        // Arrange
        var bloodRequest = new BloodRequest
        {
            Id = 101,
            RequesterId = 20,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Fulfilled,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bloodRequest);

        // Act
        var result = await _service.GetBloodRequestByIdAsync(
            101,
            30,
            "Donor");

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task GetBloodRequestByIdAsync_AdminViewsAnyRequest_ReturnsRequest()
    {
        // Arrange
        var bloodRequest = new BloodRequest
        {
            Id = 101,
            RequesterId = 20,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Fulfilled,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bloodRequest);

        // Act
        var result = await _service.GetBloodRequestByIdAsync(
            101,
            99,
            "Admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(101, result.Id);
        Assert.Equal(20, result.RequesterId);
        Assert.Equal("Fulfilled", result.Status);
    }

    [Fact]
    public async Task GetBloodRequestByIdAsync_UnknownRole_ReturnsNull()
    {
        // Arrange
        var bloodRequest = new BloodRequest
        {
            Id = 101,
            RequesterId = 20,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bloodRequest);

        // Act
        var result = await _service.GetBloodRequestByIdAsync(
            101,
            99,
            "UnknownRole");

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task GetMyBloodRequestsAsync_ReturnsRequesterRequests()
    {
        // Arrange
        var requests = new List<BloodRequest>
    {
        new()
        {
            Id = 101,
            RequesterId = 10,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        },
        new()
        {
            Id = 102,
            RequesterId = 10,
            CityId = 1,
            BloodGroup = "A+",
            HospitalName = "City Hospital",
            HospitalAddress = "Udupi",
            UnitsRequired = 1,
            Urgency = RequestUrgency.Normal,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        }
    };

        _repositoryMock
            .Setup(r => r.GetByRequesterIdAsync(
                10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetMyBloodRequestsAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, request => Assert.Equal(10, request.RequesterId));
        Assert.Equal(101, result[0].Id);
        Assert.Equal(102, result[1].Id);
    }

    [Fact]
    public async Task GetMyBloodRequestsAsync_NoRequests_ReturnsEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByRequesterIdAsync(
                10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BloodRequest>());

        // Act
        var result = await _service.GetMyBloodRequestsAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOpenRequestsAsync_ValidPagination_ReturnsPagedResult()
    {
        // Arrange
        var requests = new List<BloodRequest>
    {
        new()
        {
            Id = 102,
            RequesterId = 20,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        },
        new()
        {
            Id = 101,
            RequesterId = 21,
            CityId = 1,
            BloodGroup = "A+",
            HospitalName = "City Hospital",
            HospitalAddress = "Udupi",
            UnitsRequired = 1,
            Urgency = RequestUrgency.Normal,
            Status = BloodRequestStatus.Open,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        }
    };

        _repositoryMock
            .Setup(r => r.GetOpenRequestsAsync(
                2,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((requests, 21));

        var pagination = new PaginationRequest
        {
            PageNumber = 2,
            PageSize = 10
        };

        // Act
        var result = await _service.GetOpenRequestsAsync(pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(21, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetOpenRequestsAsync_InvalidPageNumber_ThrowsArgumentException()
    {
        // Arrange
        var pagination = new PaginationRequest
        {
            PageNumber = 0,
            PageSize = 10
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetOpenRequestsAsync(pagination));

        // Assert
        Assert.Equal(
            "Page number must be greater than zero.",
            exception.Message);

        _repositoryMock.Verify(
            r => r.GetOpenRequestsAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task GetOpenRequestsAsync_InvalidPageSize_ThrowsArgumentException(
        int pageSize)
    {
        // Arrange
        var pagination = new PaginationRequest
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetOpenRequestsAsync(pagination));

        // Assert
        Assert.Equal(
            "Page size must be between 1 and 50.",
            exception.Message);

        _repositoryMock.Verify(
            r => r.GetOpenRequestsAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


}
