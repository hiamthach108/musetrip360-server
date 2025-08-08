namespace Infrastructure.Repository;

using System.Threading.Tasks;
using Application.DTOs.Article;
using Application.Shared.Enum;
using Database;
using Domain.Museums;
using Microsoft.EntityFrameworkCore;

public interface IArticleRepository
{
  Task<Article?> GetByIdAsync(Guid id);
  ArticleList GetAll(ArticleQuery query);
  ArticleList GetAllAdmin(ArticleAdminQuery query);
  Task<Article> AddAsync(Article article);
  Task<Article> UpdateAsync(Guid id, Article article);
  Task DeleteAsync(Article article);
  Task<bool> ExistsAsync(Guid id);
}

public class ArticleList
{
  public IEnumerable<Article> Articles { get; set; } = [];
  public int TotalCount { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
  public int TotalPages { get; set; }
}

public class ArticleRepository : IArticleRepository
{
  private readonly MuseTrip360DbContext _context;

  public ArticleRepository(MuseTrip360DbContext context)
  {
    _context = context;
  }

  public async Task<Article?> GetByIdAsync(Guid id)
  {
    return await _context.Articles
      .Include(a => a.Museum)
      .Include(a => a.CreatedByUser)
      .FirstOrDefaultAsync(a => a.Id == id);
  }

  public ArticleList GetAll(ArticleQuery query)
  {
    var articles = _context.Articles
      .Include(a => a.Museum)
      .Include(a => a.CreatedByUser)
      .Where(a => a.Status == ArticleStatusEnum.Published)
      .AsQueryable();

    if (!string.IsNullOrEmpty(query.Search))
    {
      articles = articles.Where(a =>
        a.Title.Contains(query.Search) ||
        a.Content.Contains(query.Search));
    }

    if (query.MuseumId.HasValue)
    {
      articles = articles.Where(a => a.MuseumId == query.MuseumId.Value);
    }

    articles = articles.OrderByDescending(a => a.PublishedAt);

    var totalCount = articles.Count();
    var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

    var paginatedArticles = articles
      .Skip((query.Page - 1) * query.PageSize)
      .Take(query.PageSize)
      .ToList();

    return new ArticleList
    {
      Articles = paginatedArticles,
      TotalCount = totalCount,
      Page = query.Page,
      PageSize = query.PageSize,
      TotalPages = totalPages
    };
  }

  public ArticleList GetAllAdmin(ArticleAdminQuery query)
  {
    var articles = _context.Articles
      .Include(a => a.Museum)
      .Include(a => a.CreatedByUser)
      .AsQueryable();

    if (!string.IsNullOrEmpty(query.Search))
    {
      articles = articles.Where(a =>
        a.Title.Contains(query.Search) ||
        a.Content.Contains(query.Search));
    }

    if (query.MuseumId.HasValue)
    {
      articles = articles.Where(a => a.MuseumId == query.MuseumId.Value);
    }

    if (query.Status.HasValue)
    {
      articles = articles.Where(a => a.Status == query.Status.Value);
    }

    articles = articles.OrderByDescending(a => a.CreatedAt);

    var totalCount = articles.Count();
    var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

    var paginatedArticles = articles
      .Skip((query.Page - 1) * query.PageSize)
      .Take(query.PageSize)
      .ToList();

    return new ArticleList
    {
      Articles = paginatedArticles,
      TotalCount = totalCount,
      Page = query.Page,
      PageSize = query.PageSize,
      TotalPages = totalPages
    };
  }

  public async Task<Article> AddAsync(Article article)
  {
    article.Id = Guid.NewGuid();
    article.CreatedAt = DateTime.UtcNow;
    article.UpdatedAt = DateTime.UtcNow;

    _context.Articles.Add(article);
    await _context.SaveChangesAsync();
    return article;
  }

  public async Task<Article> UpdateAsync(Guid id, Article article)
  {
    _context.Articles.Update(article);
    await _context.SaveChangesAsync();
    return article;
  }

  public async Task DeleteAsync(Article article)
  {
    _context.Articles.Remove(article);
    await _context.SaveChangesAsync();
  }

  public async Task<bool> ExistsAsync(Guid id)
  {
    return await _context.Articles.AnyAsync(a => a.Id == id);
  }
}