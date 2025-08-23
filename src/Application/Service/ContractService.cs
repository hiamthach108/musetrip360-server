namespace Application.Service;

using AutoMapper;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using System.IO;
using System.Threading.Tasks;
using Application.Shared.Type;
using Core.Cloudinary;
using iText.IO.Font;

public class ContractData
{
  public string DateSigned { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
  public string MuseumName { get; set; } = string.Empty;
  public string MuseumAddress { get; set; } = string.Empty;
  public string MuseumPhone { get; set; } = string.Empty;
  public string MuseumEmail { get; set; } = string.Empty;
  public string ManagerName { get; set; } = string.Empty;
  public string StartDate { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
  public string EndDate { get; set; } = DateTime.Now.AddYears(1).ToString("dd/MM/yyyy");
  public string SystemFee { get; set; } = string.Empty;
}

public class ContractService : BaseService
{
  private readonly ICloudinaryService _cloudinaryService;
  private readonly ILogger<ContractService> _logger;
  private PdfFont normalFont;
  private PdfFont boldFont;
  private Color blueColor = new DeviceRgb(44, 82, 130); // #2c5282
  private Color orangeColor = new DeviceRgb(221, 107, 32); // #dd6b20
  private Color grayColor = new DeviceRgb(113, 128, 150); // #718096

  public ContractService(
      MuseTrip360DbContext dbContext,
      IMapper mapper,
      IHttpContextAccessor httpCtx,
      ICloudinaryService cloudinaryService,
      ILogger<ContractService> logger
  ) : base(dbContext, mapper, httpCtx)
  {
    _cloudinaryService = cloudinaryService;
    _logger = logger;
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

      _logger.LogInformation($"Generating contract for Museum: {museum.Name}, Plan: {plan.Name}");

      var pdfBytes = GenerateContract(new ContractData
      {
        DateSigned = DateTime.Now.ToString("dd/MM/yyyy"),
        MuseumName = museum.Name,
        MuseumAddress = museum.Location,
        MuseumPhone = museum.ContactPhone,
        MuseumEmail = museum.ContactEmail,
        ManagerName = museum.CreatedByUser.FullName,
        StartDate = DateTime.Now.ToString("dd/MM/yyyy"),
        EndDate = DateTime.Now.AddYears(1).ToString("dd/MM/yyyy"),
        SystemFee = plan.Price.ToString("N0")
      });

      var fileName = $"contract_{museumId}_{planId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
      using var memoryStream = new MemoryStream(pdfBytes);
      var formFile = new FormFile(memoryStream, 0, pdfBytes.Length, "file", fileName)
      {
        Headers = new HeaderDictionary(),
        ContentType = "application/pdf"
      };

      _logger.LogInformation("Uploading contract PDF to Cloudinary");
      var cloudinaryUrl = await _cloudinaryService.UploadFileAsync(formFile);

      return SuccessResp.Ok(new { Url = cloudinaryUrl });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating contract");
      throw new Exception($"Error generating contract: {ex.Message}");
    }
  }

  public byte[] GenerateContract(ContractData data)
  {
    using var stream = new MemoryStream();
    var writer = new PdfWriter(stream);
    var pdf = new PdfDocument(writer);
    var document = new Document(pdf);

    // Set up fonts
    SetupFonts();

    // Set margins
    document.SetMargins(40, 40, 40, 40);

    // Add content
    AddTitle(document);
    AddIntroduction(document, data);
    AddPartyA(document);
    AddPartyB(document, data);
    AddAgreementText(document);
    AddArticle1(document);
    AddArticle2(document, data);
    AddArticle3(document, data);
    AddArticle4(document);
    AddArticle5(document);
    AddSignatures(document, data);
    AddDisclaimer(document);

    document.Close();
    return stream.ToArray();
  }

  private void SetupFonts()
  {
    try
    {
      string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts");
      normalFont = PdfFontFactory.CreateFont(Path.Combine(fontPath, "NotoSans-Regular.ttf"), PdfEncodings.IDENTITY_H);
      boldFont = PdfFontFactory.CreateFont(Path.Combine(fontPath, "NotoSans-Bold.ttf"), PdfEncodings.IDENTITY_H);
    }
    catch
    {
      // Fallback
      normalFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN, PdfEncodings.IDENTITY_H);
      boldFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD, PdfEncodings.IDENTITY_H);
    }
  }

  private void AddTitle(Document document)
  {
    var title = new Paragraph("HỢP ĐỒNG DỊCH VỤ MUSETRIP360")
        .SetFont(boldFont)
        .SetFontSize(24)
        .SetFontColor(blueColor)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetMarginBottom(30);

    document.Add(title);
  }

  private void AddIntroduction(Document document, ContractData data)
  {
    var intro = new Paragraph()
        .Add("Hợp đồng này được lập vào ngày ")
        .Add(new Text(data.DateSigned).SetFont(boldFont).SetFontColor(orangeColor))
        .Add(", giữa:")
        .SetFont(normalFont)
        .SetFontSize(12);

    document.Add(intro);
  }

  private void AddPartyA(Document document)
  {
    var header = CreateSectionHeader("Bên A: NHÀ CUNG CẤP DỊCH VỤ (MuseTrip360)");
    document.Add(header);

    document.Add(CreateBoldValuePair("Tên công ty:", "MuseTrip360"));
    document.Add(CreateBoldValuePair("Địa chỉ:", "Đại học FPT Hồ Chí Minh"));
    document.Add(CreateBoldValuePair("Điện thoại:", "0901469725"));
    document.Add(CreateBoldValuePair("Email:", "thachlnse173476@fpt.edu.vn"));
    document.Add(CreateBoldValuePair("Đại diện bởi:", "Lê Ngọc Thạch"));
  }

  private void AddPartyB(Document document, ContractData data)
  {
    var header = CreateSectionHeader("Bên B: KHÁCH HÀNG (Bảo tàng)");
    document.Add(header);

    document.Add(CreateParameterValuePair("Tên Bảo tàng:", data.MuseumName));
    document.Add(CreateParameterValuePair("Địa chỉ:", data.MuseumAddress));
    document.Add(CreateParameterValuePair("Điện thoại:", data.MuseumPhone));
    document.Add(CreateParameterValuePair("Email:", data.MuseumEmail));
    document.Add(CreateParameterValuePair("Đại diện bởi:", $"Ông/Bà {data.ManagerName}"));
    document.Add(CreateBoldValuePair("Chức vụ:", "Quản lý Bảo tàng"));
  }

  private void AddAgreementText(Document document)
  {
    var agreement = new Paragraph("Hai bên đồng ý ký kết Hợp đồng dịch vụ MuseTrip360 với các điều khoản và điều kiện sau:")
        .SetFont(normalFont)
        .SetFontSize(12)
        .SetMarginTop(15);

    document.Add(agreement);
  }

  private void AddArticle1(Document document)
  {
    var header = CreateSectionHeader("ĐIỀU 1: ĐỐI TƯỢNG HỢP ĐỒNG");
    document.Add(header);

    var intro = new Paragraph("Bên A đồng ý cung cấp và Bên B đồng ý sử dụng hệ thống quản lý bảo tàng MuseTrip360 (sau đây gọi tắt là \"Hệ thống\") bao gồm các tính năng chính sau:")
        .SetFont(normalFont)
        .SetFontSize(12);
    document.Add(intro);

    var list = new List()
        .SetSymbolIndent(12)
        .SetListSymbol("•");

    list.Add(CreateListItem("Quản lý hiện vật, sự kiện, tour tham quan ảo."));
    list.Add(CreateListItem("Hệ thống bán vé trực tuyến."));
    list.Add(CreateListItem("Quản lý người dùng và phân quyền (RBAC)."));
    list.Add(CreateListItem("Tính năng tìm kiếm và gợi ý cá nhân hóa."));
    list.Add(CreateListItem("Các tính năng khác theo thỏa thuận."));

    document.Add(list);
  }

  private void AddArticle2(Document document, ContractData data)
  {
    var header = CreateSectionHeader("ĐIỀU 2: THỜI GIAN VÀ HIỆU LỰC HỢP ĐỒNG");
    document.Add(header);

    var timeClause = new Paragraph()
        .Add("Hợp đồng này có hiệu lực từ ngày ")
        .Add(new Text(data.StartDate).SetFont(boldFont).SetFontColor(orangeColor))
        .Add(" đến ngày ")
        .Add(new Text(data.EndDate).SetFont(boldFont).SetFontColor(orangeColor))
        .Add(".")
        .SetFont(normalFont)
        .SetFontSize(12);

    document.Add(timeClause);

    var extension = new Paragraph("Sau khi hết hạn, hai bên có thể xem xét gia hạn hợp đồng bằng văn bản thỏa thuận.")
        .SetFont(normalFont)
        .SetFontSize(12);

    document.Add(extension);
  }

  private void AddArticle3(Document document, ContractData data)
  {
    var header = CreateSectionHeader("ĐIỀU 3: PHÍ DỊCH VỤ VÀ PHƯƠNG THỨC THANH TOÁN");
    document.Add(header);

    var feeClause = new Paragraph()
        .Add(new Text("3.1. Phí dịch vụ: ").SetFont(boldFont))
        .Add("Tổng phí dịch vụ sử dụng Hệ thống MuseTrip360 là ")
        .Add(new Text(data.SystemFee).SetFont(boldFont).SetFontColor(orangeColor))
        .Add(" VNĐ.")
        .SetFont(normalFont)
        .SetFontSize(12);

    document.Add(feeClause);

    document.Add(CreateBoldValuePair("3.2. Phương thức thanh toán:", "[Chuyển khoản / Tiền mặt / ...]"));
    document.Add(CreateBoldValuePair("3.3. Thời hạn thanh toán:", "[Ví dụ: Thanh toán 100% trong vòng 7 ngày kể từ ngày ký hợp đồng / Thanh toán theo từng đợt ...]"));
  }

  private void AddArticle4(Document document)
  {
    var header = CreateSectionHeader("ĐIỀU 4: QUYỀN VÀ TRÁCH NHIỆM CỦA CÁC BÊN");
    document.Add(header);

    // 4.1 Party A responsibilities
    var subHeader1 = new Paragraph("4.1. Quyền và Trách nhiệm của Bên A (MuseTrip360):")
        .SetFont(boldFont)
        .SetFontSize(14)
        .SetFontColor(new DeviceRgb(74, 85, 104))
        .SetMarginTop(15);

    document.Add(subHeader1);

    var listA = new List().SetSymbolIndent(12).SetListSymbol("•");
    listA.Add(CreateListItem("Cung cấp và duy trì Hệ thống hoạt động ổn định."));
    listA.Add(CreateListItem("Hỗ trợ kỹ thuật cho Bên B trong quá trình sử dụng."));
    listA.Add(CreateListItem("Bảo mật dữ liệu của Bên B theo chính sách của Hệ thống."));
    listA.Add(CreateListItem("Có quyền tạm ngừng hoặc chấm dứt dịch vụ nếu Bên B vi phạm các điều khoản hợp đồng."));
    document.Add(listA);

    // 4.2 Party B responsibilities
    var subHeader2 = new Paragraph("4.2. Quyền và Trách nhiệm của Bên B (Bảo tàng):")
        .SetFont(boldFont)
        .SetFontSize(14)
        .SetFontColor(new DeviceRgb(74, 85, 104))
        .SetMarginTop(15);

    document.Add(subHeader2);

    var listB = new List().SetSymbolIndent(12).SetListSymbol("•");
    listB.Add(CreateListItem("Sử dụng Hệ thống đúng mục đích và tuân thủ các quy định của Bên A."));
    listB.Add(CreateListItem("Thanh toán phí dịch vụ đầy đủ và đúng hạn."));
    listB.Add(CreateListItem("Chịu trách nhiệm về nội dung và dữ liệu do mình đưa lên Hệ thống."));
    listB.Add(CreateListItem("Cung cấp thông tin cần thiết cho Bên A để triển khai và hỗ trợ dịch vụ."));
    document.Add(listB);
  }

  private void AddArticle5(Document document)
  {
    var header = CreateSectionHeader("ĐIỀU 5: ĐIỀU KHOẢN CHUNG");
    document.Add(header);

    document.Add(new Paragraph("5.1. Bất kỳ sửa đổi hoặc bổ sung nào đối với Hợp đồng này phải được lập thành văn bản và có chữ ký của cả hai bên.")
        .SetFont(normalFont).SetFontSize(12));

    document.Add(new Paragraph("5.2. Mọi tranh chấp phát sinh từ hoặc liên quan đến Hợp đồng này sẽ được giải quyết bằng thương lượng. Nếu không giải quyết được bằng thương lượng, tranh chấp sẽ được đưa ra Tòa án có thẩm quyền để giải quyết.")
        .SetFont(normalFont).SetFontSize(12));

    document.Add(new Paragraph("5.3. Hợp đồng này được lập thành 02 (hai) bản có giá trị pháp lý như nhau, mỗi bên giữ 01 (một) bản.")
        .SetFont(normalFont).SetFontSize(12));
  }

  private void AddSignatures(Document document, ContractData data)
  {
    document.Add(new Paragraph("").SetMarginTop(40)); // Space

    var signatureTable = new Table(2).UseAllAvailableWidth();

    // Party A signature
    var partyACell = new Cell()
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

    partyACell.Add(new Paragraph("ĐẠI DIỆN BÊN A")
        .SetFont(boldFont)
        .SetFontColor(blueColor)
        .SetFontSize(12));

    partyACell.Add(new Paragraph("(NHÀ CUNG CẤP DỊCH VỤ)")
        .SetFont(normalFont)
        .SetFontSize(11));

    partyACell.Add(new Paragraph("").SetMarginTop(40)); // Signature space

    partyACell.Add(new Paragraph("Lê Ngọc Thạch")
        .SetFont(normalFont)
        .SetFontSize(12));

    // Party B signature
    var partyBCell = new Cell()
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

    partyBCell.Add(new Paragraph("ĐẠI DIỆN BÊN B")
        .SetFont(boldFont)
        .SetFontColor(blueColor)
        .SetFontSize(12));

    partyBCell.Add(new Paragraph("(QUẢN LÝ BẢO TÀNG)")
        .SetFont(normalFont)
        .SetFontSize(11));

    partyBCell.Add(new Paragraph("").SetMarginTop(40)); // Signature space

    partyBCell.Add(new Paragraph(data.ManagerName)
        .SetFont(normalFont)
        .SetFontSize(12));

    signatureTable.AddCell(partyACell);
    signatureTable.AddCell(partyBCell);

    document.Add(signatureTable);
  }

  private void AddDisclaimer(Document document)
  {
    var disclaimer = new Paragraph("*Lưu ý: Đây là một mẫu hợp đồng đơn giản. Bạn nên tham khảo ý kiến chuyên gia pháp lý để đảm bảo hợp đồng phù hợp với tất cả các yêu cầu pháp luật hiện hành.*")
        .SetFont(normalFont)
        .SetFontSize(10)
        .SetFontColor(grayColor)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetMarginTop(30);

    document.Add(disclaimer);
  }

  private Paragraph CreateSectionHeader(string text)
  {
    return new Paragraph(text)
        .SetFont(boldFont)
        .SetFontSize(16)
        .SetFontColor(blueColor)
        .SetMarginTop(20)
        .SetMarginBottom(10)
        .SetBorderBottom(new iText.Layout.Borders.SolidBorder(new DeviceRgb(235, 244, 255), 2))
        .SetPaddingBottom(5);
  }

  private Paragraph CreateBoldValuePair(string label, string value)
  {
    return new Paragraph()
        .Add(new Text(label).SetFont(boldFont))
        .Add($" {value}")
        .SetFont(normalFont)
        .SetFontSize(12)
        .SetMarginBottom(5);
  }

  private Paragraph CreateParameterValuePair(string label, string value)
  {
    return new Paragraph()
        .Add(new Text(label).SetFont(boldFont))
        .Add(" ")
        .Add(new Text(value).SetFont(boldFont).SetFontColor(orangeColor))
        .SetFont(normalFont)
        .SetFontSize(12)
        .SetMarginBottom(5);
  }

  private ListItem CreateListItem(string text)
  {
    return (ListItem)new ListItem(text)
        .SetFont(normalFont)
        .SetFontSize(12)
        .SetMarginBottom(5);
  }
}