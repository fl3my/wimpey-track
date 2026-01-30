using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Domain;
using WimpeyTrack.Api.Dtos.Dashboard;

namespace WimpeyTrack.Api.Services;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync();
}

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    private const int MileageRateThreshold = 10000;
    
    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var monthlyMiles = await GetMonthlyMilesAsync();
        var cumulativeMiles = BuildCumulativeMonthlyMiles(monthlyMiles);
        
        var totalMileage = cumulativeMiles.LastOrDefault()?.Miles ?? 0;
        var milesThisMonth = monthlyMiles.LastOrDefault()?.Miles ?? 0;
        
        var response = new DashboardResponse()
        {
            Summary = new DashboardSummary()
            {
                TotalMileageThisTaxYear = totalMileage,
                TotalClaimedThisTaxYear = CalculateClaim(totalMileage),
                TotalClaimedThisMonth = CalculateClaim(milesThisMonth),
            },
            MonthlyMiles = monthlyMiles,
            CumulativeMiles = cumulativeMiles
        };

        return response;
    }

    private async Task<List<MonthlyMilesDto>> GetMonthlyMilesAsync()
    {
        var taxYear = TaxYear.Current();
        
        // Get the month data by number
        var monthlyData = await _context.Journeys
            .Where(j => j.Date >= taxYear.Start && j.Date <= taxYear.End)
            .GroupBy(j => j.Date.Day >= 20 
                ? j.Date.Month 
                : j.Date.Month == 1 ? 12: j.Date.Month - 1)
            .Select(g => new
            {
                MonthNumber = g.Key,
                Miles = g.Sum(j => j.TotalMiles)
            })
            .OrderBy(x => x.MonthNumber < 4 ? x.MonthNumber + 12 : x.MonthNumber)
            .ToListAsync();

        // Get list for full year
        var reportingMonths = taxYear.GetReportingMonthsUpToToday();
        var monthlyMiles = reportingMonths.Select(m =>
        {
            var miles = monthlyData.FirstOrDefault(d => d.MonthNumber == m)?.Miles ?? 0;
            
            return new MonthlyMilesDto()
            {
                Month = MonthLabel(m),
                Miles = miles,
                Claim = CalculateClaim(miles)
            };
        }).ToList();
        
        return monthlyMiles;
    }
    
    private static List<MonthlyMilesDto> BuildCumulativeMonthlyMiles(List<MonthlyMilesDto> monthlyMiles)
    {
        var runningTotal = 0;
        
        return monthlyMiles.Select(m =>
        {
            runningTotal += m.Miles;
            return new MonthlyMilesDto()
            {
                Month = m.Month,
                Miles = runningTotal
            };
        }).ToList();
    }
    
    private static decimal CalculateClaim(int miles)
    {
        const decimal highRate = 0.45m;
        const decimal lowRate = 0.25m;

        if (miles <= MileageRateThreshold)
            return miles * highRate;

        var highRateMiles = MileageRateThreshold;
        var lowRateMiles = miles - MileageRateThreshold;

        return (highRateMiles * highRate) + (lowRateMiles * lowRate);
    }
    
    private static string MonthLabel(int monthNumber)
    {
        return CultureInfo
            .InvariantCulture
            .DateTimeFormat
            .GetAbbreviatedMonthName(monthNumber);
    }
}