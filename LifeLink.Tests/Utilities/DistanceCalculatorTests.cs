using LifeLink.Application.Utilities;
using Xunit;

namespace LifeLink.Tests.Utilities;

public class DistanceCalculatorTests
{
    [Fact]
    public void SameCoordinates_ReturnsZero()
    {
        var distance = DistanceCalculator.CalculateDistanceKm(
            13.3409, 74.7421,
            13.3409, 74.7421);

        Assert.Equal(0, distance, 10);
    }

    [Fact]
    public void UdupiToMangaluru_ReturnsApproximately53Km()
    {
        var distance = DistanceCalculator.CalculateDistanceKm(
            13.3409, 74.7421,
            12.8698, 74.8430);

        Assert.InRange(distance, 52.5, 54.0);
    }

    [Fact]
    public void Distance_IsSymmetric()
    {
        var distanceAB = DistanceCalculator.CalculateDistanceKm(
            13.3409, 74.7421,
            12.8698, 74.8430);

        var distanceBA = DistanceCalculator.CalculateDistanceKm(
            12.8698, 74.8430,
            13.3409, 74.7421);

        Assert.Equal(distanceAB, distanceBA, 10);
    }

    [Fact]
    public void OneDegreeLongitudeAtEquator_IsApproximately111Km()
    {
        var distance = DistanceCalculator.CalculateDistanceKm(
            0, 0,
            0, 1);

        Assert.InRange(distance, 111.0, 112.0);
    }

    [Fact]
    public void OneDegreeLatitude_IsApproximately111Km()
    {
        var distance = DistanceCalculator.CalculateDistanceKm(
            0, 0,
            1, 0);

        Assert.InRange(distance, 111.0, 112.0);
    }

    [Fact]
    public void NegativeCoordinates_AreAccepted()
    {
        var distance = DistanceCalculator.CalculateDistanceKm(
            -10, -20,
            -11, -21);

        Assert.True(distance > 0);
    }

    [Theory]
    [InlineData(-91, 0, 0, 0)]
    [InlineData(91, 0, 0, 0)]
    [InlineData(0, 0, -91, 0)]
    [InlineData(0, 0, 91, 0)]
    public void InvalidLatitude_ThrowsArgumentOutOfRangeException(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DistanceCalculator.CalculateDistanceKm(
                latitude1,
                longitude1,
                latitude2,
                longitude2));
    }

    [Theory]
    [InlineData(-181, 0, 0, 0)]
    [InlineData(181, 0, 0, 0)]
    [InlineData(0, 0, -181, 0)]
    [InlineData(0, 0, 181, 0)]
    public void InvalidLongitude_ThrowsArgumentOutOfRangeException(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DistanceCalculator.CalculateDistanceKm(
                latitude1,
                longitude1,
                latitude2,
                longitude2));
    }

    [Theory]
    [InlineData(double.NaN, 0, 0, 0)]
    [InlineData(double.PositiveInfinity, 0, 0, 0)]
    [InlineData(double.NegativeInfinity, 0, 0, 0)]
    [InlineData(0, double.NaN, 0, 0)]
    [InlineData(0, 0, double.NaN, 0)]
    [InlineData(0, 0, 0, double.PositiveInfinity)]
    public void NonFiniteCoordinates_ThrowsArgumentException(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        Assert.Throws<ArgumentException>(() =>
            DistanceCalculator.CalculateDistanceKm(
                latitude1,
                longitude1,
                latitude2,
                longitude2));
    }
}