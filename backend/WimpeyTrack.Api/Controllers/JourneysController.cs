using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JourneysController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JourneysController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Journeys
        [HttpGet]
        public async Task<ActionResult<JourneyByWeekDto>> GetJourneysByWeek([Required] DateOnly weekStart)
        {
            var weekEnd = weekStart.AddDays(7);
            
            var journeys = await _context.Journeys
                .Include(j => j.Trips)
                .ThenInclude(t => t.Location)
                .Include(j => j.Trips)
                .ThenInclude(t => t.Reason)
                .Where(j => j.Date >= weekStart && j.Date < weekEnd)
                .OrderBy(j => j.Date)
                .Select(j => new JourneyDto() 
                { 
                    Id = j.Id, 
                    Date =  j.Date, 
                    TotalMiles =  j.TotalMiles, 
                    IsManualMiles =  j.IsManualMiles, 
                    HomeLocationId =  j.HomeLocationId,
                    Trips = j.Trips
                        .Select(t => new JourneyTripDto()
                    {
                        Id = t.Id,
                        LocationName = t.Location.Name,
                        ReasonName = t.Reason.Name
                    })
                        .OrderBy(t => t.Id)
                        .ToList()
                }).ToListAsync();
            
            return new JourneyByWeekDto()
            {
                Journeys = journeys,
                StartDate = weekStart,
                EndDate = weekEnd
            };
        }

        // GET: api/Journeys/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JourneyDto>> GetJourney(int id)
        {
            var journey = await _context.Journeys.FindAsync(id);

            if (journey == null)
            {
                return NotFound();
            }
            
            var dto = new JourneyDto()
            {
                Id = journey.Id,
                Date = journey.Date,
                TotalMiles = journey.TotalMiles,
                IsManualMiles = journey.IsManualMiles,
                HomeLocationId = journey.HomeLocationId,
            };
            
            return dto;
        }

        // PUT: api/Journeys/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJourney(int id, UpdateJourneyDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            var  journey = await _context.Journeys.FindAsync(id);
            if (journey == null)
            {
                return NotFound();
            }

            if (! await _context.Locations.AnyAsync(l => l.Id == dto.HomeLocationId))
                return BadRequest();
            
            journey.Date = dto.Date;
            journey.HomeLocationId = dto.HomeLocationId;
            
            if (dto.IsManualMiles)
            {
                journey.IsManualMiles = true;
                journey.TotalMiles = dto.TotalMiles ?? 0;
            }
            else
            {
                journey.IsManualMiles = dto.IsManualMiles;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await JourneyExistsAsync(id))
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

        // POST: api/Journeys
        [HttpPost]
        public async Task<ActionResult<JourneyDto>> PostJourney(CreateJourneyDto dto)
        {
            if (! await _context.Locations.AnyAsync(l => l.Id == dto.HomeLocationId))
                return BadRequest();
            
            var journey = new Journey()
            {
                Date = dto.Date,
                HomeLocationId = dto.HomeLocationId,
            };

            if (dto.IsManualMiles)
            {
                journey.IsManualMiles = true;
                journey.TotalMiles = dto.TotalMiles ?? 0;
            }
            
            _context.Journeys.Add(journey);
            await _context.SaveChangesAsync();

            var journeyDto = new JourneyDto()
            {
                Id = journey.Id,
                Date = journey.Date,
                TotalMiles = journey.TotalMiles,
                IsManualMiles = journey.IsManualMiles,
                HomeLocationId = journey.HomeLocationId,
            };
            
            return CreatedAtAction("GetJourney", new { id = journeyDto.Id }, journeyDto);
        }

        // DELETE: api/Journeys/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJourney(int id)
        {
            var journey = await _context.Journeys.FindAsync(id);
            if (journey == null)
            {
                return NotFound();
            }

            _context.Journeys.Remove(journey);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> JourneyExistsAsync(int id)
        {
            return await _context.Journeys.AnyAsync(e => e.Id == id);
        }
    }
}
