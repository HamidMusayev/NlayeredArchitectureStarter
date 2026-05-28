using API.Attributes;
using DAL.Redis;
using DTO.Responses;
using ENTITIES.Entities.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Redis.OM;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateToken]
public class PersonController(IPersonRepository personRepository) : ControllerBase
{
    [SwaggerOperation(Summary = "add person to redis")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpPost]
    public async Task<IActionResult> AddPerson([FromBody] Person person)
    {
        await personRepository.AddAsync(person);
        return Ok(new SuccessResult());
    }

    [SwaggerOperation(Summary = "filter by age")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpGet("filterAge")]
    public async Task<IActionResult> FilterByAge([FromQuery] int minAge, [FromQuery] int maxAge)
    {
        var data = await personRepository.FilterByAgeAsync(minAge, maxAge);
        return Ok(new SuccessDataResult<List<Person>>(data));
    }

    [SwaggerOperation(Summary = "filter by geo")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpGet("filterGeo")]
    public async Task<IActionResult> FilterByGeo([FromQuery] double lon, [FromQuery] double lat,
        [FromQuery] double radius, [FromQuery] string unit)
    {
        var data = await personRepository.FilterByGeoAsync(lon, lat, radius, Enum.Parse<GeoLocDistanceUnit>(unit));
        return Ok(new SuccessDataResult<List<Person>>(data));
    }

    [SwaggerOperation(Summary = "filter by name")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpGet("filterName")]
    public async Task<IActionResult> FilterByName([FromQuery] string firstName, [FromQuery] string lastName)
    {
        var data = await personRepository.FilterByNameAsync(firstName, lastName);
        return Ok(new SuccessDataResult<List<Person>>(data));
    }

    [SwaggerOperation(Summary = "filter by postal code")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpGet("postalCode")]
    public async Task<IActionResult> FilterByPostalCode([FromQuery] string postalCode)
    {
        var data = await personRepository.FilterByPostalCodeAsync(postalCode);
        return Ok(new SuccessDataResult<List<Person>>(data));
    }

    [SwaggerOperation(Summary = "filter by full text")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpGet("fullText")]
    public async Task<IActionResult> FilterByPersonalStatement([FromQuery] string text)
    {
        var data = await personRepository.FilterByFullTextAsync(text);
        return Ok(new SuccessDataResult<List<Person>>(data));
    }

    [SwaggerOperation(Summary = "filter by street name")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpGet("streetName")]
    public async Task<IActionResult> FilterByStreetName([FromQuery] string streetName)
    {
        var data = await personRepository.FilterByStreetNameAsync(streetName);
        return Ok(new SuccessDataResult<List<Person>>(data));
    }

    [SwaggerOperation(Summary = "filter by skill")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpGet("skill")]
    public async Task<IActionResult> FilterBySkill([FromQuery] string skill)
    {
        var data = await personRepository.FilterBySkillAsync(skill);
        return Ok(new SuccessDataResult<List<Person>>(data));
    }

    [SwaggerOperation(Summary = "update person age")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpPatch("updateAge/{id}")]
    public async Task<IActionResult> UpdateAge([FromRoute] string id, [FromBody] int newAge)
    {
        await personRepository.UpdateAgeAsync(id, newAge);
        return Ok(new SuccessResult());
    }

    [SwaggerOperation(Summary = "delete person from redis")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePerson([FromRoute] string id)
    {
        await personRepository.DeleteAsync(id);
        return Ok(new SuccessResult());
    }
}