using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Item;

public class UpdateItemDto
{
    [Required]
    public int Id { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
    public string Name { get; set; } = string.Empty;
    [Required]
    [Range(0.01, 10, ErrorMessage = "Quantity must be between 1 and 10")]
    public int Quantity { get; set; }
    [Required]
    [Range(0.01, 100, ErrorMessage = "Quantity must be between 0.01 and 100")]
    public double Cost { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
    public string Reason { get; set; } = string.Empty;
}