namespace MuseTrip360.src.Application.Controllers;

using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("/api/v1/ping")]
public class PingController : ControllerBase
{
  [HttpGet]
  public IActionResult Ping()
  {
    return Ok(new { name = "MuseTrip360", version = "1.0.0" });
  }
}