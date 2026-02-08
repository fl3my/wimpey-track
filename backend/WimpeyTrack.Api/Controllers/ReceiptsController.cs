using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.Receipt;
using WimpeyTrack.Api.Dtos.Shared;
using WimpeyTrack.Api.Enums;
using WimpeyTrack.Api.Models;
using WimpeyTrack.Api.Services;

namespace WimpeyTrack.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReceiptsController : ControllerBase
{
    private static readonly string[] AllowedTypes =
        { "image/jpeg" };

    private readonly ApplicationDbContext _context;
    private readonly IReceiptAnalysisService _analysisService;
    private readonly IReceiptImageStorage _imageStorage;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IVisionService _visionService;

    public ReceiptsController(ApplicationDbContext context, IReceiptAnalysisService analysisService, IReceiptImageStorage imageStorage, IImageProcessingService imageProcessingService, IVisionService visionService)
    {
        _context = context;
        _analysisService = analysisService;
        _imageStorage = imageStorage;
        _imageProcessingService = imageProcessingService;
        _visionService = visionService;
    }

    // GET: api/Receipts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Receipt>>> GetReceipts()
    {
        return await _context.Receipts.OrderByDescending(r => r.Date).ToListAsync();
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
    public async Task<ActionResult<ReceiptDto>> PostReceipt([FromForm] CreateReceiptDto dto, CancellationToken cancellationToken)
    { 
        // Detect the receipts
        var visionResult = await _visionService.DetectReceiptsAsync(dto.File, cancellationToken);
        
        // Map bounding boxes to domain model
        var firstBox = visionResult.Receipts
            .Select(r => new BoundingBox()
            {
                X = r.X,
                Y = r.Y,
                Width = r.Width,
                Height = r.Height
            })
            .FirstOrDefault();

        // 3. Reject if no receipt has been detected
        if (firstBox == null)
        {
            return BadRequest(new
            {
                message = "No receipt detected in image"
            });
        }
        
        // Crop the detected receipt
        await using var stream = dto.File.OpenReadStream();
        var cropped = await _imageProcessingService
            .CropAsync(stream, firstBox, cancellationToken);

        var resizedAndCropped = await _imageProcessingService.ResizeAsync(cropped);
        
        // Save cropped image
        var imagePath = await _imageStorage.SaveAsync(resizedAndCropped);
        
        return await CreateReceiptAsync(dto.Name, dto.Date, dto.Category, imagePath);
    }

    // POST: api/Receipts/fuel
    [HttpPost("fuel")]
    public async Task<ActionResult<ReceiptDto>> PostReceiptFuel(CreateFuelReceiptDto dto)
    {
        return await CreateReceiptAsync(
            dto.Name,
            dto.Date, 
            ReceiptCategory.Fuel, 
            await SaveBase64Async(dto.Base64Content));
    }
    
    // POST: api/Receipts/purchase
    [HttpPost("purchase")]
    public async Task<ActionResult<ReceiptDto>> PostReceiptPurchase(CreatePurchaseReceiptDto dto)
    {
        // Create the receipt
        var receipt = new Receipt
        {
            Name = dto.Name,
            Category = ReceiptCategory.Supplies,
            Date = dto.Date,
            ImagePath = await SaveBase64Async(dto.Base64Content),
            Purchase = new Purchase
            {
                Date = dto.Purchase.Date,
                StoreName = dto.Purchase.StoreName,
                Items = dto.Purchase.Items.Select(i => new Item
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Cost = i.Cost,
                    Reason = i.Reason,
                }).ToList(),
            }
        };

        // Save the receipt
         _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();
        
        var receiptDto = new ReceiptDto()
        {
            Id = receipt.Id,
            Name = receipt.Name,
            Date = receipt.Date,
            Category = receipt.Category,
        };
        
        return CreatedAtAction("GetReceipt", new { id = receipt.Id }, receiptDto);
    }
    
    // POST: api/Receipts/ocr
    [HttpPost("ocr")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReceiptOcrResultDto>> PostReceiptOcr([FromForm] ImageUploadDto dto)
    {
        // Validation
        if (dto.File.Length == 0)
            return BadRequest("No file uploaded.");
        
        if (!AllowedTypes.Contains(dto.File.ContentType))
            return BadRequest("File type not supported.");
        
        // Crop to receipt
        var visionResult = await _visionService.DetectReceiptsAsync(dto.File);
        if (visionResult.Receipts.Count == 0)
        {
            return BadRequest(new { message = "No receipts found."});
        }
        
        // Map bounding boxes to domain model
        var firstBox = visionResult.Receipts
            .Select(r => new BoundingBox()
            {
                X = r.X,
                Y = r.Y,
                Width = r.Width,
                Height = r.Height
            })
            .FirstOrDefault();

        // 3. Reject if no receipt has been detected
        if (firstBox == null)
        {
            return BadRequest(new
            {
                message = "No receipt detected in image"
            });
        }
        
        await using var stream = dto.File.OpenReadStream();
        var cropped = await _imageProcessingService
            .CropAsync(stream, firstBox);
        
        var resizedStream = await _imageProcessingService.ResizeAsync(cropped);
        var binaryData = BinaryData.FromBytes(resizedStream);
        
        // Make request to the receipt service
        var result = await _analysisService.AnalyseReceiptAsync(binaryData);
        
        // If no results return
        if (result == null) return BadRequest(new {message = "No receipts found."});
        
        var imageBase64 = Convert.ToBase64String(binaryData.ToArray());
        
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
        var receipt = await _context.Receipts
            .Include(r => r.Purchase)
            .FirstOrDefaultAsync(r => r.Id == id);
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
    
    private async Task<string> SaveBase64Async(string base64)
    {
        try
        {
            var fileBytes = Convert.FromBase64String(base64);
            return await _imageStorage.SaveAsync(fileBytes);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid base64 content");
        }
    }
}