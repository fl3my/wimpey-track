using System.IO.Compression;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> DownloadReport(DateOnly startDate, DateOnly endDate)
        {
            // Retrieve the data
            var books = await CreateBooksAsync(startDate, endDate);

            if (books.Count == 0)
            {
                return BadRequest();
            }
            
            // Load template
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "template.xlsx");
            
            // For testing purposes, put xlsx in zip archive
            using var zipStream = new MemoryStream();

            await using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // For each book
                for (var bookIndex = 0; bookIndex < books.Count; bookIndex++)
                {
                    var book = books[bookIndex];
                    
                    // Create an instance of ClosedXML
                    using var workbook = new XLWorkbook(templatePath);

                    // Fill Claim rate on first sheet
                    var expenseSheet = workbook.Worksheet(1);
                    expenseSheet.Cell(53, 2).Value = book.IsOverThreshold ? 0.25 : 0.45;
                    expenseSheet.Cell(49, 2).Value = book.IsOverThreshold ? "YES" : "NO";

                    // Fill sheets
                    for (var sheetIndex = 0; sheetIndex < book.Sheets.Count; sheetIndex++)
                    {
                        // Get the mileage detail sheet
                        var sheet = book.Sheets.ElementAt(sheetIndex);
                        var worksheet = workbook.Worksheet(sheetIndex + 3);

                        var currentRow = 6;
                        
                        // Fill the rows
                        foreach (var row in sheet.Rows)
                        {
                            worksheet.Cell(currentRow, 1).Value = row.Date;
                            worksheet.Cell(currentRow, 5).Value = row.TotalMiles;

                            foreach (var description in row.Description)
                            {
                                worksheet.Cell(currentRow, 2).Value = description;
                                currentRow++;
                            }
                        }
                    }

                    // Save workbook to memory
                    using var bookStream = new MemoryStream();
                    workbook.SaveAs(bookStream);
                    bookStream.Position = 0;

                    // Add XLSX to ZIP
                    var entryName =
                        $"Mileage_Report_{bookIndex + 1}_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.xlsx";

                    // Add the file to the archive
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);

                    await using var entryStream = await entry.OpenAsync();
                    await bookStream.CopyToAsync(entryStream);
                }
            }

            zipStream.Position = 0;
                
            // Return a zip file
            return File(
                zipStream.ToArray(),
                "application/zip",
                $"Mileage_Reports_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.zip"
            );
        }

        private async Task<List<Book>> CreateBooksAsync(DateOnly startDate, DateOnly endDate)
        {
            // Get data from server
            var journeys = await _context.Journeys
                .Where(j => j.Date >= startDate && j.Date <= endDate)
                .Include(j => j.HomeLocation)
                .Include(j => j.Trips)
                .ThenInclude(t => t.Location)
                .Include(j => j.Trips)
                .ThenInclude(t => t.Reason)
                .OrderBy(j => j.Date)
                .ToListAsync();
            
            var books = new List<Book>();
            
            // Create a list of journeys in a buffer
            var bufferedJourneys = new List<Journey>();
  
            // Variable to hold previous state
            bool? previousOverThreshold = null;
            
            // Iterate over each journey
            foreach (var journey in journeys)
            {
                // Check if the current journey is over the threshold
                var currentOverThreshold = await CalculateOverThresholdAsync(journey.Date);
                
                // First journey loop
                if (previousOverThreshold == null)
                {
                    previousOverThreshold = currentOverThreshold;
                
                // The claim rate has changed, so previous book msut be finished
                } else if (previousOverThreshold != currentOverThreshold)
                {
                    // Create the book at curent claim rate with buffered journeys
                    books.Add(new Book()
                    {
                        IsOverThreshold = previousOverThreshold.Value,
                        Sheets = BuildSheets(bufferedJourneys),
                    });
                    
                    // Clear the buffered journeys
                    bufferedJourneys.Clear();
                    
                    // Update state
                    previousOverThreshold = currentOverThreshold;
                }
                
                // Add to the buffered journeys
                bufferedJourneys.Add(journey);
            }
            
            // If any journeys are left, add to book
            if (bufferedJourneys.Count != 0)
            {
                books.Add(new Book
                {
                    IsOverThreshold = previousOverThreshold!.Value,
                    Sheets = BuildSheets(bufferedJourneys)
                });
            }
            
            return books;
        }

        private async Task<bool> CalculateOverThresholdAsync(DateOnly claimDate)
        {
            var currentTaxYear = claimDate.Month >= 4 ? claimDate.Year : claimDate.Year - 1;
            var taxYearStart = new DateOnly(currentTaxYear, 4, 6);

            var totalMilesSinceTaxYearStart = await _context.Journeys
                .Where(j => j.Date >= taxYearStart && j.Date <= claimDate)
                .SumAsync(j => j.TotalMiles);

            return totalMilesSinceTaxYearStart > 10000;
        }

        private List<Sheet> BuildSheets(List<Journey> journeys)
        {
            var sheets = new List<Sheet>();
            var currentSheet = new Sheet();
            
            foreach (var journey in journeys)
            {
                var homeLocationName = journey.HomeLocation.Name;

                var tripsCombined = string.Join(" > ", journey.Trips
                    .OrderBy(t => t.Id)
                    .Select(t => $"{t.Location.Name} {t.Reason.Name}")
                );

                var description = $"{homeLocationName} > {tripsCombined} > {homeLocationName}";
                var descriptionLines = SplitIntoLinesByCharacter(description, 60);
                    
                var row = new Row()
                {
                    Date = journey.Date.ToString("yyyy-MM-dd"),
                    Description = descriptionLines,
                    TotalMiles = journey.TotalMiles,
                };
                
                // Check if the second sheet needs to be used
                var currentDescriptionCount = currentSheet.Rows.Sum(r => r.Description.Count);
                if (currentDescriptionCount + descriptionLines.Count > 30 && sheets.Count == 0)
                {
                    if (currentSheet.Rows.Count > 0)
                        sheets.Add(currentSheet);
                    
                    currentSheet = new Sheet();
                }
                
                currentSheet.Rows.Add(row);
            }
            
            if (currentSheet.Rows.Count > 0)
                sheets.Add(currentSheet);
            
            return sheets;
        }
        
       
        private static List<string> SplitIntoLinesByCharacter(string text, int maxLength)
        {
            var splitText = new List<string>();
    
            var remaining = text;

            while (remaining.Length > 0)
            {
                if (remaining.Length <= maxLength)
                {
                    splitText.Add(remaining);
                    break;
                }

                // Find last whitespace before maxLength
                var chunk = remaining[..maxLength];
                var lastSpace = chunk.LastIndexOf(' ');

                if (lastSpace > 0)
                {
                    splitText.Add(remaining[..lastSpace]);
                    remaining = remaining[(lastSpace + 1)..];
                }
                else
                {
                    // No space found, force split at maxLength
                    splitText.Add(remaining[..maxLength]);
                    remaining = remaining[maxLength..];
                }
            }

            return splitText;
        }
    }
    
    public class Row
    {
        public string Date { get; set; } = string.Empty;
        public ICollection<string> Description { get; set; } = new List<string>();
        public int TotalMiles { get; set; }
    }

    public class Sheet
    {
        public ICollection<Row> Rows { get; set; } = new  List<Row>();
    }

    public class Book
    {
        public ICollection<Sheet> Sheets { get; set; } = new List<Sheet>();
        public bool IsOverThreshold { get; set; }
    }
}