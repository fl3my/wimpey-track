namespace WimpeyTrack.Api.Options;

public sealed class ExpenseWorkbookOptions
{
    public string TemplatePath { get; init; } = "Templates/template.xlsx";

    public int PurchaseStartRow { get; init; } = 12;

    public int ClaimRateRow { get; init; } = 53;
    public int ClaimRateColumn { get; init; } = 2;

    public int OverThresholdRow { get; init; } = 49;
    public int OverThresholdColumn { get; init; } = 2;

    public int MileageStartRow { get; init; } = 6;

    public int ExpenseSheetIndex { get; init; } = 1;
    public int FirstMileageSheetIndex { get; init; } = 3;
}