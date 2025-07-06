namespace Application.DTOs.Payment;

using System.Text.Json;
using Application.Shared.Enum;

public class CreateOrderReq
{
  public float TotalAmount { get; set; }

  public OrderTypeEnum OrderType { get; set; }

  public string? Metadata { get; set; }
  public List<Guid> ItemIds { get; set; } = [];
}

public class CreateOrderMsg
{
  public float TotalAmount { get; set; }

  public OrderTypeEnum OrderType { get; set; }

  public JsonDocument? Metadata { get; set; }
  public List<Guid> ItemIds { get; set; } = [];

  public Guid CreatedBy { get; set; }
}