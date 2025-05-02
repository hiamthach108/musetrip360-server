namespace Application.Middlewares;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Application.Shared.Type;
using Application.Shared.Constant;
using Core.Jwt;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
{
  public AdminOnlyAttribute()
  {
  }

  public void OnAuthorization(AuthorizationFilterContext context)
  {
    try
    {
      var payload = context.HttpContext.Items[JwtConst.PAYLOAD_KEY] as Payload;
      if (payload == null)
      {
        context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Result = ErrorResp.Unauthorized("Invalid token");
        return;
      }

      if (!payload.IsAdmin)
      {
        context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Result = ErrorResp.Forbidden("Forbidden");
        return;
      }
    }
    catch (Exception e)
    {
      context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      context.Result = ErrorResp.Unauthorized(e.Message);
      return;
    }
  }
}