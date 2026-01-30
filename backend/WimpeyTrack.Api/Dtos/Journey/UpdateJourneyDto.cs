using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Journey;

public class UpdateJourneyDto
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [Range(typeof(DateOnly), "2024-04-06", "2034-04-06")]
    public DateOnly Date { get; set; }
    
    [Required]
    [Range(1, 300)]
    public int TotalMiles { get; set; }
    
    [Required]
    public bool IsManualMiles { get; set; }
    
    [Required]
    public int HomeLocationId { get; set; }
}