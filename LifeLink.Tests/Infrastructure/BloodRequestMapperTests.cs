
using LifeLink.Domain.Entities;
using LifeLink.Domain.Enums;
using LifeLink.Infrastructure.Mapping;

namespace LifeLink.Tests.Infrastructure;

public class BloodRequestMapperTests
{
    [Fact]
    public void ToEntity_ShouldMapDomainPropertiesCorrectly()
    {
        // Arrange
        var domainRequest = new BloodRequest
        {
            Id = 1,
            RequesterId = 10,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            UnitsRequired = 2,
            Urgency = RequestUrgency.Critical,
            Status = BloodRequestStatus.Open,
            Description = "Emergency requirement",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var entity = BloodRequestMapper.ToEntity(domainRequest);

        // Assert
        Assert.Equal(domainRequest.Id, entity.Id);
        Assert.Equal(domainRequest.RequesterId, entity.RequesterId);
        Assert.Equal(domainRequest.CityId, entity.CityId);
        Assert.Equal(domainRequest.BloodGroup, entity.BloodGroup);
        Assert.Equal(domainRequest.HospitalName, entity.HospitalName);
        Assert.Equal(domainRequest.HospitalAddress, entity.HospitalAddress);
        Assert.Equal("Critical", entity.Urgency);
        Assert.Equal("Open", entity.Status);
    }



    [Fact]
    public void ToDomain_ShouldMapEntityPropertiesCorrectly()
    {
        // Arrange
        var entity = new LifeLink.Infrastructure.Data.Entities.BloodRequest
        {
            Id = 1,
            RequesterId = 10,
            CityId = 1,
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            UnitsRequired = 2,
            Urgency = "Critical",
            Status = "Open",
            Description = "Emergency requirement",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var domainRequest = BloodRequestMapper.ToDomain(entity);

        // Assert
        Assert.Equal(entity.Id, domainRequest.Id);
        Assert.Equal(entity.RequesterId, domainRequest.RequesterId);
        Assert.Equal(entity.CityId, domainRequest.CityId);
        Assert.Equal(entity.BloodGroup, domainRequest.BloodGroup);
        Assert.Equal(entity.HospitalName, domainRequest.HospitalName);
        Assert.Equal(entity.HospitalAddress, domainRequest.HospitalAddress);
        Assert.Equal(RequestUrgency.Critical, domainRequest.Urgency);
        Assert.Equal(BloodRequestStatus.Open, domainRequest.Status);
    }



    [Fact]
    public void ToDomain_WithInvalidUrgency_ShouldThrowInvalidOperationException()
    {
        var entity = new LifeLink.Infrastructure.Data.Entities.BloodRequest
        {
            BloodGroup = "O+",
            HospitalName = "KMC Hospital",
            HospitalAddress = "Attavar, Mangaluru",
            Urgency = "SuperCritical",
            Status = "Open"
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => BloodRequestMapper.ToDomain(entity));

        Assert.Contains(
            "Invalid RequestUrgency value 'SuperCritical'",
            exception.Message);
    }

}
