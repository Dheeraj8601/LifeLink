using LifeLink.Application.DTOs.Cities;
using LifeLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LifeLink.Api.Controllers;

[ApiController]
[Route("api/cities")]
public class CitiesController : ControllerBase
{
    private readonly ICityService _cityService;

    public CitiesController(ICityService cityService)
    {
        _cityService = cityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCities(
        CancellationToken cancellationToken)
    {
        var cities = await _cityService.GetAllAsync(cancellationToken);

        var response = cities.Select(city => new CityResponse
        {
            Id = city.Id,
            Name = city.Name,
            State = city.State
        }).ToList();

        return Ok(response);
    }
}