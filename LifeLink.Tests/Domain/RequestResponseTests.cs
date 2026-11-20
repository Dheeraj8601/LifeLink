
using LifeLink.Domain.Entities;
using LifeLink.Domain.Enums;

namespace LifeLink.Tests.Domain;

public class RequestResponseTests
{
    [Fact]
    public void Constructor_SetsInitialStatusToPending()
    {
        // Arrange & Act
        var response = new RequestResponse();

        // Assert
        Assert.Equal(RequestResponseStatus.Pending, response.Status);
        Assert.True(response.RespondedAt <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(RequestResponseStatus.Accepted)]
    [InlineData(RequestResponseStatus.Rejected)]
    [InlineData(RequestResponseStatus.Withdrawn)]
    public void UpdateStatus_FromPending_AllowsValidTransitions(
        RequestResponseStatus newStatus)
    {
        // Arrange
        var response = new RequestResponse();

        // Act
        response.UpdateStatus(newStatus);

        // Assert
        Assert.Equal(newStatus, response.Status);
    }

    [Fact]
    public void UpdateStatus_FromAcceptedToWithdrawn_IsAllowed()
    {
        // Arrange
        var response = new RequestResponse();
        response.UpdateStatus(RequestResponseStatus.Accepted);

        // Act
        response.UpdateStatus(RequestResponseStatus.Withdrawn);

        // Assert
        Assert.Equal(
            RequestResponseStatus.Withdrawn,
            response.Status);
    }

    [Theory]
    [InlineData(RequestResponseStatus.Rejected)]
    [InlineData(RequestResponseStatus.Pending)]
    [InlineData(RequestResponseStatus.Accepted)]
    public void UpdateStatus_FromRejectedOrWithdrawnTerminalStates_IsNotAllowed(
        RequestResponseStatus newStatus)
    {
        // Arrange
        var response = new RequestResponse();
        response.UpdateStatus(RequestResponseStatus.Rejected);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => response.UpdateStatus(newStatus));
    }

    [Fact]
    public void UpdateStatus_FromWithdrawnToAccepted_IsNotAllowed()
    {
        // Arrange
        var response = new RequestResponse();
        response.UpdateStatus(RequestResponseStatus.Withdrawn);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => response.UpdateStatus(RequestResponseStatus.Accepted));
    }

    [Fact]
    public void UpdateStatus_FromAcceptedToRejected_IsNotAllowed()
    {
        // Arrange
        var response = new RequestResponse();
        response.UpdateStatus(RequestResponseStatus.Accepted);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => response.UpdateStatus(RequestResponseStatus.Rejected));
    }
}
