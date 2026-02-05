using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Data.Seed;

public static class PreferenceSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Preferences.Any())
            return;

        var prefs = new Preference()
        {
            Id = 1,
            MilesAdjustmentFactor = 1,
        };
        
        context.Preferences.Add(prefs);
        await context.SaveChangesAsync();
    }
}