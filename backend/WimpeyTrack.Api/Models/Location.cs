using System.Text.Json.Serialization;

namespace WimpeyTrack.Api.Models;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    [JsonIgnore]
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}