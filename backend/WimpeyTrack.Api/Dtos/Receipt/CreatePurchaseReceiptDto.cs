
using WimpeyTrack.Api.Dtos.Purchase;

namespace WimpeyTrack.Api.Dtos.Receipt;

public class CreatePurchaseReceiptDto : CreateReceiptBase64Dto
{
    public CreatePurchaseDto Purchase { get; set; } = null!;
}