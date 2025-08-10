namespace Application.Service;

using AutoMapper;
using Database;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Repository;
using Application.DTOs.Article;
using Application.Shared.Type;
using Domain.Museums;
using Application.Shared.Enum;

public interface IArticleService
{
    Task<IActionResult> HandleGetAll(ArticleQuery query);
    Task<IActionResult> HandleGetAllAdmin(ArticleAdminQuery query);
    Task<IActionResult> HandleGetById(Guid id);
    Task<IActionResult> HandleCreate(ArticleCreateDto createDto);
    Task<IActionResult> HandleUpdate(Guid id, ArticleUpdateDto updateDto);
    Task<IActionResult> HandleDelete(Guid id);
}

public class ArticleService : BaseService, IArticleService
{
    private readonly IArticleRepository _articleRepository;
    private readonly IMuseumRepository _museumRepository;

    public ArticleService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(dbContext, mapper, httpContextAccessor)
    {
        _articleRepository = new ArticleRepository(dbContext);
        _museumRepository = new MuseumRepository(dbContext);
    }

    public async Task<IActionResult> HandleGetAll(ArticleQuery query)
    {
        try
        {
            var result = _articleRepository.GetAll(query);
            var articles = _mapper.Map<List<ArticleDto>>(result.Articles);

            return SuccessResp.Ok(new
            {
                List = articles,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            });
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleGetAllAdmin(ArticleAdminQuery query)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
                return ErrorResp.Unauthorized("Authentication required");

            var result = _articleRepository.GetAllAdmin(query);
            var articles = _mapper.Map<List<ArticleDto>>(result.Articles);

            return SuccessResp.Ok(new
            {
                List = articles,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            });
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleGetById(Guid id)
    {
        try
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
                return ErrorResp.NotFound("Article not found");

            var articleDto = _mapper.Map<ArticleDto>(article);
            return SuccessResp.Ok(articleDto);
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleCreate(ArticleCreateDto createDto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
                return ErrorResp.Unauthorized("Authentication required");

            if (!_museumRepository.IsMuseumExists(createDto.MuseumId))
                return ErrorResp.BadRequest("Museum not found");

            var article = _mapper.Map<Article>(createDto);
            article.CreatedBy = payload.UserId;
            article.Status = createDto.Status == ArticleStatusEnum.Draft ? ArticleStatusEnum.Draft : ArticleStatusEnum.Pending;

            var createdArticle = await _articleRepository.AddAsync(article);
            var articleDto = _mapper.Map<ArticleDto>(createdArticle);

            return SuccessResp.Created(articleDto);
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleUpdate(Guid id, ArticleUpdateDto updateDto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
                return ErrorResp.Unauthorized("Authentication required");

            var existingArticle = await _articleRepository.GetByIdAsync(id);
            if (existingArticle == null)
                return ErrorResp.NotFound("Article not found");

            if (updateDto.Status == ArticleStatusEnum.Published && existingArticle.Status != ArticleStatusEnum.Pending)
                return ErrorResp.BadRequest("Article must be in Pending status to be published");
            if (updateDto.Status == ArticleStatusEnum.Rejected && existingArticle.Status != ArticleStatusEnum.Pending)
                return ErrorResp.BadRequest("Article must be in Pending status to be rejected");


            // map to existing article
            _mapper.Map(updateDto, existingArticle);

            if (updateDto.Status == ArticleStatusEnum.Published)
            {
                existingArticle.PublishedAt = DateTime.UtcNow;
            }

            var updatedArticle = await _articleRepository.UpdateAsync(id, existingArticle);
            var articleDto = _mapper.Map<ArticleDto>(updatedArticle);

            return SuccessResp.Ok(articleDto);
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleDelete(Guid id)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
                return ErrorResp.Unauthorized("Authentication required");

            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
                return ErrorResp.NotFound("Article not found");

            await _articleRepository.DeleteAsync(article);
            return SuccessResp.Ok("Article deleted successfully");
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }
}