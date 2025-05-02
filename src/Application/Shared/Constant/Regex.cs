namespace Application.Shared.Constant;

public static class RegexConst
{
  public const string EMAIL = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
  public const string PHONE_NUMBER = @"^0[0-9]{2,14}$";
  public const string PASSWORD = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{6,20}$";
  public const string FULL_NAME = @"^[a-zA-Z ]+$";
}