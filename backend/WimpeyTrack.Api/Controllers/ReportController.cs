using System.IO.Compression;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Models;
using WimpeyTrack.Api.Services;

namespace WimpeyTrack.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfConverterService _pdfConverterService;
        private readonly IReceiptImageStorage _imageStorage;
        private readonly IImageProcessingService _imageProcessingService;
        
        public ReportController(ApplicationDbContext context, IPdfConverterService pdfConverterService, IReceiptImageStorage imageStorage, IImageProcessingService imageProcessingService)
        {
            _context = context;
            _pdfConverterService = pdfConverterService;
            _imageStorage = imageStorage;
            _imageProcessingService = imageProcessingService;
        }
        
        [HttpGet]
        public async Task<IActionResult> DownloadReport(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be after end date.");
            }
            
            // Retrieve the data
            var books = await CreateBooksAsync(startDate, endDate);

            if (books.Count == 0)
            {
                return BadRequest(new { message = "Sorry, no journeys have been made during this time range"});
            }
            
            var receipts = await _context.Receipts
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .OrderBy(r => r.Date)
                .ToListAsync();

            
            // Load template
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "template.xlsx");
            
            using var zipStream = new MemoryStream();

            await using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // For each book
                for (var bookIndex = 0; bookIndex < books.Count; bookIndex++)
                {
                    var book = books[bookIndex]; ;
                    
                    // Create an instance of ClosedXML
                    using var workbook = new XLWorkbook(templatePath);

                    // Fill Claim rate on first sheet
                    var expenseSheet = workbook.Worksheet(1);
                    
                    expenseSheet.Cell(53, 2).Value = book.IsOverThreshold ? 0.25 : 0.45;
                    expenseSheet.Cell(49, 2).Value = book.IsOverThreshold ? "YES" : "NO";
                    
                    // Fill the sheet with purchases
                    var currentPurchaseRow = 12;
                    foreach (var purchaseRow in book.PurchaseRows)
                    {
                        expenseSheet.Cell(currentPurchaseRow, 1).Value = purchaseRow.ExpenseCode;
                        expenseSheet.Cell(currentPurchaseRow, 2).Value = purchaseRow.Date;
                        expenseSheet.Cell(currentPurchaseRow, 3).Value = purchaseRow.ExpenseDetail;
                        expenseSheet.Cell(currentPurchaseRow, 4).Value = purchaseRow.ReasonForExpense;
                        expenseSheet.Cell(currentPurchaseRow, 5).Value = purchaseRow.VatCode;
                        expenseSheet.Cell(currentPurchaseRow, 6).Value = purchaseRow.ReceiptAttached ? "YES" : "NO";
                        expenseSheet.Cell(currentPurchaseRow, 7).Value = purchaseRow.Cost;
                        
                        currentPurchaseRow++;
                    }
                    
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
                    using var xlsxStream = new MemoryStream();
                    workbook.SaveAs(xlsxStream);
                    xlsxStream.Position = 0;

                    // Hardcoded page range
                    var pageRange = "1,3";

                    // If the mileage detail goes over to the next page, add the last page
                    if (book.Sheets.Count > 1)
                    {
                        pageRange = $"{pageRange},4";
                    }
                    
                    // Convert the xlsx to pdf
                    var pdfBytes = await _pdfConverterService.ConvertXlsxToPdfAsync(xlsxStream, pageRange);
                    
                    // Split pdf pages
                    using var inputPdfStream = new MemoryStream(pdfBytes);
                    var inputPdf = PdfReader.Open(inputPdfStream, PdfDocumentOpenMode.Import);
                    
                    for (var pageIndex = 0; pageIndex < inputPdf.PageCount; pageIndex++)
                    {
                        using var singlePageDoc = new PdfDocument();
                        singlePageDoc.AddPage(inputPdf.Pages[pageIndex]);
                        
                        using var singlePageStream = new MemoryStream();
                        await singlePageDoc.SaveAsync(singlePageStream);
                        singlePageStream.Position = 0;
                        
                        var entryName = $"Expenses_{bookIndex + 1}_Page_{pageIndex + 1}_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.pdf";
                        
                        var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                        await using var entryStream = await entry.OpenAsync();
                        await singlePageStream.CopyToAsync(entryStream);
                    }
                }
                
                // Calculate the pages required
                const int receiptsPerRow = 5;
                var totalPages =  (int)Math.Ceiling(receipts.Count / (double)receiptsPerRow);

                // For each page
                for (var pageIndex = 0; pageIndex < totalPages; pageIndex++)
                {
                    // Get receipts
                    var receiptsForThisPage = receipts
                        .Skip(pageIndex * receiptsPerRow)
                        .Take(receiptsPerRow)
                        .ToList();
                    
                    // Convert receipts to list of byte arrays
                    var receiptImageBytes = new List<byte[]>();
                    foreach (var receiptRow in receiptsForThisPage)
                    {
                        var imageBytes = await _imageStorage.GetAsync(receiptRow.ImagePath);
                        if (imageBytes is { Length: > 0 })
                        {
                            receiptImageBytes.Add(imageBytes);
                        }
                    }

                    // If no receipts continue
                    if (receiptsForThisPage.Count <= 0) continue;
                    
                    // Combine the receipts into one image
                    var combinedImage = await _imageProcessingService.CombineReceiptsAsync(receiptImageBytes, receiptsPerRow);
                    
                    // Save the receipts images
                    var entryName = $"Receipts_Page_{pageIndex + 1}.jpeg";
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                    await using var entryStream = await entry.OpenAsync();
                    await entryStream.WriteAsync(combinedImage);
                }
            }
            
            zipStream.Position = 0;
            
            // Return a zip file
            return File(
                zipStream.ToArray(),
                "application/zip",
                $"Expenses_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.zip"
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
            
            var purchases = await _context.Purchases
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .OrderBy(p => p.Date)
                .Include(purchase => purchase.Items)
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
                    var bookStartDate = books.Count == 0 ? startDate : bufferedJourneys.First().Date;
                    var bookEndDate = bufferedJourneys.Last().Date;
                    
                    var itemsInBook = purchases
                        .Where(p => p.Date >= bookStartDate && p.Date <= bookEndDate && p.Items.Count != 0) 
                        .SelectMany(p => p.Items,
                            (p, i) => new
                            {
                                Purchase = p,
                                Item = i,
                            })
                        .Select(x => new PurchaseRow
                        {
                            ExpenseCode = "O",
                            Date = x.Purchase.Date.ToString("yyyy-MM-dd"),
                            ExpenseDetail = $"{x.Purchase.StoreName} - {x.Item.Name} *{x.Item.Quantity}",
                            ReasonForExpense = x.Item.Reason,
                            VatCode = "P10",
                            ReceiptAttached = true,
                            Cost = x.Item.Cost * x.Item.Quantity,
                        })
                        .ToList();
                    
                    // Create the book at current claim rate with buffered journeys
                    books.Add(new Book()
                    {
                        IsOverThreshold = previousOverThreshold.Value,
                        PurchaseRows = itemsInBook,
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
                var bookStartDate = books.Count == 0 ? startDate : bufferedJourneys.First().Date;
                var bookEndDate = endDate;
                
                var itemsInBook = purchases
                    .Where(p => p.Date >= bookStartDate && p.Date <= bookEndDate && p.Items.Count != 0) 
                    .SelectMany(p => p.Items,
                        (p, i) => new
                        {
                            Purchase = p,
                            Item = i,
                        })
                    .Select(x => new PurchaseRow
                    {
                        ExpenseCode = "O",
                        Date = x.Purchase.Date.ToString("yyyy-MM-dd"),
                        ExpenseDetail = $"{x.Purchase.StoreName} - {x.Item.Name} *{x.Item.Quantity}",
                        ReasonForExpense = x.Item.Reason,
                        VatCode = "P10",
                        ReceiptAttached = true,
                        Cost = x.Item.Cost * x.Item.Quantity,
                    })
                    .ToList();
                
                books.Add(new Book
                {
                    IsOverThreshold = previousOverThreshold!.Value,
                    PurchaseRows = itemsInBook,
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

    public class PurchaseRow
    {
        public string ExpenseCode  { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string ExpenseDetail { get; set; } = string.Empty;
        public string ReasonForExpense { get; set; } = string.Empty;
        public string VatCode { get; set; } = string.Empty;
        public bool ReceiptAttached { get; set; }
        public double Cost { get; set; }
    }
    
    public class Book
    {
        public ICollection<Sheet> Sheets { get; set; } = new List<Sheet>();
        public ICollection<PurchaseRow> PurchaseRows { get; set; } = new List<PurchaseRow>();
        public bool IsOverThreshold { get; set; }
    }
}