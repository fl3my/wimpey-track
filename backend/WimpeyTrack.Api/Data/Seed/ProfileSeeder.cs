using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Data.Seed;

public static class ProfileSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        
        if (context.Profiles.Any())
            return;

        // Get the first location, user can change later
        var homeLocation = await context.Locations.FirstAsync();
        
        var profile = new Profile
        {
            Id = 1,
            FullName = "",
            StaffNumber = "",
            BusinessUnit = "",
            DepartmentSiteName = "",
            VehicleFuelType = "",
            VehicleEngineSize = 0,
            VehicleRegistration = "",
            VehicleMake = "",
            HomePostcode = "",
            HomeLocationId = homeLocation.Id,
        };
        
        context.Profiles.Add(profile);
        await context.SaveChangesAsync();
    }
}