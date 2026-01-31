
using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Journey;

public class JourneyByWeekDto
{
    [Required]
    public DateOnly WeekStart { get; set; }
    
    [Required]
    public DateOnly PrevWeekStart { get; set; }
    
    [Required]
    public DateOnly NextWeekStart { get; set; }
    
    [Required]
    public List<JourneyDayDto> Days { get; set; } = [];
}