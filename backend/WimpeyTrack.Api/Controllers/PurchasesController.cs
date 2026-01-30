using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos;
using WimpeyTrack.Api.Dtos.Item;
using WimpeyTrack.Api.Dtos.Purchase;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Purchases
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseDto>>> GetPurchases()
        {
            return await _context.Purchases.Select(p => new PurchaseDto()
            {
                Id = p.Id,
                Date = p.Date,
                StoreName =  p.StoreName,
                Items = p.Items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Cost = i.Cost,
                    Reason = i.Reason
                }).ToList()
            }).ToListAsync();
        }

        // GET: api/Purchases/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseDto>> GetPurchase(int id)
        {
            var purchase = await _context.Purchases.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            var dto = new PurchaseDto()
            {
                Id = purchase.Id,
                Date = purchase.Date,
                StoreName = purchase.StoreName,
                Items = purchase.Items.Select(i => new ItemDto()
                {
                    Id = i.Id,
                    Name = i.Name,
                    Cost = i.Cost,
                    Quantity = i.Quantity,
                    Reason =  i.Reason,
                }).ToList(),
            };
            
            return dto;
        }

        // PUT: api/Purchases/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPurchase(int id, UpdatePurchaseDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            // Check if the purchase exists
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }
            
            // Update properties
            purchase.Date = dto.Date;
            purchase.StoreName = dto.StoreName;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Purchases
        [HttpPost]
        public async Task<ActionResult<PurchaseDto>> PostPurchase(CreatePurchaseDto dto)
        {
            var purchase = new Purchase()
            {
                Date = dto.Date,
                StoreName = dto.StoreName,
                ReceiptId = dto.ReceiptId,
                Items = dto.Items.Select(i => new Item
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Cost = i.Cost,
                    Reason = i.Reason
                }).ToList()
            };
            
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            var purchaseDto = new PurchaseDto()
            {
                Id = purchase.Id,
                Date = purchase.Date,
                StoreName = purchase.StoreName,
                Items = purchase.Items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Cost = i.Cost,
                    Reason = i.Reason
                }).ToList(),
            };
            
            return CreatedAtAction("GetPurchase", new { id = purchase.Id }, purchaseDto);
        }

        // DELETE: api/Purchases/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PurchaseExists(int id)
        {
            return _context.Purchases.Any(e => e.Id == id);
        }
    }
}
