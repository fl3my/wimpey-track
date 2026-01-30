namespace WimpeyTrack.Api.Services;

public interface IReceiptImageStorage
{
    Task<string> SaveAsync(byte[] imageBytes);
    Task<string> SaveAsync(IFormFile file);
    Task<byte[]?> GetAsync(string imagePath);
    Task DeleteAsync(string imagePath);
}