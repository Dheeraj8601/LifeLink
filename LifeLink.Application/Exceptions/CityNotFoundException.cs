namespace LifeLink.Application.Exceptions;

public class CityNotFoundException : Exception
{
    public CityNotFoundException(int cityId)
        : base($"City with ID {cityId} does not exist.")
    {
    }
}