namespace Application.Controllers;

using Application.Shared.Type;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.DTOs.Auth;
using Application.Shared.Enum;
using Application.Service;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase

{
  private readonly ILogger<AuthController> _logger;

  private readonly IAuthService _authService;

  public AuthController(ILogger<AuthController> logger, IAuthService authService)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _authService = authService;
  }

  [HttpGet("google/login")]
  public IActionResult Login([FromQuery] string redirect, [FromQuery] string state)
  {
    var props = new AuthenticationProperties { RedirectUri = $"api/v1/auth/google/callback?redirect={redirect}&state={state}" };
    return Challenge(props, GoogleDefaults.AuthenticationScheme);
  }

  [HttpGet("google/callback")]
  public async Task<IActionResult> GoogleLogin([FromQuery] string redirect, [FromQuery] string state)
  {
    _logger.LogInformation("Google Login");
    // Get data from google
    var response = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    if (response.Principal == null) return ErrorResp.BadRequest("No principal found");

    var name = response.Principal.FindFirstValue(ClaimTypes.Name);
    var fullName = response.Principal.FindFirstValue(ClaimTypes.GivenName);
    var email = response.Principal.FindFirstValue(ClaimTypes.Email);

    if (string.IsNullOrEmpty(email))
    {
      return ErrorResp.BadRequest("Email is required");
    }

    var resp = await _authService.HandleGoogleLogin(redirect, state, new GgAuthInfo
    {
      Email = email,
      FullName = fullName ?? name,
    });

    if (resp.Success && resp.RedirectLink != null)
    {
      return Redirect(resp.RedirectLink);
    }
    else if (resp.Success)
    {
      return SuccessResp.Ok("Login success you can close this tab");
    }
    else
    {
      return ErrorResp.BadRequest(resp.Message ?? "Login failed");
    }
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterReq req)
  {
    _logger.LogInformation("Register");
    return await _authService.HandleRegister(req);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginReq req)
  {
    _logger.LogInformation("Login");

    switch (req.AuthType)
    {
      case AuthTypeEnum.Email:
        return await _authService.HandleLoginEmail(req);
      case AuthTypeEnum.Google:
        return await _authService.HandleLoginGoogle(req.Redirect ?? "");
      case AuthTypeEnum.Firebase:
        return await _authService.HandleLoginFirebase(req.FirebaseToken ?? "");
      default:
        return ErrorResp.BadRequest("Invalid auth type");
    }
  }

  [HttpGet("verify-token")]
  public async Task<IActionResult> VerifyToken([FromQuery] string token)
  {
    _logger.LogInformation("Verify Token");
    return await _authService.HandleVerifyGgToken(token);
  }

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh([FromBody] RefreshReq req)
  {
    _logger.LogInformation("Refresh");
    return await _authService.HandleRefreshToken(req);
  }

  [HttpPost("forgot-password/request")]
  public async Task<IActionResult> ForgotPassword([FromBody] RequestOTP req)
  {
    _logger.LogInformation("Forgot Password");
    return await _authService.HandleOTPForgotPassword(req);
  }

  [HttpPost("forgot-password/verify")]
  public async Task<IActionResult> VerifyOTPChangePassword([FromBody] VerifyOTPChangePassword req)
  {
    _logger.LogInformation("Verify OTP Change Password");
    return await _authService.HandleVerifyOTPChangePassword(req);
  }
}