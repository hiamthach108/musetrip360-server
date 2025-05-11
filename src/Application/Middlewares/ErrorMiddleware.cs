
namespace Application.Middlewares;

using Application.Shared.Constant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

public class ErrorHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ErrorHandlingMiddleware> _logger;

  public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      await HandleExceptionAsync(context, ex);
    }
  }

  private async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    _logger.LogError(exception, "An unhandled exception occurred.");

    var response = context.Response;
    response.ContentType = "application/json";

    var errorResponse = new
    {
      error = "An unexpected error occurred.",
      message = exception.Message,
      code = RespCode.INTERNAL_SERVER_ERROR
    };

    switch (exception)
    {
      case ArgumentNullException _:
        response.StatusCode = (int)HttpStatusCode.BadRequest;
        errorResponse = new { error = "A required argument was not provided.", message = exception.Message, code = RespCode.BAD_REQUEST };
        break;

      case UnauthorizedAccessException _:
        response.StatusCode = (int)HttpStatusCode.Unauthorized;
        errorResponse = new { error = "You are not authorized to perform this action.", message = exception.Message, code = RespCode.UNAUTHORIZED };
        break;

      case KeyNotFoundException _:
        response.StatusCode = (int)HttpStatusCode.NotFound;
        errorResponse = new { error = "The requested resource was not found.", message = exception.Message, code = RespCode.NOT_FOUND };
        break;

      default:
        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        break;
    }

    var result = JsonSerializer.Serialize(errorResponse);
    await response.WriteAsync(result);
  }
}