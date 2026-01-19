namespace WimpeyTrack.Api.Models;

public class Trip
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public int ReasonId { get; set; }
    public Reason Reason { get; set; } = null!;
    public int JourneyId { get; set; }
    public Journey Journey { get; set; } = null!;
}