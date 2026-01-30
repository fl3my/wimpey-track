namespace WimpeyTrack.Api.Domain;

public sealed class TaxYear
{
    public DateOnly Start { get; }
    public DateOnly End { get; }
    public int StartYear { get; }

    private TaxYear(int startYear)
    {
        StartYear = startYear;
        Start = new DateOnly(startYear, 4, 6);
        End = Start.AddYears(1).AddDays(-1);
    }

    public static TaxYear Current()
    {
        var now = DateTime.UtcNow;
        
        var startYear = now.Month > 4 || (now.Month == 4 && now.Day >= 6)
            ? now.Year
            : now.Year - 1;

        return new TaxYear(startYear);
    }
    
    public static TaxYear FromDate(DateOnly date)
    {
        var startYear =
            date.Month > 4 || (date.Month == 4 && date.Day >= 6)
                ? date.Year
                : date.Year - 1;

        return new TaxYear(startYear);
    }

    private DateOnly CurrentEnd()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return today < End ? today : End;
    }
    
    public IReadOnlyList<int> GetReportingMonthsUpToToday()
    {
        var months = new List<int>();
        var cursor = Start;
        var end = CurrentEnd();

        while (cursor <= end)
        {
            months.Add(cursor.Month);
            cursor = cursor.AddMonths(1);
        }

        return months;
    }
}