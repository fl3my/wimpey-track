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
        var journeys = await _context.Journeys
            .Where(j => j.Date >= taxYear.Start && j.Date <= taxYear.End)
            .Select(j => new
            {
                Date = j.Date,
                TotalMiles = j.TotalMiles
            })
            .ToListAsync();

        var monthlyData = journeys
            .GroupBy(j => TaxYear.GetTaxMonthStart(j.Date))
            .Select(g => new
            {
                TaxMonthStart = g.Key,
                Miles = g.Sum(x => x.TotalMiles)
            })
            .OrderBy(x => x.TaxMonthStart)
            .ToList();
        
        // Get list for full year
        var taxMonths = taxYear.GetTaxMonthsUpToToday();
        var remainingHighRateMiles = MileageRateThreshold;
        
        var monthlyMiles = taxMonths.Select(taxMonth =>
        {
            var miles = monthlyData
                .FirstOrDefault(d => d.TaxMonthStart == taxMonth)?.Miles ?? 0;

            var highRateMiles = Math.Min(miles, remainingHighRateMiles);
            var lowRateMiles = miles - highRateMiles;

            var claim =
                (highRateMiles * 0.45m) +
                (lowRateMiles * 0.25m);

            remainingHighRateMiles -= highRateMiles;
            return new MonthlyMilesDto()
            {
                Month = CultureInfo.InvariantCulture.DateTimeFormat
                    .GetAbbreviatedMonthName(taxMonth.AddMonths(1).Month),
                Miles = miles,
                Claim = claim
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
    
    
    private static string TaxMonthLabel(int taxMonthStartMonth)
    {
        var labelMonth = taxMonthStartMonth == 12 ? 1 : taxMonthStartMonth + 1;

        return CultureInfo.InvariantCulture
            .DateTimeFormat
            .GetAbbreviatedMonthName(labelMonth);
    }
}