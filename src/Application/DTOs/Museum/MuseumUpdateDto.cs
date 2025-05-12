namespace Application.DTOs.Museum;

using System.ComponentModel.DataAnnotations;
using System;
using Application.Shared.Enum;
using System.Text.Json;

public class MuseumUpdateDto
{
  public string? Name { get; set; }

  public string? Description { get; set; }

  public string? Location { get; set; }

  public string? ContactEmail { get; set; }

  public string? ContactPhone { get; set; }

  public MuseumStatusEnum? Status { get; set; }

  public JsonDocument? Metadata { get; set; }
}