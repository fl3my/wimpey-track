
namespace WimpeyTrack.Api.Dtos.Journey;

public class JourneyByWeekDto
{
    public ICollection<JourneyDto> Journeys { get; set; } = new List<JourneyDto>();
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}