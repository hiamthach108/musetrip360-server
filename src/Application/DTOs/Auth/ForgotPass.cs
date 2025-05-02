namespace Application.DTOs.Auth;

public class RequestOTP
{
  public string Email { get; set; } = null!;
}

public class VerifyOTPChangePassword
{
  public string Email { get; set; } = null!;
  public string Otp { get; set; } = null!;
  public string NewPassword { get; set; } = null!;
}