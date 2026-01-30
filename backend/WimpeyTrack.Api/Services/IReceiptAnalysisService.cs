using WimpeyTrack.Api.Dtos.Shared;

namespace WimpeyTrack.Api.Services;

public interface IReceiptAnalysisService
{
    Task<ReceiptData?> AnalyseReceiptAsync(BinaryData receiptData);
}