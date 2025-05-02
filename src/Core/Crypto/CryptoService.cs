namespace Core.Crypto;

using BCrypt.Net;
using System.Security.Cryptography;

public interface ICryptoService
{
  string HashPassword(string password);
  bool VerifyPassword(string password, string hash);
  string GenerateRandomToken(int length);
}

public class CryptoService : ICryptoService
{
  private const int WorkFactor = 10;

  public string HashPassword(string password)
  {
    return BCrypt.HashPassword(password, WorkFactor);
  }

  public bool VerifyPassword(string password, string hash)
  {
    return BCrypt.Verify(password, hash);
  }

  public string GenerateRandomToken(int length)
  {
    const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    char[] chars = new char[length];

    using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
    {
      byte[] data = new byte[length];

      // Get random bytes
      crypto.GetBytes(data);

      // Convert random bytes to chars from valid char set
      for (int i = 0; i < length; i++)
      {
        chars[i] = validChars[data[i] % validChars.Length];
      }
    }

    return new string(chars);
  }
}