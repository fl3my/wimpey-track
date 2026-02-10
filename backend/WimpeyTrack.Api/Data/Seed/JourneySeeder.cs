using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Data.Seed;

public static class JourneySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IWebHostEnvironment env)
    {
        if (context.Journeys.Any())
            return;
        
        var path = Path.Combine(
            env.ContentRootPath,
            "seed",
            "current-miles.tsv"
        );
        
        
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
            HeaderValidated = null,
            MissingFieldFound = null
        });
        
        var records = csv.GetRecords<JourneyTsvRow>();

        var homeLocation = await context.Locations.FirstAsync();
        
        var journeys = records
            .Select(r => new  Journey
            {
                Date = r.Date,
                TotalMiles = r.TotalMiles,
                IsManualMiles = true,
                HomeLocation = homeLocation,
            })
            .ToList();

        await context.Journeys.AddRangeAsync(journeys);
        await context.SaveChangesAsync();
    }
    
    private class JourneyTsvRow
    {
        [Name("date")]
        public DateOnly Date { get; set; }
        
        [Name("total_miles")]
        public  int TotalMiles { get; set; }
        
    }
}