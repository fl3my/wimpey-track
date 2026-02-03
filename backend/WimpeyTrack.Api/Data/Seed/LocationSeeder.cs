using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Data.Seed;

public static class LocationSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IWebHostEnvironment env)
    {
        if (context.Locations.Any())
            return;
        
        var path = Path.Combine(
            env.ContentRootPath,
            "app",
            "seed",
            "central-belt-locations.tsv"
        );
        
        
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
            HeaderValidated = null,
            MissingFieldFound = null
        });
        
        var records = csv.GetRecords<LocationTsvRow>();

        var locations = records
            .Where(r => !string.IsNullOrWhiteSpace(r.Name))
            .Select(r => new Location()
            {
                Name = r.Name.Trim(),
                Latitude = r.Lat,
                Longitude = r.Lon
            })
            .ToList();

        await context.Locations.AddRangeAsync(locations);
        await context.SaveChangesAsync();
    }
    
    public class LocationTsvRow
    {
        [Name("name")]
        public string Name { get; set; } = null!;
        
        [Name("@lat")]
        public double Lat { get; set; }
        
        [Name("@lon")]
        public double Lon { get; set; }
    }
}