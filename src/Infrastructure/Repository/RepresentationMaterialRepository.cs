using Database;
using Domain.Content;
using Microsoft.EntityFrameworkCore;

public interface IRepresentationMaterialRepository
{
  Task<RepresentationMaterial?> GetByIdAsync(Guid id);
  Task<IEnumerable<RepresentationMaterial>> GetAllAsync();
  Task<IEnumerable<RepresentationMaterial>> GetByEventIdAsync(Guid eventId);
  Task AddAsync(RepresentationMaterial material);
  Task UpdateAsync(Guid materialId, RepresentationMaterial material);
  Task DeleteAsync(RepresentationMaterial material);
}

public class RepresentationMaterialRepository : IRepresentationMaterialRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public RepresentationMaterialRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task AddAsync(RepresentationMaterial material)
  {
    await _dbContext.RepresentationMaterials.AddAsync(material);
    await _dbContext.SaveChangesAsync();
  }

  public async Task DeleteAsync(RepresentationMaterial material)
  {
    _dbContext.RepresentationMaterials.Remove(material);
    await _dbContext.SaveChangesAsync();
  }

  public async Task<IEnumerable<RepresentationMaterial>> GetAllAsync()
  {
    return await _dbContext.RepresentationMaterials.ToListAsync();
  }

  public async Task<IEnumerable<RepresentationMaterial>> GetByEventIdAsync(Guid eventId)
  {
    return await _dbContext.RepresentationMaterials.Where(m => m.EventId == eventId).ToListAsync();
  }

  public async Task<RepresentationMaterial?> GetByIdAsync(Guid id)
  {
    return await _dbContext.RepresentationMaterials.FindAsync(id);
  }

  public async Task UpdateAsync(Guid materialId, RepresentationMaterial material)
  {
    var existingMaterial = await GetByIdAsync(materialId);
    if (existingMaterial == null)
    {
      throw new Exception("Material not found");
    }
    _dbContext.Entry(existingMaterial).CurrentValues.SetValues(material);
    await _dbContext.SaveChangesAsync();
  }
}
