namespace Application.Shared.Helpers;

public static class StrHelper
{
  public static string GenerateRandomString(int length)
  {
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var random = new Random();
    return new string(Enumerable.Repeat(chars, length)
      .Select(s => s[random.Next(s.Length)]).ToArray());
  }

  public static string GenerateRandomOTP()
  {
    const string chars = "0123456789";
    var random = new Random();
    return new string([.. Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)])]);
  }
}