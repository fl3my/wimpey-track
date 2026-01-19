namespace WimpeyTrack.Api.Dtos;

public class JourneyDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int TotalMiles { get; set; }
    public bool IsManualMiles { get; set; }
    public int HomeLocationId { get; set; }
}