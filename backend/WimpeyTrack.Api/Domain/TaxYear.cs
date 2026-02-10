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
    
    public static DateOnly GetTaxMonthStart(DateOnly date)
    {
        if (date.Day >= 20)
            return new DateOnly(date.Year, date.Month, 20);

        var previous = date.AddMonths(-1);
        return new DateOnly(previous.Year, previous.Month, 20);
    }

    public IReadOnlyList<DateOnly> GetTaxMonthsUpToToday()
    {
        var months = new List<DateOnly>();

        // first tax month that overlaps this tax year
        var cursor = GetTaxMonthStart(Start);
        var end = CurrentEnd();

        while (cursor <= end)
        {
            months.Add(cursor);
            cursor = cursor.AddMonths(1);
        }

        return months;
    }
}