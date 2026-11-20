using LifeLink.Domain.Entities;
using LifeLink.Domain.Enums;

namespace LifeLink.Tests.Domain;

public class BloodRequestTests
{
    [Fact]
    public void UpdateStatus_FromOpenToFulfilled_ShouldUpdateStatus()
    {
        // Arrange
        var request = new BloodRequest
        {
            Status = BloodRequestStatus.Open
        };

        // Act
        request.UpdateStatus(BloodRequestStatus.Fulfilled);

        // Assert
        Assert.Equal(
            BloodRequestStatus.Fulfilled,
            request.Status);
    }

    [Fact]
    public void UpdateStatus_FromFulfilledToOpen_ShouldThrowException()
    {
        // Arrange
        var request = new BloodRequest
        {
            Status = BloodRequestStatus.Fulfilled
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            request.UpdateStatus(BloodRequestStatus.Open));
    }


    [Theory]
    [InlineData(BloodRequestStatus.Open, BloodRequestStatus.PartiallyFulfilled)]
    [InlineData(BloodRequestStatus.Open, BloodRequestStatus.Fulfilled)]
    [InlineData(BloodRequestStatus.Open, BloodRequestStatus.Cancelled)]
    [InlineData(BloodRequestStatus.PartiallyFulfilled, BloodRequestStatus.Fulfilled)]
    [InlineData(BloodRequestStatus.PartiallyFulfilled, BloodRequestStatus.Cancelled)]
    public void UpdateStatus_WithValidTransition_ShouldUpdateStatus(
    BloodRequestStatus currentStatus,
    BloodRequestStatus newStatus)
    {
        // Arrange
        var request = new BloodRequest
        {
            Status = currentStatus
        };

        // Act
        request.UpdateStatus(newStatus);

        // Assert
        Assert.Equal(newStatus, request.Status);
    }

    [Theory]
    [InlineData(BloodRequestStatus.Fulfilled, BloodRequestStatus.Open)]
    [InlineData(BloodRequestStatus.Fulfilled, BloodRequestStatus.Cancelled)]
    [InlineData(BloodRequestStatus.Cancelled, BloodRequestStatus.Open)]
    [InlineData(BloodRequestStatus.Cancelled, BloodRequestStatus.Fulfilled)]
    [InlineData(BloodRequestStatus.Open, BloodRequestStatus.Open)]
    public void UpdateStatus_WithInvalidTransition_ShouldThrowException(
    BloodRequestStatus currentStatus,
    BloodRequestStatus newStatus)
    {
        // Arrange
        var request = new BloodRequest
        {
            Status = currentStatus
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            request.UpdateStatus(newStatus));
    }
}