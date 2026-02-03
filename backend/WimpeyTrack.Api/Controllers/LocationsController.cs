using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.Location;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDto>>> GetLocations()
        {
            var locations = await _context.Locations
                .OrderByDescending(l => l.Trips.Count())
                .Select(r => new LocationDto()
            {
                Id =  r.Id,
                Name = r.Name,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
            })
            .ToListAsync();
            
            return locations;
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDto>> GetLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            var dto = new LocationDto()
            {
                Id = location.Id,
                Name = location.Name,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
            };
            
            return dto;
        }

        // PUT: api/Locations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, UpdateLocationDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
                return NotFound();
            
            location.Name = dto.Name;
            location.Latitude = dto.Latitude;
            location.Longitude = dto.Longitude;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await LocationExistsAsync(id))
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

        // POST: api/Locations
        [HttpPost]
        public async Task<ActionResult<LocationDto>> PostLocation(CreateLocationDto dto)
        {
            var location = new Location()
            {
                Name = dto.Name,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
            };
            
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            var locationDto = new LocationDto()
            {
                Id = location.Id,
                Name = location.Name,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
            };
            
            return CreatedAtAction("GetLocation", new { id = locationDto.Id }, locationDto);
        }

        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> LocationExistsAsync(int id)
        {
            return await _context.Locations.AnyAsync(e => e.Id == id);
        }
    }
}
