using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.Journey;
using WimpeyTrack.Api.Models;
using WimpeyTrack.Api.Providers;
using WimpeyTrack.Api.Services;

namespace WimpeyTrack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JourneysController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJourneyDistanceService _distanceService;
        private readonly IProfileProvider _profileProvider;

        public JourneysController(ApplicationDbContext context, IJourneyDistanceService distanceService, IProfileProvider profileProvider)
        {
            _context = context;
            _distanceService = distanceService;
            _profileProvider = profileProvider;
        }

        // GET: api/Journeys
        [HttpGet]
        public async Task<ActionResult<JourneyByWeekDto>> GetJourneysByWeek(DateOnly? weekStart)
        {
            var resolvedWeekStart =
                weekStart.HasValue
                    ? GetWeekStartMonday(weekStart.Value)
                    : GetWeekStartMonday(DateOnly.FromDateTime(DateTime.UtcNow));

            
            var weekEnd = resolvedWeekStart.AddDays(7);
            
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
            
            var days = Enumerable.Range(0, 7).Select(offset =>
            {
                var date = resolvedWeekStart.AddDays(offset);
                
                var journeyForDay = journeys
                    .FirstOrDefault(j => j.Date == date);


                return new JourneyDayDto()
                {
                    Date = date,
                    Journey = journeyForDay
                };
            }).ToList();
           
            return new JourneyByWeekDto()
            {
                WeekStart = resolvedWeekStart,
                PrevWeekStart = resolvedWeekStart.AddDays(-7),
                NextWeekStart = resolvedWeekStart.AddDays(7),
                Days = days
            };
        }

        private static DateOnly GetWeekStartMonday(DateOnly date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var diff = dayOfWeek == 0 ? -6 : 1 -dayOfWeek;
            return date.AddDays(diff);
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
                journey.TotalMiles = dto.TotalMiles;
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
            var homeLocationId = await _profileProvider.GetHomeLocationIdAsync();
            
            // Do not allow journeys with the same date
            if (await _context.Journeys.AnyAsync(j => j.Date == dto.Date))
            {
                return BadRequest(new {message = "Journey on same date exists"});
            }
            
            // Home location must be a valid location
            if (! await _context.Locations.AnyAsync(l => l.Id == homeLocationId))
                return BadRequest(new { message = "Home Location not found" });
            
            // Get list of trips and reasons from request
            var tripLocationIds = dto.Trips.Select(t => t.LocationId).Distinct().ToList();
            var tripReasonIds = dto.Trips.Select(t => t.ReasonId).Distinct().ToList();
           
            // Validate that locations exist
            var validLocationIds = await _context.Locations
                .Where(l => tripLocationIds.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync();

            if (validLocationIds.Count != tripLocationIds.Count)
            {
                return BadRequest(new { message="One or more trip locations do not exist." });
            }
            
            // Validate that reasons exist
            var validReasonIds = await _context.Reasons
                .Where(r => tripReasonIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync();

            if (validReasonIds.Count != tripReasonIds.Count)
            {
                return BadRequest(new {message = "One or more trip reasons do not exist."});
            }
            
            var journey = new Journey()
            {
                Date = dto.Date,
                HomeLocationId = homeLocationId,
                Trips = dto.Trips.Select(t => new Trip()
                {
                    LocationId = t.LocationId,
                    ReasonId = t.ReasonId
                }).ToList()
            };
            
            journey.IsManualMiles = false;
            
            _context.Journeys.Add(journey);
            await _context.SaveChangesAsync();

            // Recalculate the total mileage
            await _distanceService.RecalculateMilesAsync(journey.Id);
            
            // Requery wth navigational properties
            journey = await _context.Journeys
                .Include(j => j.Trips)
                .ThenInclude(t => t.Location)
                .Include(j => j.Trips)
                .ThenInclude(t => t.Reason)
                .FirstAsync(j => j.Id == journey.Id);
            
            var journeyDto = new JourneyDto()
            {
                Id = journey.Id,
                Date = journey.Date,
                TotalMiles = journey.TotalMiles,
                IsManualMiles = journey.IsManualMiles,
                HomeLocationId = journey.HomeLocationId,
                Trips = journey.Trips.Select(t => new JourneyTripDto()
                {
                    Id =  t.Id,
                    LocationName = t.Location.Name,
                    ReasonName = t.Reason.Name
                }).ToList()
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
