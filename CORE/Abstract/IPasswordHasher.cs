namespace CORE.Abstract;

public interface IPasswordHasher
{
    string GenerateSalt();
    string Hash(string password, string salt);
}