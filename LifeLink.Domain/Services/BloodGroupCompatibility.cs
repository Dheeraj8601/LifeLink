
namespace LifeLink.Domain.Services;

public static class BloodGroupCompatibility
{
    public static string[] GetCompatibleDonorBloodGroups(
        string recipientBloodGroup)
    {
        if (string.IsNullOrWhiteSpace(recipientBloodGroup))
        {
            throw new ArgumentException(
                "Recipient blood group is required.",
                nameof(recipientBloodGroup));
        }

        return recipientBloodGroup.Trim().ToUpperInvariant() switch
        {
            "O-" => ["O-"],
            "O+" => ["O-", "O+"],

            "A-" => ["O-", "A-"],
            "A+" => ["O-", "O+", "A-", "A+"],

            "B-" => ["O-", "B-"],
            "B+" => ["O-", "O+", "B-", "B+"],

            "AB-" => ["O-", "A-", "B-", "AB-"],
            "AB+" => ["O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+"],

            _ => throw new ArgumentException(
                $"Invalid blood group: {recipientBloodGroup}",
                nameof(recipientBloodGroup))
        };
    }

    public static bool IsCompatible(
        string donorBloodGroup,
        string recipientBloodGroup)
    {
        if (string.IsNullOrWhiteSpace(donorBloodGroup) ||
            string.IsNullOrWhiteSpace(recipientBloodGroup))
        {
            return false;
        }

        var compatibleDonorGroups =
            GetCompatibleDonorBloodGroups(recipientBloodGroup);

        return compatibleDonorGroups.Contains(
            donorBloodGroup.Trim().ToUpperInvariant());
    }
}
