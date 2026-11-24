
namespace LifeLink.Application.Interfaces.Services;

public interface IBloodRequestNotificationScheduler
{
    string EnqueueDonorNotification(int bloodRequestId);
}
