using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Location;

public class UpdateLocationDto
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Range(55.3669, 56.1059, ErrorMessage="Latitude must be between 55.3669 and 56.1059")]
    public double Latitude { get; set; }
    
    [Required]
    [Range(-4.9821, -2.6953, ErrorMessage = "Longitude must be between -4.9821 and -2.6953")]
    public double Longitude { get; set; }
}