
using Hangfire;

using LifeLink.Application.Interfaces.Services;

namespace LifeLink.Api.BackgroundJobs;

public class HangfireBloodRequestNotificationScheduler
    : IBloodRequestNotificationScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireBloodRequestNotificationScheduler(
        IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public string EnqueueDonorNotification(int bloodRequestId)
    {
        if (bloodRequestId <= 0)
        {
            throw new ArgumentException(
                "A valid blood request ID is required.",
                nameof(bloodRequestId));
        }

        return _backgroundJobClient.Enqueue<BloodRequestDonorNotificationJob>(
            job => job.ExecuteAsync(
                bloodRequestId,
                CancellationToken.None));
    }
}
