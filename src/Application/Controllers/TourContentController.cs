using Application.Middlewares;
using Microsoft.AspNetCore.Mvc;
[Route("api/v1/tour-contents")]
[ApiController]
public class TourContentController : ControllerBase
{
    private readonly ITourContentService _tourContentService;
    private readonly IAdminTourContentService _adminTourContentService;
    public TourContentController(ITourContentService tourContentService, IAdminTourContentService adminTourContentService)
    {
        _tourContentService = tourContentService;
        _adminTourContentService = adminTourContentService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TourContentQuery query)
    {
        return await _tourContentService.HandleGetAllAsync(query);
    }
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] TourContentAdminQuery query)
    {
        return await _adminTourContentService.HandleGetAllAdminAsync(query);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _tourContentService.HandleGetByIdAsync(id);
    }
    [Protected]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TourContentCreateDto dto)
    {
        return await _adminTourContentService.HandleCreateAsync(dto);
    }
    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TourContentUpdateDto dto)
    {
        return await _adminTourContentService.HandleUpdateAsync(id, dto);
    }
    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _adminTourContentService.HandleDeleteAsync(id);
    }
    [Protected]
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        return await _adminTourContentService.HandleActivateAsync(id);
    }
    [Protected]
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return await _adminTourContentService.HandleDeactivateAsync(id);
    }
}

