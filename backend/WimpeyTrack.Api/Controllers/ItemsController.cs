using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos;
using WimpeyTrack.Api.Dtos.Item;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Controllers
{
    [Route("Purchases/{purchaseId}/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetItems()
        {
            return await _context.Items.Select(s => new ItemDto()
            {
                Id = s.Id,
                Name = s.Name,
                Quantity =  s.Quantity,
                Cost =  s.Cost,
                Reason =  s.Reason,
            }).ToListAsync();
        }

        // GET: api/Items/5
        [HttpGet("{itemId}")]
        public async Task<ActionResult<ItemDto>> GetItem(int purchaseId, int itemId)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.PurchaseId == purchaseId &&i.Id == itemId);
                
            if (item == null)
            {
                return NotFound();
            }

            var dto = new ItemDto()
            {
                Id = item.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                Cost = item.Cost,
                Reason = item.Reason,
            };
            
            return dto;
        }

        // PUT: api/Items/5
        [HttpPut("{itemId}")]
        public async Task<IActionResult> PutItem(int purchaseId, int itemId, UpdateItemDto dto)
        {
            if (itemId != dto.Id)
            {
                return BadRequest();
            }

            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return NotFound();
            }

            // Update the properties
            item.Name = dto.Name;
            item.Cost = dto.Cost;
            item.Quantity = dto.Quantity;
            item.Reason = dto.Reason;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(itemId))
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

        // POST: api/Items
        [HttpPost]
        public async Task<ActionResult<ItemDto>> PostItem(int purchaseId, CreateItemDto dto)
        {
            if (!await _context.Purchases.AnyAsync(t => t.Id == purchaseId))
                return NotFound();

            var item = new Item()
            {
                Name = dto.Name,
                Quantity = dto.Quantity,
                Cost = dto.Cost,
                Reason = dto.Reason,
                PurchaseId = purchaseId
            };
            
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var dtoItem = new ItemDto()
            {
                Id = item.Id,
                Quantity = item.Quantity,
                Name = item.Name,
                Cost = item.Cost,
                Reason = item.Reason,
            };
            
            return CreatedAtAction("GetItem", new { purchaseId = item.PurchaseId, itemId = item.Id }, dtoItem);
        }

        // DELETE: api/Items/5
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteItem(int purchaseId, int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return NotFound();
            }

            if (item.PurchaseId != purchaseId)
            {
                return BadRequest();
            }
            
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}
