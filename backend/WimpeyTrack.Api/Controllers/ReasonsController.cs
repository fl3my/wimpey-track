using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.Reason;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReasonsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReasonsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Reasons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReasonDto>>> GetReasons()
        {
            var reasons = await _context.Reasons.Select(r => new ReasonDto()
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync();
            
            return reasons;
        }

        // GET: api/Reasons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReasonDto>> GetReason(int id)
        {
            var reason = await _context.Reasons.FindAsync(id);

            if (reason == null)
            {
                return NotFound();
            }

            var dto = new ReasonDto()
            {
                Id = reason.Id,
                Name = reason.Name
            };
                
            return dto;
        }

        // PUT: api/Reasons/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReason(int id, UpdateReasonDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            var reason = await _context.Reasons.FindAsync(id);
            
            if (reason == null)
                return NotFound();
            
            // Check if name already exists
            var nameAlreadyExists = await _context.Reasons.AnyAsync(r => r.Name == dto.Name);
            if (nameAlreadyExists)
                return BadRequest("Reason with same name already exists");;
            
            reason.Name = dto.Name;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ReasonExistsAsync(id))
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

        // POST: api/Reasons
        [HttpPost]
        public async Task<ActionResult<ReasonDto>> PostReason(CreateReasonDto dto)
        {
            var reason = new Reason()
            {
                Name = dto.Name
            };

            // Check if name already exists
            var nameAlreadyExists = await _context.Reasons.AnyAsync(r => r.Name == dto.Name);
            if (nameAlreadyExists)
            {
                return Conflict(new { message = "Reason with same name already exists" });
            }

            _context.Reasons.Add(reason);
            
            await _context.SaveChangesAsync();

            var reasonDto = new ReasonDto()
            {
                Id = reason.Id,
                Name = reason.Name
            };
            
            return CreatedAtAction("GetReason", new { id = reasonDto.Id }, reasonDto);
        }

        // DELETE: api/Reasons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReason(int id)
        {
            var reason = await _context.Reasons.FindAsync(id);
            if (reason == null)
            {
                return NotFound();
            }

            _context.Reasons.Remove(reason);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ReasonExistsAsync(int id)
        {
            return await _context.Reasons.AnyAsync(e => e.Id == id);
        }
    }
}
