using LifeLink.Application.DTOs.Donors;
using LifeLink.Application.Exceptions;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Security;
using LifeLink.Application.Services;
using Moq;
using Xunit;

namespace LifeLink.Tests.Services;

public class DonorServiceTests
{
    private readonly Mock<IDonorRepository> _donorRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly DonorService _donorService;

    public DonorServiceTests()
    {
        _donorRepositoryMock = new Mock<IDonorRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _donorService = new DonorService(
            _donorRepositoryMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task RegisterDonorAsync_InvalidCityId_ThrowsCityNotFoundException()
    {
        // Arrange
        var command = new RegisterDonorCommand
        {
            FullName = "Test Donor",
            Email = "test@example.com",
            Password = "Test@12345",
            PhoneNumber = "9876543210",
            CityId = 999,
            BloodGroup = "O+"
        };

        _donorRepositoryMock
            .Setup(r => r.CityExistsAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<CityNotFoundException>(
            () => _donorService.RegisterDonorAsync(command));
    }

    [Fact]
    public async Task RegisterDonorAsync_ExistingEmail_ThrowsDuplicateEmailException()
    {
        // Arrange
        var command = new RegisterDonorCommand
        {
            FullName = "Test Donor",
            Email = "existing@example.com",
            Password = "Test@12345",
            PhoneNumber = "9876543210",
            CityId = 1,
            BloodGroup = "O+"
        };

        _donorRepositoryMock
            .Setup(r => r.CityExistsAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _donorRepositoryMock
            .Setup(r => r.EmailExistsAsync(
                "existing@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => _donorService.RegisterDonorAsync(command));
    }

    [Fact]
    public async Task RegisterDonorAsync_ValidCommand_RegistersAndReturnsDonor()
    {
        // Arrange
        var command = new RegisterDonorCommand
        {
            FullName = "Test Donor",
            Email = "TEST@example.com",
            Password = "Test@12345",
            PhoneNumber = "9876543210",
            CityId = 1,
            BloodGroup = "O+",
            Latitude = 13.3409m,
            Longitude = 74.7421m
        };

        _donorRepositoryMock
            .Setup(r => r.CityExistsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _donorRepositoryMock
            .Setup(r => r.EmailExistsAsync(
                "test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(p => p.HashPassword("Test@12345"))
            .Returns("hashed-password");

        _donorRepositoryMock
            .Setup(r => r.RegisterAsync(
                It.IsAny<LifeLink.Domain.Entities.User>(),
                It.IsAny<LifeLink.Domain.Entities.Donor>(),
                It.IsAny<CancellationToken>()))
            .Returns((
                LifeLink.Domain.Entities.User user,
                LifeLink.Domain.Entities.Donor donor,
                CancellationToken token) =>
            {
                user.Id = 10;
                donor.Id = 20;
                donor.UserId = 10;

                return Task.CompletedTask;
            });

        // Act
        var result = await _donorService.RegisterDonorAsync(command);

        // Assert
        Assert.Equal(20, result.Id);
        Assert.Equal(10, result.UserId);
        Assert.Equal("O+", result.BloodGroup);

        _passwordHasherMock.Verify(
            p => p.HashPassword("Test@12345"),
            Times.Once);

        _donorRepositoryMock.Verify(
            r => r.RegisterAsync(
                It.IsAny<LifeLink.Domain.Entities.User>(),
                It.IsAny<LifeLink.Domain.Entities.Donor>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}