using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Services;

public interface IRouteService
{
    Task<int> CalculateAllTripsDistancesAsync(List<(double Latitude, double Longitude)> coordinates);
}