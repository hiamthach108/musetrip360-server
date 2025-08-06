namespace Infrastructure.Repository;

using Database;
using Domain.Content;
using Microsoft.EntityFrameworkCore;

public interface ICategoryRepository
{
  Task<Category?> GetByIdAsync(Guid id);
  Task<IEnumerable<Category>> GetAllAsync();
  Task<Category> AddAsync(Category category);
  Task<Category?> UpdateAsync(Guid id, Category category);
  Task<bool> DeleteAsync(Guid id);
  Task<bool> ExistsAsync(Guid id);
  Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
}

public class CategoryRepository : ICategoryRepository
{
  private readonly MuseTrip360DbContext _context;

  public CategoryRepository(MuseTrip360DbContext context)
  {
    _context = context;
  }

  public async Task<Category?> GetByIdAsync(Guid id)
  {
    return await _context.Categories
      .Include(c => c.Museums)
      .FirstOrDefaultAsync(c => c.Id == id);
  }

  public async Task<IEnumerable<Category>> GetAllAsync()
  {
    return await _context.Categories
      .Include(c => c.Museums)
      .OrderBy(c => c.Name)
      .ToListAsync();
  }

  public async Task<Category> AddAsync(Category category)
  {
    var result = await _context.Categories.AddAsync(category);
    await _context.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<Category?> UpdateAsync(Guid id, Category category)
  {
    var existingCategory = await _context.Categories.FindAsync(id);
    if (existingCategory == null)
      return null;

    existingCategory.Name = category.Name;
    existingCategory.Description = category.Description;
    existingCategory.Metadata = category.Metadata;

    await _context.SaveChangesAsync();
    return existingCategory;
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    var category = await _context.Categories.FindAsync(id);
    if (category == null)
      return false;

    _context.Categories.Remove(category);
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<bool> ExistsAsync(Guid id)
  {
    return await _context.Categories.AnyAsync(c => c.Id == id);
  }

  public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
  {
    var query = _context.Categories.Where(c => c.Name == name);
    if (excludeId.HasValue)
    {
      query = query.Where(c => c.Id != excludeId.Value);
    }
    return await query.AnyAsync();
  }
}