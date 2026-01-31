using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Journey;

public class JourneyDayDto
{
    [Required]
    public DateOnly Date { get; set; }
    
    [Required]
    public JourneyDto? Journey { get; set; }
}