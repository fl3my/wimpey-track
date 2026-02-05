using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;

namespace WimpeyTrack.Api.Services;

/// <summary>
/// Recalculate the miles for a journey using routing service.
/// </summary>
public interface IJourneyDistanceService
{
    Task RecalculateMilesAsync(int journeyId);
}

public class JourneyDistanceService : IJourneyDistanceService
{
    private readonly ApplicationDbContext _context;
    private readonly IRouteService _routeService;
    private readonly IPreferenceProvider _preferences;

    public JourneyDistanceService(ApplicationDbContext context, IRouteService routeService, IPreferenceProvider preferences)
    {
        _context = context;
        _routeService = routeService;
        _preferences = preferences;
    }

    public async Task RecalculateMilesAsync(int journeyId)
    {
        // Get journey
        var journey = await _context.Journeys
            .Include(j => j.HomeLocation)
            .FirstOrDefaultAsync(j => j.Id == journeyId);

        if (journey == null || journey.IsManualMiles)
        {
            return;
        }
        
        // get a list of coordinates
        var coordinates = await _context.Trips
            .Where(t => t.JourneyId == journeyId)
            .OrderBy(t => t.Id)
            .Select(t => new
            {
                t.Location.Latitude,
                t.Location.Longitude
            }).ToListAsync();

        // if no coordinates, return 0
        if (!coordinates.Any())
        {
            journey.TotalMiles = 0;
            await _context.SaveChangesAsync();
            return;
        }

        // Create a list of coordinates in order
        var home = (journey.HomeLocation.Latitude, journey.HomeLocation.Longitude);
        var route = new List<(double, double)> { home };
        route.AddRange(coordinates.Select(c => (c.Latitude, c.Longitude)));
        route.Add(home);
        
        var distance = await _routeService.CalculateAllTripsDistancesAsync(route);

        // Alter the percentage of the returned distance to reflect more accurate distance
        var adjustmentFactor = await _preferences.GetMilesAdjustmentFactorAsync();
        
        journey.TotalMiles = (int)Math.Round(distance * adjustmentFactor, 0);
        
        await _context.SaveChangesAsync();
    }
}