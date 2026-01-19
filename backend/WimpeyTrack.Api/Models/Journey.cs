namespace WimpeyTrack.Api.Models;

public class Journey
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int TotalMiles { get; set; }
    public bool IsManualMiles { get; set; }
    public int HomeLocationId { get; set; }
    public Location HomeLocation { get; set; } = null!;
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}