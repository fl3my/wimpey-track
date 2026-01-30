using ClosedXML.Excel;
using WimpeyTrack.Api.Dtos.Book;

namespace WimpeyTrack.Api.Domain;

/// <summary>
/// Responsible for adding the data to the spreadsheet using ClosedXML.
/// </summary>
public interface IExpenseWorkbookBuilder
{
    Stream Build(Book book);
}

public class ExpenseWorkbookBuilder : IExpenseWorkbookBuilder
{
    private readonly IWebHostEnvironment _env;
    
    private const int PurchaseStartRow = 12;

    private const int ClaimRateRow = 53;
    private const int ClaimRateColumn = 2;

    private const int OverThresholdRow = 49;
    private const int OverThresholdColumn = 2;

    private const int MileageStartRow = 6;

    private const int ExpenseSheetIndex = 1;
    private const int FirstMileageSheetIndex = 3;
    
    private const string TemplateRelativePath = "Templates/template.xlsx";
    
    public ExpenseWorkbookBuilder(IWebHostEnvironment env)
    {
        _env = env;
    }
    
    public Stream Build(Book book)
    {
        var workbook = LoadTemplate();
        
        PopulateExpenseSheets(workbook, book);
        PopulateMileageSheets(workbook, book);
        
        return SaveToStream(workbook);
    }

    private XLWorkbook LoadTemplate()
    {
        var templatePath = Path.Combine(_env.ContentRootPath, TemplateRelativePath);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file {templatePath} does not exist");
        }

        return new XLWorkbook(templatePath);
    }
    
    private static Stream SaveToStream(XLWorkbook workbook)
    {
        var stream = new MemoryStream();    
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private void PopulateExpenseSheets(XLWorkbook workbook, Book book)
    {
        var sheet = workbook.Worksheet(ExpenseSheetIndex);
        
        PopulateClaimRate(sheet, book);
        PopulatePurchaseRows(sheet, book);
    }

    private void PopulateClaimRate(IXLWorksheet sheet, Book book)
    {
        sheet.Cell(ClaimRateRow, ClaimRateColumn)
            .Value = book.IsOverThreshold ? 0.25 : 0.45;

        sheet.Cell(OverThresholdRow, OverThresholdColumn)
            .Value = book.IsOverThreshold ? "YES" : "NO";
    }
    
    private void PopulatePurchaseRows(IXLWorksheet sheet, Book book)
    {
        var currentRow = PurchaseStartRow;
        
        foreach (var row in book.PurchaseRows)
        {
            sheet.Cell(currentRow, 1).Value = row.ExpenseCode;
            sheet.Cell(currentRow, 2).Value = row.Date;
            sheet.Cell(currentRow, 3).Value = row.ExpenseDetail;
            sheet.Cell(currentRow, 4).Value = row.ReasonForExpense;
            sheet.Cell(currentRow, 5).Value = row.VatCode;
            sheet.Cell(currentRow, 6).Value = row.ReceiptAttached ? "YES" : "NO";
            sheet.Cell(currentRow, 7).Value = row.Cost;

            currentRow++;
        }
    }

    private void PopulateMileageSheets(XLWorkbook workbook, Book book)
    {
        var sheetIndex = FirstMileageSheetIndex;

        foreach (var sheet in book.Sheets)
        {
            var worksheet = workbook.Worksheet(sheetIndex);
            PopulateMileageSheet(worksheet, sheet);
            sheetIndex++;
        }
    }

    private void PopulateMileageSheet(IXLWorksheet worksheet, Sheet sheet)
    {
        var currentRow = MileageStartRow;

        foreach (var row in sheet.Rows)
        {
            worksheet.Cell(currentRow, 1).Value = row.Date;
            worksheet.Cell(currentRow, 5).Value = row.TotalMiles;

            foreach (var descriptionLine in row.Description)
            {
                worksheet.Cell(currentRow, 2).Value = descriptionLine;
                currentRow++;
            }
        }
    }
}