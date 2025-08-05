namespace Core.Jwt;

using Application.Shared.Enum;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public interface IJwtService
{
  string GenerateToken(Guid userId, Guid sessionId, string email, UserStatusEnum status, bool isAdmin, int exp);
  Payload? ValidateToken(string token);
}

public class JwtService : IJwtService
{
  private readonly string DEFAULT_SECRET = "0ebe2440a9eba77bed3a7a081b9bb26d792baaec3fcac1eae95b7148bfdcb8c5";
  private readonly byte[] _key;
  private readonly JwtSecurityTokenHandler _handler;
  public JwtService()
  {
    var SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? DEFAULT_SECRET;
    _key = Encoding.ASCII.GetBytes(SecretKey);
    _handler = new JwtSecurityTokenHandler();
  }

  public string GenerateToken(Guid userId, Guid sessionId, string email, UserStatusEnum status, bool isAdmin, int exp)
  {
    var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? DEFAULT_SECRET);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new Claim[]
      {
        new("sessionId", sessionId.ToString()),
        new("status", status.ToString()),
        new("email", email),
        new("isAdmin", isAdmin.ToString())
      }),
      Issuer = userId.ToString(),
      Expires = DateTime.UtcNow.AddSeconds(exp),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = _handler.CreateToken(tokenDescriptor);

    return _handler.WriteToken(token);
  }


  public Payload? ValidateToken(string token)
  {
    _handler.ValidateToken(token, new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(_key),
      ValidateIssuer = false,
      ValidateAudience = false,
      // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
      ClockSkew = TimeSpan.Zero
    }, out SecurityToken validatedToken);

    var result = (JwtSecurityToken)validatedToken;

    var payload = new Payload()
    {
      UserId = Guid.Parse(result.Issuer),
      Email = result.Claims.First(x => x.Type == "email").Value,
      SessionId = Guid.Parse(result.Claims.First(x => x.Type == "sessionId").Value),
      Status = Enum.Parse<UserStatusEnum>(result.Claims.First(x => x.Type == "status").Value),
      IsAdmin = bool.Parse(result.Claims.First(x => x.Type == "isAdmin").Value)
    };

    return payload;
  }
}