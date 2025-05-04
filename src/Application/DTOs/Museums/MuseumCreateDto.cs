using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Museums;

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

  [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
  public double Rating { get; set; }

  public Guid CreatedBy { get; set; }

  [Required(ErrorMessage = "Status is required")]
  public string Status { get; set; }
}