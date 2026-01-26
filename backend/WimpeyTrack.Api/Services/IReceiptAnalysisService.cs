using WimpeyTrack.Api.Dtos;

namespace WimpeyTrack.Api.Services;

public interface IReceiptAnalysisService
{
    Task<ReceiptData?> AnalyseReceiptAsync(BinaryData receiptData);
}