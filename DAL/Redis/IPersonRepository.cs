using ENTITIES.Entities.Redis;
using Redis.OM;

namespace DAL.Redis;

public interface IPersonRepository
{
    Task<Person> AddAsync(Person person);

    Task<List<Person>> FilterByAgeAsync(int minAge, int maxAge);

    Task<List<Person>> FilterByGeoAsync(double lon, double lat, double radius, GeoLocDistanceUnit unit);

    Task<List<Person>> FilterByNameAsync(string firstName, string lastName);

    Task<List<Person>> FilterByPostalCodeAsync(string postalCode);

    Task<List<Person>> FilterByFullTextAsync(string text);

    Task<List<Person>> FilterByStreetNameAsync(string streetName);

    Task<List<Person>> FilterBySkillAsync(string skill);

    Task UpdateAgeAsync(string id, int newAge);

    Task DeleteAsync(string id);
}