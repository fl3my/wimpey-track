namespace WimpeyTrack.Api.Services;

public class FileSystemReceiptImageStorage : IReceiptImageStorage
{
    private readonly string _storagePath;
    private readonly ILogger<FileSystemReceiptImageStorage> _logger;

    public FileSystemReceiptImageStorage(IConfiguration configuration, IWebHostEnvironment env, ILogger<FileSystemReceiptImageStorage> logger)
    {
        _storagePath = configuration["ReceiptStorage:Path"] 
                       ?? Path.Combine(env.WebRootPath , "uploads", "receipts");;
        
        _logger = logger;
        
        // Ensure directory exists
        Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> SaveAsync(byte[] imageBytes)
    {
        var fileName = $"{Guid.NewGuid()}.jpg";
        var fullPath = Path.Combine(_storagePath, fileName);
        
        await File.WriteAllBytesAsync(fullPath, imageBytes);
        _logger.LogInformation("Saved receipt image to {FileName}", fileName);
        
        return $"/uploads/receipts/{fileName}";
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("File size must be greater than zero", nameof(file));
        }
        
        var extension = Path.GetExtension(file.FileName);
        
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";
        
        var fileName = $"{Guid.NewGuid()}.jpg";
        var fullPath = Path.Combine(_storagePath, fileName);
        
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        _logger.LogInformation("Saved receipt image to {FileName}", fileName);

        return $"/uploads/receipts/{fileName}";
    }

    public async Task<byte[]?> GetAsync(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return null;
        
        var fileName = Path.GetFileName(imagePath);
        var fullPath = Path.Combine(_storagePath, fileName);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("File {FileName} not found", imagePath);
            return null;
        }
        
        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task DeleteAsync(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return Task.CompletedTask;

        var fileName = Path.GetFileName(imagePath);
        var fullPath = Path.Combine(_storagePath, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted receipt image: {ImagePath}", imagePath);
        }
        
        return Task.CompletedTask;
    }
}