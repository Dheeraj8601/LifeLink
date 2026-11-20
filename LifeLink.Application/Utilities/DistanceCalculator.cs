
namespace LifeLink.Application.Utilities;

public static class DistanceCalculator
{
    private const double EarthRadiusKm = 6371.0;

    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }

    public static double CalculateDistanceKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        // 1. Validate geographical coordinates.
        if (!double.IsFinite(latitude1) ||
            !double.IsFinite(latitude2) ||
            !double.IsFinite(longitude1) ||
            !double.IsFinite(longitude2))
        {
            throw new ArgumentException(
                "Coordinates must contain finite numeric values.");
        }

        if (latitude1 < -90 || latitude1 > 90 ||
            latitude2 < -90 || latitude2 > 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latitude1),
                "Latitude must be between -90 and 90 degrees.");
        }

        if (longitude1 < -180 || longitude1 > 180 ||
            longitude2 < -180 || longitude2 > 180)
        {
            throw new ArgumentOutOfRangeException(
                nameof(longitude1),
                "Longitude must be between -180 and 180 degrees.");
        }

        // 2. Convert degrees into radians.
        double lat1 = ToRadians(latitude1);
        double lon1 = ToRadians(longitude1);

        double lat2 = ToRadians(latitude2);
        double lon2 = ToRadians(longitude2);

        // 3. Calculate differences.
        double deltaLatitude = lat2 - lat1;
        double deltaLongitude = lon2 - lon1;

        // 4. Apply the Haversine formula.
        double a =
            Math.Pow(Math.Sin(deltaLatitude / 2), 2) +
            Math.Cos(lat1) *
            Math.Cos(lat2) *
            Math.Pow(Math.Sin(deltaLongitude / 2), 2);

        // Protect against floating-point rounding errors.
        a = Math.Clamp(a, 0.0, 1.0);

        // 5. Calculate angular distance.
        double c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a));

        // 6. Return distance in kilometres.
        return EarthRadiusKm * c;
    }
}
