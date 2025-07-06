namespace Application.DTOs.Museum;

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class MuseumCreateDto
{
  [Required(ErrorMessage = "Name is required")]
  public string Name { get; set; } = null!;

  [Required(ErrorMessage = "Description is required")]
  public string Description { get; set; } = null!;

  [Required(ErrorMessage = "Location is required")]
  public string Location { get; set; } = null!;

  [Required(ErrorMessage = "Contact email is required")]
  [EmailAddress(ErrorMessage = "Invalid email address")]
  public string ContactEmail { get; set; } = null!;

  [Required(ErrorMessage = "Contact phone is required")]
  [Phone(ErrorMessage = "Invalid phone number")]
  public string ContactPhone { get; set; } = null!;

  public decimal Latitude { get; set; }

  public decimal Longitude { get; set; }

  public JsonDocument? Metadata { get; set; }
}