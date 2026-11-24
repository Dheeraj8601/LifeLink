
namespace LifeLink.Infrastructure.Email;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "LifeLink";

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool UseStartTls { get; set; } = true;
}
