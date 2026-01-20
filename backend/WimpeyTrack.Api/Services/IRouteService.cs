using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Services;

public interface IRouteService
{
    Task<double> CalculateAllTripsDistancesAsync(List<(double Latitude, double Longitude)> coordinates);
}