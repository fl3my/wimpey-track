using System.Text.Json.Serialization;

namespace WimpeyTrack.Api.Models;

public class Reason
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [JsonIgnore]
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}