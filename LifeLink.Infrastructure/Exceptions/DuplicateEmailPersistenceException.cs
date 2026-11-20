namespace LifeLink.Infrastructure.Exceptions;

public class DuplicateEmailPersistenceException : Exception
{
    public DuplicateEmailPersistenceException(Exception innerException)
        : base("A user with this email already exists.", innerException)
    {
    }
}