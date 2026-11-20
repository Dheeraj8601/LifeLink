
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Security;
using LifeLink.Application.Models.Donors;
using LifeLink.Application.Services;
using Moq;
using Xunit;

namespace LifeLink.Tests.Services;

public class DonorServiceNearbyMatchingTests
{
    private readonly Mock<IDonorRepository> _donorRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly DonorService _sut;

    public DonorServiceNearbyMatchingTests()
    {
        _donorRepositoryMock = new Mock<IDonorRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _sut = new DonorService(
            _donorRepositoryMock.Object,
            _passwordHasherMock.Object);
    }

    private static IReadOnlyList<DonorMatchResult> CreateDonors()
    {
        return new List<DonorMatchResult>
        {
            new()
            {
                DonorId = 1,
                UserId = 101,
                CityId = 1,
                BloodGroup = "O+",
                DistanceKm = 2.5,
                IsAvailable = true,
                IsVerified = true
            },
            new()
            {
                DonorId = 2,
                UserId = 102,
                CityId = 2,
                BloodGroup = "O+",
                DistanceKm = 5.75,
                IsAvailable = true,
                IsVerified = true
            }
        };
    }

    private static (
        IReadOnlyList<DonorMatchResult> Items,
        int TotalCount) CreateRepositoryResult()
    {
        return (CreateDonors(), 25);
    }

    [Fact]
    public async Task GetNearbyDonorsAsync_ValidRequest_CallsRepository()
    {
        var expectedResult = CreateRepositoryResult();

        _donorRepositoryMock
            .Setup(repository => repository.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                20,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _sut.GetNearbyDonorsAsync(
            "O+",
            13.3409,
            74.7421,
            20,
            1,
            10);

        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.Items.Count);

        _donorRepositoryMock.Verify(
            repository => repository.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                20,
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetNearbyDonorsAsync_LowercaseBloodGroup_NormalizesToUppercase()
    {
        _donorRepositoryMock
            .Setup(repository => repository.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                20,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRepositoryResult());

        await _sut.GetNearbyDonorsAsync(
            "o+",
            13.3409,
            74.7421,
            20,
            1,
            10);

        _donorRepositoryMock.Verify(
            repository => repository.GetNearbyDonorsAsync(
                "O+",
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetNearbyDonorsAsync_BloodGroupWithWhitespace_TrimsValue()
    {
        _donorRepositoryMock
            .Setup(repository => repository.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                20,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRepositoryResult());

        await _sut.GetNearbyDonorsAsync(
            " O+ ",
            13.3409,
            74.7421,
            20,
            1,
            10);

        _donorRepositoryMock.Verify(
            repository => repository.GetNearbyDonorsAsync(
                "O+",
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetNearbyDonorsAsync_RepositoryResult_ReturnsResult()
    {
        var expectedResult = CreateRepositoryResult();

        _donorRepositoryMock
            .Setup(repository => repository.GetNearbyDonorsAsync(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _sut.GetNearbyDonorsAsync(
            "O+",
            13.3409,
            74.7421,
            20,
            1,
            10);

        Assert.Same(expectedResult.Items, result.Items);
        Assert.Equal(expectedResult.TotalCount, result.TotalCount);
    }

    [Fact]
    public async Task GetNearbyDonorsAsync_CancellationToken_ForwardsToken()
    {
        using var cancellationSource = new CancellationTokenSource();

        var token = cancellationSource.Token;

        _donorRepositoryMock
            .Setup(repository => repository.GetNearbyDonorsAsync(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                token))
            .ReturnsAsync(CreateRepositoryResult());

        await _sut.GetNearbyDonorsAsync(
            "O+",
            13.3409,
            74.7421,
            20,
            1,
            10,
            token);

        _donorRepositoryMock.Verify(
            repository => repository.GetNearbyDonorsAsync(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                token),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetNearbyDonorsAsync_InvalidBloodGroup_ThrowsArgumentException(
        string? bloodGroup)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.GetNearbyDonorsAsync(
                bloodGroup!,
                13.3409,
                74.7421,
                20,
                1,
                10));

        _donorRepositoryMock.Verify(
            repository => repository.GetNearbyDonorsAsync(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(-90.0001)]
    [InlineData(90.0001)]
    [InlineData(91)]
    public async Task GetNearbyDonorsAsync_InvalidLatitude_Throws(
        double latitude)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetNearbyDonorsAsync(
                "O+",
                latitude,
                74.7421,
                20,
                1,
                10));
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(-180.0001)]
    [InlineData(180.0001)]
    [InlineData(181)]
    public async Task GetNearbyDonorsAsync_InvalidLongitude_Throws(
        double longitude)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                longitude,
                20,
                1,
                10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(100.0001)]
    [InlineData(101)]
    public async Task GetNearbyDonorsAsync_InvalidRadius_Throws(
        double radiusKm)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                radiusKm,
                1,
                10));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public async Task GetNearbyDonorsAsync_NonFiniteRadius_Throws(
        double radiusKm)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                radiusKm,
                1,
                10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetNearbyDonorsAsync_InvalidPageNumber_Throws(
        int pageNumber)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                20,
                pageNumber,
                10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetNearbyDonorsAsync_InvalidPageSize_Throws(
        int pageSize)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                20,
                1,
                pageSize));
    }

    [Fact]
    public async Task GetNearbyDonorsAsync_PageSize100_IsAccepted()
    {
        _donorRepositoryMock
            .Setup(repository => repository.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                20,
                1,
                100,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRepositoryResult());

        await _sut.GetNearbyDonorsAsync(
            "O+",
            13.3409,
            74.7421,
            20,
            1,
            100);

        _donorRepositoryMock.Verify(
            repository => repository.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                20,
                1,
                100,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetNearbyDonorsAsync_OverflowingPagination_Throws()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                74.7421,
                20,
                int.MaxValue,
                100));

        _donorRepositoryMock.Verify(
            repository => repository.GetNearbyDonorsAsync(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public async Task GetNearbyDonorsAsync_NonFiniteLatitude_Throws(
    double latitude)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetNearbyDonorsAsync(
                "O+",
                latitude,
                74.7421,
                20,
                1,
                10));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public async Task GetNearbyDonorsAsync_NonFiniteLongitude_Throws(
        double longitude)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetNearbyDonorsAsync(
                "O+",
                13.3409,
                longitude,
                20,
                1,
                10));
    }
}
