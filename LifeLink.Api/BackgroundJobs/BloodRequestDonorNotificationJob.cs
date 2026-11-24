
using System.Net;

using Hangfire;

using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Services;

namespace LifeLink.Api.BackgroundJobs;

public class BloodRequestDonorNotificationJob
{
    private const double NotificationRadiusKm = 50;

    private readonly IBloodRequestRepository _bloodRequestRepository;
    private readonly IDonorRepository _donorRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BloodRequestDonorNotificationJob> _logger;

    public BloodRequestDonorNotificationJob(
        IBloodRequestRepository bloodRequestRepository,
        IDonorRepository donorRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<BloodRequestDonorNotificationJob> logger)
    {
        _bloodRequestRepository = bloodRequestRepository;
        _donorRepository = donorRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(
        int bloodRequestId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting donor notification job for blood request {BloodRequestId}.",
            bloodRequestId);

        var bloodRequest =
            await _bloodRequestRepository.GetByIdAsync(
                bloodRequestId,
                cancellationToken);

        if (bloodRequest is null)
        {
            _logger.LogWarning(
                "Blood request {BloodRequestId} was not found. Job skipped.",
                bloodRequestId);

            return;
        }

        if (bloodRequest.Latitude is null ||
            bloodRequest.Longitude is null)
        {
            _logger.LogWarning(
                "Blood request {BloodRequestId} has no location coordinates. " +
                "Donor matching skipped.",
                bloodRequestId);

            return;
        }

        var donors =
            await _donorRepository.GetEligibleDonorsForBloodRequestAsync(
                bloodRequest.BloodGroup,
                (double)bloodRequest.Latitude.Value,
                (double)bloodRequest.Longitude.Value,
                NotificationRadiusKm,
                cancellationToken);

        if (donors.Count == 0)
        {
            _logger.LogInformation(
                "No eligible donors found for blood request {BloodRequestId}.",
                bloodRequestId);

            return;
        }

        _logger.LogInformation(
            "Found {DonorCount} eligible donors for blood request {BloodRequestId}.",
            donors.Count,
            bloodRequestId);

        var successfulEmails = 0;
        var failedEmails = 0;
        var successfulNotifications = 0;
        var failedNotifications = 0;

        foreach (var donor in donors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var subject =
                    $"LifeLink: {bloodRequest.BloodGroup} blood required nearby";

                var emailBody = BuildEmailBody(
                    donor.FullName,
                    bloodRequest.Id,
                    bloodRequest.BloodGroup,
                    bloodRequest.HospitalName,
                    bloodRequest.HospitalAddress,
                    bloodRequest.UnitsRequired,
                    bloodRequest.Urgency.ToString(),
                    donor.DistanceKm);

                await _emailService.SendEmailAsync(
                    donor.Email,
                    subject,
                    emailBody,
                    cancellationToken);

                successfulEmails++;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedEmails++;

                _logger.LogError(
                    exception,
                    "Email notification failed for donor {DonorId}, " +
                    "blood request {BloodRequestId}.",
                    donor.DonorId,
                    bloodRequestId);
            }

            try
            {
                await _notificationService.NotifyUserAsync(
                    donor.UserId,
                    "NearbyBloodRequest",
                    new
                    {
                        BloodRequestId = bloodRequest.Id,
                        BloodGroup = bloodRequest.BloodGroup,
                        HospitalName = bloodRequest.HospitalName,
                        HospitalAddress = bloodRequest.HospitalAddress,
                        UnitsRequired = bloodRequest.UnitsRequired,
                        Urgency = bloodRequest.Urgency.ToString(),
                        DistanceKm = Math.Round(donor.DistanceKm, 2),
                        Message =
                            $"A nearby {bloodRequest.BloodGroup} blood request " +
                            "matches your donor profile."
                    },
                    cancellationToken);

                successfulNotifications++;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedNotifications++;

                _logger.LogError(
                    exception,
                    "Dashboard notification failed for donor {DonorId}, " +
                    "blood request {BloodRequestId}.",
                    donor.DonorId,
                    bloodRequestId);
            }
        }

        _logger.LogInformation(
            "Donor notification job completed for request {BloodRequestId}. " +
            "Emails: {SuccessfulEmails} successful, {FailedEmails} failed. " +
            "Dashboard notifications: {SuccessfulNotifications} successful, " +
            "{FailedNotifications} failed.",
            bloodRequestId,
            successfulEmails,
            failedEmails,
            successfulNotifications,
            failedNotifications);
    }

    private static string BuildEmailBody(
        string donorName,
        int bloodRequestId,
        string bloodGroup,
        string hospitalName,
        string hospitalAddress,
        int unitsRequired,
        string urgency,
        double distanceKm)
    {
        var safeDonorName = WebUtility.HtmlEncode(donorName);
        var safeBloodGroup = WebUtility.HtmlEncode(bloodGroup);
        var safeHospitalName = WebUtility.HtmlEncode(hospitalName);
        var safeHospitalAddress = WebUtility.HtmlEncode(hospitalAddress);
        var safeUrgency = WebUtility.HtmlEncode(urgency);

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; color: #333;">
                <h2>Blood Donation Request - LifeLink</h2>

                <p>Hello {safeDonorName},</p>

                <p>
                    A nearby blood request matches your donor profile.
                    Your help could make a difference.
                </p>

                <h3>Request Details</h3>

                <table cellpadding="8" cellspacing="0"
                       style="border-collapse: collapse;">
                    <tr>
                        <td><strong>Blood Group</strong></td>
                        <td>{safeBloodGroup}</td>
                    </tr>
                    <tr>
                        <td><strong>Hospital</strong></td>
                        <td>{safeHospitalName}</td>
                    </tr>
                    <tr>
                        <td><strong>Address</strong></td>
                        <td>{safeHospitalAddress}</td>
                    </tr>
                    <tr>
                        <td><strong>Units Required</strong></td>
                        <td>{unitsRequired}</td>
                    </tr>
                    <tr>
                        <td><strong>Urgency</strong></td>
                        <td>{safeUrgency}</td>
                    </tr>
                    <tr>
                        <td><strong>Your Distance</strong></td>
                        <td>{distanceKm:F2} km</td>
                    </tr>
                </table>

                <p>
                    Log in to your LifeLink account to view the request
                    and respond if you are willing and eligible to donate.
                </p>

                <p>
                    <strong>Request ID:</strong> {bloodRequestId}
                </p>

                <p>Thank you for being part of LifeLink.</p>

                <p>LifeLink Team</p>
            </body>
            </html>
            """;
    }
}
