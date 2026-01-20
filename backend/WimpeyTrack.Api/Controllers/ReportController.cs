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
            var book = await CreateBookAsync(startDate, endDate);
            
            // Load template
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "template.xlsx");
            using var workbook = new XLWorkbook(templatePath);
            
            // Target the first mileage sheet
            
            // Fill Claim rate on first sheet
            var expenseSheet = workbook.Worksheet(1);
            expenseSheet.Cell(53, 2).Value = book.ClaimRate;
            expenseSheet.Cell(49, 2).Value = "YES";
            
            // For each sheet
            for(var sheetIndex = 0; sheetIndex < book.Sheets.Count; sheetIndex++)
            {
                // Get the sheet index
                var sheet = book.Sheets.ElementAt(sheetIndex);
                var worksheet = workbook.Worksheet(sheetIndex + 3); // Mileage sheet is the 2nd sheet
                
                // Get the starting row
                int currentRow = 6;
                
                // Fill the rows downwards
                foreach (var row in sheet.Rows)
                {
                    // Fill rows
                    worksheet.Cell(currentRow, 1).Value = row.Date;
                    worksheet.Cell(currentRow, 5).Value = row.TotalMiles;
                    
                    foreach (var description in row.Description)
                    {
                        worksheet.Cell(currentRow, 2).Value = description;
                        currentRow++;
                    }
                }
            }
            
            using var stream = new MemoryStream();
            
            workbook.SaveAs(stream);
            stream.Position = 0;
            
            return File(stream.ToArray(), 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Mileage_Report_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.xlsx");
        }

        private async Task<Book> CreateBookAsync(DateOnly startDate, DateOnly endDate)
        {
            // Get data from server
            var journeys = await _context.Journeys
                .Where(j => j.Date > startDate && j.Date < endDate)
                .Include(j => j.HomeLocation)
                .Include(j => j.Trips)
                .ThenInclude(t => t.Location)
                .Include(j => j.Trips)
                .ThenInclude(t => t.Reason)
                .ToListAsync();
            
            // Calculate mileage claim rate
            var claimRate = await CalculateClaimRateAsync();
            
            // Build sheets
            var sheets = BuildSheets(journeys);
            
            return new Book()
            {
                Sheets = sheets,
                ClaimRate = claimRate
            };
        }

        private async Task<double> CalculateClaimRateAsync()
        {
            var currentTaxYear = DateTime.Now.Month >= 4 ? DateTime.Now.Year : DateTime.Now.Year - 1;
            var taxYearStart = new DateOnly(currentTaxYear, 4, 4);

            var totalMilesSinceTaxYearStart = await _context.Journeys
                .Where(j => j.Date >= taxYearStart)
                .SumAsync(j => j.TotalMiles);

            return totalMilesSinceTaxYearStart > 10000 ? 0.25 : 0.45;
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
        public double ClaimRate { get; set; }
    }
}