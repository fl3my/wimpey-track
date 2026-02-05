using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.Receipt;
using WimpeyTrack.Api.Services;

namespace WimpeyTrack.Api.Providers;

/// <summary>
/// Returns images of receipts from a specific date range.
/// </summary>
public interface IReceiptProvider
{
    Task<IReadOnlyList<ReceiptImage>> GetAsync(
        DateOnly startDate,
        DateOnly endDate);
}

public class ReceiptProvider : IReceiptProvider
{
    private readonly ApplicationDbContext _context;
    private readonly IReceiptImageStorage _imageStorage;

    public ReceiptProvider(ApplicationDbContext context, IReceiptImageStorage imageStorage)
    {
        _context = context;
        _imageStorage = imageStorage;
    }

    public async Task<IReadOnlyList<ReceiptImage>> GetAsync(DateOnly startDate, DateOnly endDate)
    {
        // Get receipts in date range
        var receipts = await _context.Receipts
            .Where(r => r.Date >= startDate && r.Date <= endDate)
            .OrderBy(r => r.Date)
            .ToListAsync();

        var result = new List<ReceiptImage>();

        // Iterate over receipts and add to receipt image object
        foreach (var receipt in receipts)
        {
            var bytes = await _imageStorage.GetAsync(receipt.ImagePath);
            if (bytes is { Length: > 0 })
            {
                result.Add(new ReceiptImage()
                {
                    Date = receipt.Date,
                    ImageBytes = bytes
                });
            }
        }

        return result;
    }
}