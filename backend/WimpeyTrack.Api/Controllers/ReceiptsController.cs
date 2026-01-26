using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos;
using WimpeyTrack.Api.Enums;
using WimpeyTrack.Api.Models;
using WimpeyTrack.Api.Services;

namespace WimpeyTrack.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class ReceiptsController : ControllerBase
{
    private const long MaxFileSize = 2 * 1024 * 1024; // 2 MB
    private static readonly string[] AllowedTypes =
        { "image/jpeg" };

    private readonly ApplicationDbContext _context;
    private readonly IReceiptAnalysisService _analysisService;
    private readonly IReceiptImageStorage _imageStorage;

    public ReceiptsController(ApplicationDbContext context, IReceiptAnalysisService analysisService, IReceiptImageStorage imageStorage)
    {
        _context = context;
        _analysisService = analysisService;
        _imageStorage = imageStorage;
    }

    // GET: api/Receipts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Receipt>>> GetReceipts()
    {
        return await _context.Receipts.ToListAsync();
    }

    // GET: api/Receipts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ReceiptDto>> GetReceipt(int id)
    {
        var receipt = await _context.Receipts.FindAsync(id);
        
        if (receipt == null) return NotFound();

        var dto = new ReceiptDto()
        {
            Id = receipt.Id,
            Name = receipt.Name,
            Date = receipt.Date,
            Category = receipt.Category,
            ImagePath = receipt.ImagePath,
        };

        return dto;
    }

    // POST: api/Receipts
    [HttpPost()]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReceiptDto>> PostReceipt([FromForm] CreateReceiptDto dto)
    { 
        var imagePath = await _imageStorage.SaveAsync(dto.File);

        return await CreateReceiptAsync(dto.Name, dto.Date, dto.Category, imagePath);
    }

    // POST: api/Receipts/UploadBase64
    [HttpPost("UploadBase64")]
    public async Task<ActionResult<ReceiptDto>> PostReceiptBase64(CreateReceiptBase64Dto dto)
    {
        // Attempt to convert the base64 into a file
        byte[] fileBytes;
        try
        {
            fileBytes = Convert.FromBase64String(dto.Base64Content);
        }
        catch
        {
            return BadRequest("Invalid base64 content.");
        }
        
        // Save the file to the system
        var imagePath = await _imageStorage.SaveAsync(fileBytes);

        return await CreateReceiptAsync(dto.Name, dto.Date, dto.Category, imagePath);
    }
    
    // POST: api/Receipts/ocr
    [HttpPost("ocr")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReceiptOcrResultDto>> PostReceiptOcr([FromForm] ImageUploadDto dto)
    {
        // Validation
        if (dto.File == null || dto.File.Length == 0)
            return BadRequest("No file uploaded.");

        if (dto.File.Length > MaxFileSize)
            return BadRequest("File too large.");
    
        if (!AllowedTypes.Contains(dto.File.ContentType))
            return BadRequest("File type not supported.");
        
        // Convert the image to binary data
        await using var stream = dto.File.OpenReadStream();
        var binaryData = await BinaryData.FromStreamAsync(stream);
        
        // Make request to the receipt service
        var result = await _analysisService.AnalyseReceiptAsync(binaryData);
        
        // If no results return
        if (result == null) return BadRequest("No receipts found.");

        // Convert the image to base 64 so can be returned
        var imageBase64 = Convert.ToBase64String(binaryData);
        
        var receiptResultDto = new ReceiptOcrResultDto()
        {
            ImageBase64 = imageBase64,
            ReceiptData = new List<ReceiptData>() { result }
        };
        
        return Ok(receiptResultDto);
    }

    // DELETE: api/Receipts/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReceipt(int id)
    {
        var receipt = await _context.Receipts.FindAsync(id);
        if (receipt == null) return NotFound();
        
        if (!string.IsNullOrWhiteSpace(receipt.ImagePath))
        {
            await _imageStorage.DeleteAsync(receipt.ImagePath);
        }
        
        _context.Receipts.Remove(receipt);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    private async Task<ActionResult<ReceiptDto>> CreateReceiptAsync(
        string name,
        DateOnly date,
        ReceiptCategory category,
        string imagePath)
    {
        // Create receipt entity
        var receipt = new Receipt
        {
            Name = name,
            Date = date,
            Category = category,
            ImagePath = imagePath
        };

        
        // Attempt to save receipt
        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();

        // Create receipt dto
        var receiptDto = new ReceiptDto
        {
            Id = receipt.Id,
            Name = receipt.Name,
            Category = receipt.Category,
            Date = receipt.Date,
            ImagePath = receipt.ImagePath
        };

        return CreatedAtAction("GetReceipt", new { id = receipt.Id }, receiptDto);
    }
}