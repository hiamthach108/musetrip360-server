namespace Application.Service;

using Application.DTOs.Category;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Content;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface ICategoryService
{
  Task<IActionResult> HandleGetAllAsync();
  Task<IActionResult> HandleGetByIdAsync(Guid id);
  Task<IActionResult> HandleCreateAsync(CategoryCreateDto dto);
  Task<IActionResult> HandleUpdateAsync(Guid id, CategoryUpdateDto dto);
  Task<IActionResult> HandleDeleteAsync(Guid id);
}

public class CategoryService : BaseService, ICategoryService
{
  private readonly ICategoryRepository _categoryRepository;

  public CategoryService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpCtx)
    : base(dbContext, mapper, httpCtx)
  {
    _categoryRepository = new CategoryRepository(dbContext);
  }

  public async Task<IActionResult> HandleGetAllAsync()
  {
    try
    {
      var categories = await _categoryRepository.GetAllAsync();
      var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
      return SuccessResp.Ok(categoryDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving categories: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetByIdAsync(Guid id)
  {
    try
    {
      var category = await _categoryRepository.GetByIdAsync(id);
      if (category == null)
        return ErrorResp.NotFound("Category not found");

      var categoryDto = _mapper.Map<CategoryDto>(category);
      return SuccessResp.Ok(categoryDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving category: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleCreateAsync(CategoryCreateDto dto)
  {
    try
    {
      if (await _categoryRepository.NameExistsAsync(dto.Name))
        return ErrorResp.BadRequest("Category name already exists");

      var category = _mapper.Map<Category>(dto);
      var createdCategory = await _categoryRepository.AddAsync(category);
      var categoryDto = _mapper.Map<CategoryDto>(createdCategory);

      return SuccessResp.Created(categoryDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error creating category: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleUpdateAsync(Guid id, CategoryUpdateDto dto)
  {
    try
    {
      if (!await _categoryRepository.ExistsAsync(id))
        return ErrorResp.NotFound("Category not found");

      if (await _categoryRepository.NameExistsAsync(dto.Name, id))
        return ErrorResp.BadRequest("Category name already exists");

      var category = _mapper.Map<Category>(dto);
      var updatedCategory = await _categoryRepository.UpdateAsync(id, category);

      if (updatedCategory == null)
        return ErrorResp.NotFound("Category not found");

      var categoryDto = _mapper.Map<CategoryDto>(updatedCategory);
      return SuccessResp.Ok(categoryDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error updating category: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleDeleteAsync(Guid id)
  {
    try
    {
      var deleted = await _categoryRepository.DeleteAsync(id);
      if (!deleted)
        return ErrorResp.NotFound("Category not found");

      return SuccessResp.NoContent();
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error deleting category: {ex.Message}");
    }
  }
}