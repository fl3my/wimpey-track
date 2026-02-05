using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.Book;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Domain;

/// <summary>
/// Builds the expense books for a reporting period by grouping journeys according
/// to mileage threshold changes ad attaching purchases withing each books date
/// range.
/// </summary>
public interface IBookBuilder
{
    Task<IReadOnlyList<Book>> BuildAsync(DateOnly startDate, DateOnly endDate, BookProfile profile);
}

public class BookBuilder : IBookBuilder
{
    private readonly ApplicationDbContext _context;

    private const int Threshold = 10000;
    private const int MaxLineLength = 60;
    private const int MaxRowLines = 30;
        
    public BookBuilder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Book>> BuildAsync(DateOnly startDate, DateOnly endDate, BookProfile profile)
    {
        // Load the journeys in the date range
        var journeys = await _context.Journeys
            .Where(j => j.Date >= startDate && j.Date <= endDate)
            .Include(j => j.HomeLocation)
            .Include(j => j.Trips)
            .ThenInclude(t => t.Location)
            .Include(j => j.Trips)
            .ThenInclude(t => t.Reason)
            .OrderBy(j => j.Date)
            .ToListAsync();

        // If no journeys return an empty book
        if (journeys.Count == 0) return Array.Empty<Book>();

        // Load the purchases in the date range
        var purchases = await _context.Purchases
            .Where(p => p.Date >= startDate && p.Date <= endDate)
            .Include(p => p.Items)
            .OrderBy(p => p.Date)
            .ToListAsync();

        // Pre-calculate the threshold per date
        var thresholdLookup = await BuildThresholdLookupAsync(journeys);

        // Group journeys into groups
        var journeyGroups = GroupJourneysByThreshold(journeys, thresholdLookup);

        // Build the books
        var books = new List<Book>();

        for (int i = 0; i < journeyGroups.Count; i++)
        {
            var currentGroup = journeyGroups[i];

            // Get the start and end dates of the group
            var groupStart = currentGroup.First().Date;
            var groupEnd = currentGroup.Last().Date;

            DateOnly purchaseStart;
            DateOnly purchaseEnd;
            
            if (i == 0)
            {
                // First book
                purchaseStart = startDate;
            }
            else
            {
                // IF not first book
                purchaseStart = journeyGroups[i - 1].Last().Date.AddDays(1);
            }

            if (i == journeyGroups.Count - 1)
            {
                // If last book
                purchaseEnd = endDate;
            }
            else
            {
                // If not last book
                purchaseEnd = groupEnd;
            }
            
            // Add books
            books.Add(new Book
            {
                IsOverThreshold = thresholdLookup[groupStart],
                PurchaseRows = BuildPurchaseRows(
                    purchases,
                    purchaseStart,
                    purchaseEnd),
                Sheets = BuildSheets(currentGroup),
                BookProfile =  profile
            });
        }

        return books;
    }

    private List<Sheet> BuildSheets(IReadOnlyList<Journey> journeys)
    {
        var sheets = new List<Sheet>();
        var currentSheet = new Sheet();

        foreach (var journey in journeys)
        {
            // Get the home location name
            var homeLocationName = journey.HomeLocation.Name;

            // Dynamically add trips
            var tripsCombined = string.Join(" > ", journey.Trips
                .OrderBy(t => t.Id)
                .Select(t => $"{t.Location.Name} {t.Reason.Name}")
            );

            // Construct the line, If over line limit split
            var description = $"{homeLocationName} > {tripsCombined} > {homeLocationName}";
            var descriptionLines = SplitIntoLinesByCharacter(description, MaxLineLength);

            // Construct a row
            var row = new Row()
            {
                Date = journey.Date.ToString("yyyy-MM-dd"),
                Description = descriptionLines,
                TotalMiles = journey.TotalMiles,
            };
            
            // Create a new sheet if line count goes over max rows
            var lineCount = currentSheet.Rows.Sum(r => r.Description.Count);
            if (lineCount + descriptionLines.Count > MaxRowLines && sheets.Count == 0)
            {
                sheets.Add(currentSheet);
                currentSheet = new Sheet();
            }

            currentSheet.Rows.Add(row);
        }

        // If any rows are left add to the sheet
        if (currentSheet.Rows.Count > 0)
        {
            sheets.Add(currentSheet);
        }

        return sheets;
    }

    private static List<string> SplitIntoLinesByCharacter(string text, int maxLength)
    {
        var result = new List<string>();
        var remaining = text;

        // While there is still text remianing
        while (remaining.Length > 0)
        {
            // If the text is less than max length return
            if (remaining.Length <= maxLength)
            {
                result.Add(remaining);
                break;
            }
            
            var chunk = remaining[..maxLength];
            var lastSpace = chunk.LastIndexOf(' ');
            
            // Find last white space before max length
            if (lastSpace > 0)
            {
                result.Add(remaining[..lastSpace]);
                remaining = remaining[(lastSpace + 1)..];
            }
            else
            {
                result.Add(remaining[..maxLength]);
                remaining = remaining[..maxLength];
            }
        }

        return result;
    }

    private ICollection<PurchaseRow> BuildPurchaseRows(IReadOnlyList<Purchase> purchases, DateOnly start, DateOnly end)
    {
        // Return a list of Purchase row models
        return purchases
            .Where(p => p.Date >= start && p.Date <= end && p.Items.Count > 0)
            .SelectMany(p => p.Items,
                (p, i) => new PurchaseRow
                {
                    ExpenseCode = "O",
                    Date = p.Date.ToString("yyyy-MM-dd"),
                    ExpenseDetail =
                        $"{p.StoreName} - {i.Name} *{i.Quantity}",
                    ReasonForExpense = i.Reason,
                    VatCode = "P10",
                    ReceiptAttached = true,
                    Cost = i.Cost * i.Quantity
                }).ToList();
    }

    private IReadOnlyList<List<Journey>> GroupJourneysByThreshold(IReadOnlyList<Journey> journeys,
        IDictionary<DateOnly, bool> thresholdLookup)
    {
        // Create a list of lists to store the grouped journeys
        var result = new List<List<Journey>>();

        // Create a current list
        var current = new List<Journey>();

        bool? currentThreshold = null;
        foreach (var journey in journeys)
        {
            // Get if the journey is over the threshold
            var threshold = thresholdLookup[journey.Date];

            // If it is the first iteration, set the current threshold
            if (currentThreshold == null)
            {
                currentThreshold = threshold;
            }
            else if (currentThreshold != threshold)
            {
                // If the threshold changes, add the current list to the results
                result.Add(current);

                // And create a new list
                current = new List<Journey>();

                // And set tht new current threshold
                currentThreshold = threshold;
            }

            // Add a journey to the current list
            current.Add(journey);
        }

        // Add remaining journeys list to result if threshold doesnt change
        if (current.Count > 0)
            result.Add(current);

        return result;
    }

    private async Task<Dictionary<DateOnly, bool>> BuildThresholdLookupAsync(IReadOnlyList<Journey> journeys)
    {
        var result = new Dictionary<DateOnly, bool>();

        // Prefetch all journey dates
        var journeyDates = journeys.Select(j => j.Date).Distinct();
        foreach (var date in journeyDates)
            // Fill a dictionary with a bool representing if over threshold
            result[date] = await CalculateOverThresholdAsync(date);

        return result;
    }

    private async Task<bool> CalculateOverThresholdAsync(DateOnly claimDate)
    {
        // Calculate the tax year depending on the claim date
        var taxYear = TaxYear.FromDate(claimDate);

        // Sum the miles from the claim date to the tax year start
        var miles = await _context.Journeys
            .Where(j => j.Date >= taxYear.Start && j.Date <= claimDate)
            .SumAsync(j => j.TotalMiles);

        // Return true if over threshold
        return miles > Threshold;
    }
}