namespace WimpeyTrack.Api.Dtos.Location;

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TripCount { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}