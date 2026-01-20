using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos;

public class CreateJourneyDto
{
    [Required]
    public DateOnly Date { get; set; }
    
    public int? TotalMiles { get; set; }
    
    [Required]
    public bool IsManualMiles { get; set; }
    
    [Required]
    public int HomeLocationId { get; set; }
}