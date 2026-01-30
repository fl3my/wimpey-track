using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Journey;

public class UpdateJourneyDto
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public DateOnly Date { get; set; }
    
    public int? TotalMiles { get; set; }
    
    [Required]
    public bool IsManualMiles { get; set; }
    
    [Required]
    public int HomeLocationId { get; set; }
}