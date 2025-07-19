namespace Application.Shared.Constant;

public static class JwtConst
{
  public const int ACCESS_TOKEN_EXP = 3600; // 1 hour
  public const int REFRESH_TOKEN_EXP = 3600 * 24 * 30; // 30 days
  public const int REFRESH_TOKEN_LENGTH = 24;

  public const string PAYLOAD_KEY = "payload";

}
