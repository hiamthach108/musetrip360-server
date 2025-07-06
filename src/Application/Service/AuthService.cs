namespace Application.Service;

using Database;
using AutoMapper;
using Core.Jwt;
using Application.Shared.Enum;
using Application.Shared.Constant;
using Application.DTOs.Auth;
using Domain.Users;
using System;
using Infrastructure.Repository;
using Application.DTOs.User;
using Core.Crypto;
using Infrastructure.Cache;
using Application.Shared.Type;
using Core.Mail;
using Application.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;
using Core.Firebase;

public interface IAuthService
{
  public Task<GgAuthResp> HandleGoogleLogin(string redirect, string state, GgAuthInfo info);
  public Task<IActionResult> HandleRegister(RegisterReq req);
  public Task<IActionResult> HandleLoginEmail(LoginReq req);
  public Task<IActionResult> HandleLoginGoogle(string redirect);
  public Task<IActionResult> HandleLoginFirebase(string token);
  public Task<IActionResult> HandleVerifyGgToken(string token);
  public Task<IActionResult> HandleRefreshToken(RefreshReq req);
  public Task<IActionResult> HandleOTPForgotPassword(RequestOTP email);
  public Task<IActionResult> HandleVerifyOTPChangePassword(VerifyOTPChangePassword req);
}

public class AuthService : IAuthService
{
  private readonly IMapper _mapper;
  private readonly IJwtService _jwtService;
  private readonly ICryptoService _cryptoService;
  private readonly IUserRepository _userRepo;
  private readonly ICacheService _cacheService;
  protected readonly IHttpContextAccessor _httpCtx;
  protected readonly IMailService _mailService;
  protected readonly IFirebaseAdminService _firebaseService;

  public AuthService(
    IMapper mapper,
    MuseTrip360DbContext dbContext,
    IJwtService jwtService,
    ICryptoService cryptoService,
    ICacheService cacheService,
    IHttpContextAccessor httpCtx,
    IMailService mailService,
    IFirebaseAdminService firebaseService
  )
  {
    _mapper = mapper;
    _jwtService = jwtService;
    _userRepo = new UserRepository(dbContext);
    _cryptoService = cryptoService;
    _cacheService = cacheService;
    _httpCtx = httpCtx;
    _mailService = mailService;
    _firebaseService = firebaseService;
  }

  private string GenerateAccessTk(Guid userId, Guid sessionId, string email, UserStatusEnum status, bool isAdmin = false)
  {
    return _jwtService.GenerateToken(userId, sessionId, email, status, isAdmin, JwtConst.ACCESS_TOKEN_EXP);
  }

  public async Task<GgAuthResp> HandleGoogleLogin(string redirect, string state, GgAuthInfo info)
  {
    var user = await _userRepo.GetUserByEmail(info.Email);

    if (user == null)
    {
      var newUser = new User
      {
        Email = info.Email,
        FullName = info.FullName ?? "Guest",
        Status = UserStatusEnum.Active,
        AuthType = AuthTypeEnum.Google,
        Username = info.Email,
        AvatarUrl = info.ProfileUrl,
      };

      user = await _userRepo.AddAsync(newUser);
      if (user == null)
      {
        return new GgAuthResp
        {
          Success = false,
          Message = "Cannot create user",
        };
      }
    }

    var sessionId = Guid.NewGuid();
    var accessTk = GenerateAccessTk(user.Id, sessionId, user.Email, UserStatusEnum.NotVerified);

    var redisKey = $"local:state:{state}";
    await _cacheService.Set(redisKey, user, TimeSpan.FromMinutes(15));

    var redirectLink = redirect + "?access_token=" + accessTk + "&access_token_exp=" + DateTimeOffset.UtcNow.AddSeconds(JwtConst.ACCESS_TOKEN_EXP).ToUnixTimeSeconds();

    return new GgAuthResp
    {
      Success = true,
      RedirectLink = redirectLink,
    };
  }

  public async Task<IActionResult> HandleRegister(RegisterReq req)
  {
    var hashedPassword = _cryptoService.HashPassword(req.Password);

    var user = await _userRepo.GetUserByEmail(req.Email);

    if (user != null)
    {
      return ErrorResp.BadRequest("Email is already taken");
    }

    var newUser = new User
    {
      Email = req.Email,
      FullName = req.FullName,
      HashedPassword = hashedPassword,
      Status = UserStatusEnum.Active,
      AuthType = AuthTypeEnum.Email,
      Username = req.Email,
    };

    var userAdded = await _userRepo.AddAsync(newUser) ?? throw new Exception("Cannot create user");

    return SuccessResp.Ok(_mapper.Map<UserDto>(userAdded));
  }

  public async Task<IActionResult> HandleLoginEmail(LoginReq req)
  {

    if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
    {
      return ErrorResp.BadRequest("Email & Password are required");
    }

    var user = await _userRepo.GetUserByEmail(req.Email);
    if (user == null)
    {
      return ErrorResp.BadRequest("Email is incorrect");
    }

    if (user.HashedPassword == null || !_cryptoService.VerifyPassword(req.Password, user.HashedPassword))
    {
      return ErrorResp.BadRequest("Password is incorrect");
    }

    var sessionId = Guid.NewGuid();
    var accessTk = GenerateAccessTk(user.Id, sessionId, user.Email, user.Status);
    var accessTkExpAt = DateTimeOffset.UtcNow.AddSeconds(JwtConst.ACCESS_TOKEN_EXP).ToUnixTimeSeconds();
    var refreshTk = _cryptoService.GenerateRandomToken(JwtConst.REFRESH_TOKEN_LENGTH);
    var refreshTkExpAt = DateTimeOffset.UtcNow.AddSeconds(JwtConst.REFRESH_TOKEN_EXP).ToUnixTimeSeconds();

    var userDto = _mapper.Map<UserDto>(user);
    // // cache refresh token
    var redisKey = $"users:{user.Id}:refresh_token:{refreshTk}";
    await _cacheService.Set(redisKey, userDto, TimeSpan.FromSeconds(JwtConst.REFRESH_TOKEN_EXP));

    // update user last login
    user.LastLogin = DateTime.UtcNow;
    await _userRepo.UpdateAsync(user.Id, user);

    return SuccessResp.Ok(new LoginEmailResp
    {
      UserId = user.Id,
      AccessToken = accessTk,
      AccessTokenExpAt = accessTkExpAt,
      RefreshToken = refreshTk,
      RefreshTokenExpAt = refreshTkExpAt,
    });
  }

  public async Task<IActionResult> HandleLoginGoogle(string redirect)
  {
    var ctx = _httpCtx.HttpContext;
    if (ctx == null) return ErrorResp.BadRequest("Cannot get request");

    var request = ctx.Request;
    var state = _cryptoService.GenerateRandomToken(JwtConst.REFRESH_TOKEN_LENGTH);
    var host = $"{request.Scheme}://{request.Host}";

    return SuccessResp.Ok(new LoginGoogleResp
    {
      Token = state,
      RedirectLink = $"{host}/api/v1/auth/google/login?redirect={redirect}&state={state}",
    });
  }

  public async Task<IActionResult> HandleVerifyGgToken(string token)
  {
    var state = await _cacheService.Get<User>($"local:state:{token}");
    if (state == null)
    {
      return ErrorResp.BadRequest("Cannot verify token");
    }

    var sessionId = Guid.NewGuid();
    var accessTk = GenerateAccessTk(state.Id, sessionId, state.Email, state.Status, false);
    var accessTkExpAt = DateTimeOffset.UtcNow.AddSeconds(JwtConst.ACCESS_TOKEN_EXP).ToUnixTimeSeconds();
    var refreshTk = _cryptoService.GenerateRandomToken(JwtConst.REFRESH_TOKEN_LENGTH);
    var refreshTkExpAt = DateTimeOffset.UtcNow.AddSeconds(JwtConst.REFRESH_TOKEN_EXP).ToUnixTimeSeconds();

    var userDto = _mapper.Map<UserDto>(state);
    // // cache refresh token
    var redisKey = $"users:{state.Id}:refresh_token:{refreshTk}";
    await _cacheService.Set(redisKey, userDto, TimeSpan.FromSeconds(JwtConst.REFRESH_TOKEN_EXP));

    return SuccessResp.Ok(new VerifyTokenResp
    {
      UserId = state.Id,
      AccessToken = accessTk,
      AccessTokenExpAt = accessTkExpAt,
      RefreshToken = refreshTk,
      RefreshTokenExpAt = refreshTkExpAt,
    });
  }

  public async Task<IActionResult> HandleRefreshToken(RefreshReq req)
  {

    if (string.IsNullOrEmpty(req.RefreshToken) || string.IsNullOrEmpty(req.UserId))
    {
      return ErrorResp.BadRequest("Refresh token & User ID are required");
    }

    var redisKey = $"users:{req.UserId}:refresh_token:{req.RefreshToken}";
    var user = await _cacheService.Get<UserDto>(redisKey);
    if (user == null)
    {
      return ErrorResp.BadRequest("Refresh token is invalid");
    }

    var sessionId = Guid.NewGuid();
    var accessTk = GenerateAccessTk(user.Id, sessionId, user.Email, user.Status, false);
    var accessTkExpAt = DateTimeOffset.UtcNow.AddSeconds(JwtConst.ACCESS_TOKEN_EXP).ToUnixTimeSeconds();

    return SuccessResp.Ok(new RefreshResp
    {
      AccessToken = accessTk,
      AccessTokenExpAt = accessTkExpAt,
    });
  }

  public async Task<IActionResult> HandleOTPForgotPassword(RequestOTP req)
  {
    var otp = StrHelper.GenerateRandomOTP();

    var redisKey = $"local:otp:{req.Email}:forgot_password";
    await _cacheService.Set(redisKey, otp, TimeSpan.FromMinutes(3));

    var subject = "Forgot Password OTP";
    var message = $"Your OTP is: {otp}";

    await _mailService.SendEmailAsync(req.Email, subject, message);

    return SuccessResp.Ok("OTP sent to your email");
  }

  public async Task<IActionResult> HandleVerifyOTPChangePassword(VerifyOTPChangePassword req)
  {
    var redisKey = $"local:otp:{req.Email}:forgot_password";
    var otp = await _cacheService.Get<string>(redisKey);

    if (otp == null)
    {
      return ErrorResp.BadRequest("OTP is invalid");
    }

    if (otp.Equals(req.Otp))
    {
      var user = await _userRepo.GetUserByEmail(req.Email);
      if (user == null)
      {
        return ErrorResp.NotFound("User not found");
      }

      user.HashedPassword = _cryptoService.HashPassword(req.NewPassword);

      await _userRepo.UpdateAsync(user.Id, user);

      return SuccessResp.Ok("Password changed successfully");
    }
    else
    {
      return ErrorResp.BadRequest("OTP is incorrect");
    }
  }

  public async Task<IActionResult> HandleLoginFirebase(string token)
  {
    var user = await _firebaseService.VerifyIdTokenAsync(token);

    if (user == null)
    {
      return ErrorResp.BadRequest("Cannot verify token");
    }

    var email = user.Claims["email"].ToString();
    var name = user.Claims["name"].ToString();
    var picture = user.Claims["picture"].ToString();

    if (string.IsNullOrEmpty(email))
    {
      return ErrorResp.BadRequest("Cannot get email from token");
    }

    var userDb = await _userRepo.GetUserByEmail(email);

    if (userDb == null)
    {
      var newUser = new User
      {
        Email = email,
        FullName = name ?? "Guest" + user.Uid,
        Status = UserStatusEnum.Active,
        AuthType = AuthTypeEnum.Firebase,
        Username = email,
        AvatarUrl = picture,
      };

      userDb = await _userRepo.AddAsync(newUser);
      if (userDb == null)
      {
        return ErrorResp.BadRequest("Cannot create user");
      }
    }

    var sessionId = Guid.NewGuid();

    var accessTk = GenerateAccessTk(userDb.Id, sessionId, userDb.Email, UserStatusEnum.Active, false);

    var accessTkExpAt = DateTimeOffset.UtcNow.AddSeconds(JwtConst.ACCESS_TOKEN_EXP).ToUnixTimeSeconds();
    var refreshTk = _cryptoService.GenerateRandomToken(JwtConst.REFRESH_TOKEN_LENGTH);
    var refreshTkExpAt = DateTimeOffset.UtcNow.AddSeconds(JwtConst.REFRESH_TOKEN_EXP).ToUnixTimeSeconds();

    var userDto = _mapper.Map<UserDto>(userDb);
    // // cache refresh token
    var redisKey = $"users:{userDb.Id}:refresh_token:{refreshTk}";
    await _cacheService.Set(redisKey, userDto, TimeSpan.FromSeconds(JwtConst.REFRESH_TOKEN_EXP));

    return SuccessResp.Ok(new LoginEmailResp
    {
      UserId = userDb.Id,
      AccessToken = accessTk,
      AccessTokenExpAt = accessTkExpAt,
      RefreshToken = refreshTk,
      RefreshTokenExpAt = refreshTkExpAt,
    });
  }
}