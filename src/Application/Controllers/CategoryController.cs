namespace Application.Controllers;

using Application.DTOs.Category;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/categories")]
public class CategoryController : ControllerBase
{
  private readonly ILogger<CategoryController> _logger;
  private readonly ICategoryService _categoryService;

  public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService)
  {
    _logger = logger;
    _categoryService = categoryService;
  }

  [HttpGet]
  public async Task<IActionResult> GetAllCategories()
  {
    _logger.LogInformation("Get all categories request received");
    return await _categoryService.HandleGetAllAsync();
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetCategoryById(Guid id)
  {
    _logger.LogInformation("Get category by id request received for id: {Id}", id);
    return await _categoryService.HandleGetByIdAsync(id);
  }

  [Protected]
  [HttpPost]
  public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
  {
    _logger.LogInformation("Create category request received");
    return await _categoryService.HandleCreateAsync(dto);
  }

  [Protected]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpdateDto dto)
  {
    _logger.LogInformation("Update category request received for id: {Id}", id);
    return await _categoryService.HandleUpdateAsync(id, dto);
  }

  [Protected]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteCategory(Guid id)
  {
    _logger.LogInformation("Delete category request received for id: {Id}", id);
    return await _categoryService.HandleDeleteAsync(id);
  }
}