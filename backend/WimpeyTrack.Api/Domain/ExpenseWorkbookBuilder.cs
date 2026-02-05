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

    // Sheet 1
    private const int FullNameRow = 4;
    private const int FullNameColumn = 3;

    private const int StaffNumberRow = 6;
    private const int StaffNumberColumn = 3;

    private const int BusinessUnitRow = 6;
    private const int BusinessUnitColumn = 5;

    private const int DeptSiteRow = 6;
    private const int DeptSiteColumn = 9;
    
    private const int PurchaseStartRow = 12;

    private const int ClaimRateRow = 53;
    private const int ClaimRateColumn = 2;

    private const int OverThresholdRow = 49;
    private const int OverThresholdColumn = 2;
    
    // Second sheets
    private const int MileageStartRow = 6;
    
    private const int StartHomePostcodeRow = 6;
    private const int StartHomePostcodeColumn = 3;
    
    private const int EndHomePostcodeRow = 6;
    private const int EndHomePostcodeColumn = 4;

    private const int FuelTypeRow = 4;
    private const int FuelTypeColumn = 13;

    private const int EngineSizeRow = 5;
    private const int EngineSizeColumn = 13;

    private const int RegistrationNumberRow = 6;
    private const int RegistrationNumberColumn = 13;

    private const int MakeRow = 7;
    private const int MakeColumn = 13;
    
    // Sheet indexes
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
        
        PopulateUserDetails(sheet, book);
    }

    private void PopulateUserDetails(IXLWorksheet sheet, Book book)
    {
        sheet.Cell(FullNameRow, FullNameColumn).Value = book.BookProfile.FullName;
        sheet.Cell(StaffNumberRow, StaffNumberColumn).Value = book.BookProfile.StaffNumber;
        sheet.Cell(BusinessUnitRow, BusinessUnitColumn).Value = book.BookProfile.BusinessUnit;
        sheet.Cell(DeptSiteRow, DeptSiteColumn).Value = book.BookProfile.DepartmentSiteName;
    }

    private void PopulateVehicleDetails(IXLWorksheet sheet, Book book)
    {
        sheet.Cell(FuelTypeRow, FuelTypeColumn).Value = book.BookProfile.VehicleFuelType;
        sheet.Cell(EngineSizeRow, EngineSizeColumn).Value = book.BookProfile.VehicleEngineSize;
        sheet.Cell(RegistrationNumberRow, RegistrationNumberColumn).Value = book.BookProfile.VehicleRegistration;
        sheet.Cell(MakeRow, MakeColumn).Value = book.BookProfile.VehicleMake;
    }

    private void PopualatePostcode(IXLWorksheet sheet, Book book)
    {
        sheet.Cell(StartHomePostcodeRow, StartHomePostcodeColumn).Value = book.BookProfile.HomePostcode;
        sheet.Cell(EndHomePostcodeRow, EndHomePostcodeColumn).Value = book.BookProfile.HomePostcode;
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
            PopulateVehicleDetails(worksheet, book);
            PopualatePostcode(worksheet, book);
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