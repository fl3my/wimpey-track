namespace WimpeyTrack.Api.Data.Seed;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        await PreferenceSeeder.SeedAsync(context);
        await LocationSeeder.SeedAsync(context, env);
    }
}