namespace Application.DTOs.Payment;

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Domain.Payment;

public class BankAccountDto
{
  public Guid Id { get; set; }
  public Guid MuseumId { get; set; }
  public string HolderName { get; set; } = null!;
  public string BankName { get; set; } = null!;
  public string AccountNumber { get; set; } = null!;
  public string QRCode { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public class BankAccountCreateDto
{
  [Required]
  [StringLength(100)]
  public string HolderName { get; set; } = null!;

  [Required]
  [StringLength(100)]
  public string BankName { get; set; } = null!;

  [Required]
  [StringLength(100)]
  public string AccountNumber { get; set; } = null!;

  [Required]
  [StringLength(1000)]
  public string QRCode { get; set; } = null!;
}

public class BankAccountUpdateDto
{
  [StringLength(100)]
  public string? HolderName { get; set; }

  [StringLength(100)]
  public string? BankName { get; set; }

  [StringLength(100)]
  public string? AccountNumber { get; set; }

  [StringLength(1000)]
  public string? QRCode { get; set; }
}

public class BankAccountProfile : Profile
{
  public BankAccountProfile()
  {
    CreateMap<BankAccount, BankAccountDto>();

    CreateMap<BankAccountCreateDto, BankAccount>()
      .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    CreateMap<BankAccountUpdateDto, BankAccount>()
      .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}