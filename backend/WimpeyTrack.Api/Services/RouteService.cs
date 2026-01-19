using System.Text.Json;

namespace WimpeyTrack.Api.Services;

public class RouteService : IRouteService
{
    private readonly HttpClient _httpClient;
    
    public RouteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<int> CalculateAllTripsDistancesAsync(List<(double Latitude, double Longitude)> coordinates)
    {
        // If less than 3 coordinates, return 0
        if (coordinates.Count < 2)
        {
            return 0;
        }
        
        // Concatenate the coordinates together for the request
        var coordinatesString = string.Join(";", 
            coordinates.Select(coord => $"{coord.Longitude},{coord.Latitude}"));
        
        // Construct the query
        var url = $"route/v1/driving/{coordinatesString}?overview=false";
            
        // Make the request
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);
        
        // Extract the distance from the response
        var distance = json.RootElement
            .GetProperty("routes")[0]
            .GetProperty("distance")
            .GetDouble();
        
        // Convert meters to miles
        var distanceMiles = (int) Math.Round(distance * 0.000621371, 0);
        
        return distanceMiles;
    }
}