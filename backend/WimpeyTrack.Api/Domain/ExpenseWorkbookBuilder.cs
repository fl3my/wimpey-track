using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using WimpeyTrack.Api.Dtos.Book;
using WimpeyTrack.Api.Options;

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
    private readonly ExpenseWorkbookOptions _options;
    
    public ExpenseWorkbookBuilder(IOptions<ExpenseWorkbookOptions> options)
    {
        _options = options.Value;
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
        if (!File.Exists(_options.TemplatePath))
        {
            throw new FileNotFoundException($"Template file {_options.TemplatePath} does not exist");
        }

        return new XLWorkbook(_options.TemplatePath);
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
        var sheet = workbook.Worksheet(_options.ExpenseSheetIndex);
        
        PopulateClaimRate(sheet, book);
        PopulatePurchaseRows(sheet, book);
    }

    private void PopulateClaimRate(IXLWorksheet sheet, Book book)
    {
        sheet.Cell(_options.ClaimRateRow, _options.ClaimRateColumn)
            .Value = book.IsOverThreshold ? 0.25 : 0.45;

        sheet.Cell(_options.OverThresholdRow, _options.OverThresholdColumn)
            .Value = book.IsOverThreshold ? "YES" : "NO";
    }
    
    private void PopulatePurchaseRows(IXLWorksheet sheet, Book book)
    {
        var currentRow = _options.PurchaseStartRow;
        
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
        var sheetIndex = _options.FirstMileageSheetIndex;

        foreach (var sheet in book.Sheets)
        {
            var worksheet = workbook.Worksheet(sheetIndex);
            PopulateMileageSheet(worksheet, sheet);
            sheetIndex++;
        }
    }

    private void PopulateMileageSheet(IXLWorksheet worksheet, Sheet sheet)
    {
        var currentRow = _options.MileageStartRow;

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