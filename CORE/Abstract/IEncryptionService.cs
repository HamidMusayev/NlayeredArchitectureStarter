namespace CORE.Abstract;

public interface IEncryptionService
{
    string Encrypt(string value);
    string Decrypt(string value);
}