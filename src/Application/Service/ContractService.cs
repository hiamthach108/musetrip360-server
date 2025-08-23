namespace Application.Service;

using AutoMapper;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using System.Text;
using Application.Shared.Type;
using PuppeteerSharp.Media;
using Domain.Subscription;
using Domain.Museums;
using Core.Cloudinary;

public class ContractService : BaseService
{
  private readonly IWebHostEnvironment _environment;
  private readonly ICloudinaryService _cloudinaryService;

  public ContractService(
    MuseTrip360DbContext dbContext,
    IMapper mapper,
    IHttpContextAccessor httpCtx,
    IWebHostEnvironment environment,
    ICloudinaryService cloudinaryService) : base(dbContext, mapper, httpCtx)
  {
    _environment = environment;
    _cloudinaryService = cloudinaryService;
  }

  public async Task<IActionResult> GenerateContractPdf(Guid museumId, Guid planId)
  {
    try
    {
      var museum = await _dbContext.Museums
        .Include(m => m.CreatedByUser)
        .FirstOrDefaultAsync(m => m.Id == museumId);

      if (museum == null)
      {
        throw new Exception("Museum not found");
      }

      var plan = await _dbContext.Plans
        .FirstOrDefaultAsync(p => p.Id == planId);

      if (plan == null)
      {
        throw new Exception("Plan not found");
      }

      var templatePath = Path.Combine(_environment.ContentRootPath, "templates", "contract.html");

      if (!File.Exists(templatePath))
      {
        throw new Exception("Contract template not found");
      }

      var templateContent = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);

      var contractHtml = PopulateTemplate(templateContent, museum, plan);

      var pdfBytes = await ConvertHtmlToPdf(contractHtml);

      var fileName = $"contract_{museumId}_{planId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
      using var memoryStream = new MemoryStream(pdfBytes);
      var formFile = new FormFile(memoryStream, 0, pdfBytes.Length, "file", fileName)
      {
        Headers = new HeaderDictionary(),
        ContentType = "application/pdf"
      };

      var cloudinaryUrl = await _cloudinaryService.UploadFileAsync(formFile);

      return SuccessResp.Ok(new { Url = cloudinaryUrl });
    }
    catch (Exception ex)
    {
      throw new Exception($"Error generating contract: {ex.Message}");
    }
  }

  private string PopulateTemplate(string template, Museum museum, Plan plan)
  {
    var startDate = DateTime.Now;
    var endDate = DateTime.Now.AddDays(plan.DurationDays);
    var dateSigned = DateTime.Now;

    var replacements = new Dictionary<string, string>
    {
      {"{{DateSigned}}", dateSigned.ToString("dd/MM/yyyy")},
      {"{{MuseumName}}", museum.Name},
      {"{{MuseumAddress}}", museum.Location},
      {"{{MuseumPhone}}", museum.ContactPhone},
      {"{{MuseumEmail}}", museum.ContactEmail},
      {"{{ManagerName}}", museum.CreatedByUser?.FullName ?? "N/A"},
      {"{{StartDate}}", startDate.ToString("dd/MM/yyyy")},
      {"{{EndDate}}", endDate.ToString("dd/MM/yyyy")},
      {"{{SystemFee}}", plan.Price.ToString("N0")}
    };

    foreach (var replacement in replacements)
    {
      template = template.Replace(replacement.Key, replacement.Value);
    }

    return template;
  }

  private async Task<byte[]> ConvertHtmlToPdf(string htmlContent)
  {
    await new BrowserFetcher().DownloadAsync();

    using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
      Headless = true,
      Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
    });

    using var page = await browser.NewPageAsync();

    await page.SetContentAsync(htmlContent, new NavigationOptions
    {
      WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
    });

    var pdfOptions = new PdfOptions
    {
      Format = PaperFormat.A4,
    };

    var pdfBytes = await page.PdfDataAsync(pdfOptions);

    return pdfBytes;
  }
}