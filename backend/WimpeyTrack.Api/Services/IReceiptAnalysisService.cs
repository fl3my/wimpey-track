using WimpeyTrack.Api.Dtos;

namespace WimpeyTrack.Api.Services;

public interface IReceiptAnalysisService
{
    Task<List<ReceiptData>> AnalyseReceiptAsync(BinaryData receiptData);
}