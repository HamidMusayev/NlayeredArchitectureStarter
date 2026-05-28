using ENTITIES.Entities.Redis;
using Redis.OM;
using Redis.OM.Searching;

namespace DAL.Redis;

public class PersonRepository : IPersonRepository
{
    private readonly RedisCollection<Person> _collection;
    private readonly RedisConnectionProvider _provider;

    public PersonRepository(RedisConnectionProvider provider)
    {
        _provider = provider;
        _collection = (RedisCollection<Person>)_provider.RedisCollection<Person>();
    }

    public async Task<Person> AddAsync(Person person)
    {
        await _collection.InsertAsync(person);
        return person;
    }

    public Task<List<Person>> FilterByAgeAsync(int minAge, int maxAge)
    {
        return Task.FromResult(_collection.Where(x => x.Age >= minAge && x.Age <= maxAge).ToList());
    }

    public Task<List<Person>> FilterByGeoAsync(double lon, double lat, double radius, GeoLocDistanceUnit unit)
    {
        return Task.FromResult(
            _collection.GeoFilter(x => x.Address!.Location, lon, lat, radius, unit).ToList());
    }

    public Task<List<Person>> FilterByNameAsync(string firstName, string lastName)
    {
        return Task.FromResult(
            _collection.Where(x => x.FirstName == firstName && x.LastName == lastName).ToList());
    }

    public Task<List<Person>> FilterByPostalCodeAsync(string postalCode)
    {
        return Task.FromResult(_collection.Where(x => x.Address!.PostalCode == postalCode).ToList());
    }

    public Task<List<Person>> FilterByFullTextAsync(string text)
    {
        return Task.FromResult(_collection.Where(x => x.PersonalStatement == text).ToList());
    }

    public Task<List<Person>> FilterByStreetNameAsync(string streetName)
    {
        return Task.FromResult(_collection.Where(x => x.Address!.StreetName == streetName).ToList());
    }

    public Task<List<Person>> FilterBySkillAsync(string skill)
    {
        return Task.FromResult(_collection.Where(x => x.Skills.Contains(skill)).ToList());
    }

    public Task UpdateAgeAsync(string id, int newAge)
    {
        foreach (var person in _collection.Where(x => x.Id == id))
            person.Age = newAge;
        _collection.Save();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _provider.Connection.Unlink($"Person:{id}");
        return Task.CompletedTask;
    }
}