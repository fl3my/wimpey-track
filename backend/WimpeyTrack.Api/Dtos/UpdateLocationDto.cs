using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos;

public class UpdateLocationDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public double Latitude { get; set; }
    
    [Required]
    public double Longitude { get; set; }
}