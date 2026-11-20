namespace LifeLink.Application.Interfaces.Security;

public interface ITokenService
{
    string GenerateAccessToken(int userId,string email,string role);
}