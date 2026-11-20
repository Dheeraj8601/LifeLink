
using LifeLink.Domain.Enums;
using LifeLink.Infrastructure.Mapping;
using DomainRequestResponse =
    LifeLink.Domain.Entities.RequestResponse;
using EfRequestResponse =
    LifeLink.Infrastructure.Data.Entities.RequestResponse;

namespace LifeLink.Tests.Mapping;

public class RequestResponseMapperTests
{
    [Theory]
    [InlineData(RequestResponseStatus.Pending, "Pending")]
    [InlineData(RequestResponseStatus.Accepted, "Accepted")]
    [InlineData(RequestResponseStatus.Rejected, "Rejected")]
    [InlineData(RequestResponseStatus.Withdrawn, "Withdrawn")]
    public void ToEntity_MapsDomainToEfEntity(
        RequestResponseStatus status,
        string expectedStatus)
    {
        // Arrange
        var domain = DomainRequestResponse.Reconstitute(
            101,
            1,
            10,
            status,
            DateTime.UtcNow);

        // Act
        var result = RequestResponseMapper.ToEntity(domain);

        // Assert
        Assert.Equal(101, result.Id);
        Assert.Equal(1, result.BloodRequestId);
        Assert.Equal(10, result.DonorId);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(domain.RespondedAt, result.RespondedAt);
    }

    [Theory]
    [InlineData("Pending", RequestResponseStatus.Pending)]
    [InlineData("Accepted", RequestResponseStatus.Accepted)]
    [InlineData("Rejected", RequestResponseStatus.Rejected)]
    [InlineData("Withdrawn", RequestResponseStatus.Withdrawn)]
    public void ToDomain_MapsEfEntityToDomain(
        string status,
        RequestResponseStatus expectedStatus)
    {
        // Arrange
        var respondedAt = DateTime.UtcNow;

        var entity = new EfRequestResponse
        {
            Id = 101,
            BloodRequestId = 1,
            DonorId = 10,
            Status = status,
            RespondedAt = respondedAt
        };

        // Act
        var result = RequestResponseMapper.ToDomain(entity);

        // Assert
        Assert.Equal(101, result.Id);
        Assert.Equal(1, result.BloodRequestId);
        Assert.Equal(10, result.DonorId);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(respondedAt, result.RespondedAt);
    }

    [Theory]
    [InlineData("InvalidStatus")]
    [InlineData("")]
    public void ToDomain_InvalidStatus_ThrowsInvalidOperationException(
        string status)
    {
        // Arrange
        var entity = new EfRequestResponse
        {
            Id = 101,
            BloodRequestId = 1,
            DonorId = 10,
            Status = status,
            RespondedAt = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            (Action)(() => RequestResponseMapper.ToDomain(entity)));
    }
}
