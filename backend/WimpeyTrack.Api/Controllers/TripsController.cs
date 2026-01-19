using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos;
using WimpeyTrack.Api.Models;
using WimpeyTrack.Api.Services;

namespace WimpeyTrack.Api.Controllers
{
    [Route("api/journeys/{journeyId}/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRouteService _routeService;

        public TripsController(ApplicationDbContext context, IRouteService routeService)
        {
            _context = context;
            _routeService = routeService;
        }

        // GET: api/Journeys/1/Trips
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripDto>>> GetTrips(int journeyId)
        {
            if (!await _context.Journeys.AnyAsync(t => t.Id == journeyId))
                return NotFound();
            
            var trips = await _context.Trips
                .Where(t => t.JourneyId == journeyId)
                .OrderBy(t => t.Id)
                .Select(t => new TripDto()
                {
                    Id = t.Id,
                    LocationId = t.LocationId,
                    ReasonId = t.ReasonId,
                })
                .ToListAsync();

            return trips;
        }

        // GET: api/Trips/5
        [HttpGet("{tripId}")]
        public async Task<ActionResult<Trip>> GetTrip(int journeyId, int tripId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.JourneyId == journeyId && t.Id == tripId);

            if (trip == null)
            {
                return NotFound();
            }

            return trip;
        }

        // PUT: api/Journeys/1/Trips/5
        [HttpPut("{tripId}")]
        public async Task<IActionResult> PutTrip(int journeyId, int tripId, UpdateTripDto dto)
        {
            if (dto.Id != tripId)
            {
                return BadRequest();
            }
            
            if (!await _context.Trips.AnyAsync(t => t.JourneyId == journeyId && t.Id == tripId))
                return BadRequest();

            if(!await _context.Locations.AnyAsync(t => t.Id == dto.LocationId))
                return BadRequest();
            
            if (!await _context.Reasons.AnyAsync(t => t.Id == dto.ReasonId))
                return BadRequest();
            
            var trip =  await _context.Trips.FindAsync(tripId);
            if (trip == null)
                return NotFound();
            
            trip.LocationId = dto.LocationId;
            trip.ReasonId = dto.ReasonId;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TripExistsAsync(tripId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            await RecalculateJourneyDistanceAsync(journeyId);
            
            return NoContent();
        }

        // POST: api/Journeys/1/Trips
        [HttpPost]
        public async Task<ActionResult<Trip>> PostTrip(int journeyId, CreateTripDto dto)
        {
            if (!await _context.Journeys.AnyAsync(t => t.Id == journeyId))
                return NotFound();
            
            if(!await _context.Locations.AnyAsync(t => t.Id == dto.LocationId))
                return BadRequest();
            
            if (!await _context.Reasons.AnyAsync(t => t.Id == dto.ReasonId))
                return BadRequest();
            
            var trip = new Trip()
            {
                LocationId = dto.LocationId,
                ReasonId = dto.ReasonId,
                JourneyId = journeyId,
            };
            
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            
            await RecalculateJourneyDistanceAsync(journeyId);
                
            var tripDto = new TripDto()
            {
                Id = trip.Id,
                LocationId = trip.LocationId,
                ReasonId = trip.ReasonId,
            };
            
            return CreatedAtAction("GetTrip", new { journeyId = journeyId, tripId = tripDto.Id }, tripDto);
        }

        // DELETE: api/Journeys/1/Trips/5
        [HttpDelete("{tripId}")]
        public async Task<IActionResult> DeleteTrip(int journeyId, int tripId)
        {
            var trip = await _context.Trips.FindAsync(tripId);
            if (trip == null)
            {
                return NotFound();
            }

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            await RecalculateJourneyDistanceAsync(journeyId);
            
            return NoContent();
        }

        private async Task<bool> TripExistsAsync(int id)
        {
            return await _context.Trips.AnyAsync(e => e.Id == id);
        }
        
        private async Task RecalculateJourneyDistanceAsync(int journeyId)
        {
            var journey = await _context.Journeys
                .Include(j => j.HomeLocation)
                .FirstOrDefaultAsync(j => j.Id == journeyId);
    
            if (journey == null || journey.IsManualMiles)
                return;
    
            // Get coordinates - use anonymous object first
            var coordinates = await _context.Trips
                .Where(t => t.JourneyId == journeyId)
                .OrderBy(t => t.Id)
                .Select(t => new 
                {
                    Latitude = t.Location.Latitude,
                    Longitude = t.Location.Longitude
                })
                .ToListAsync();

            if (coordinates.Count == 0)
            {
                journey.TotalMiles = 0;
            }
            else
            {
                // Add home location at the start and end
                var homeCoord = (journey.HomeLocation.Latitude, journey.HomeLocation.Longitude);
        
                var coordList = new List<(double, double)> { homeCoord };
        
                // Convert anonymous objects to tuples
                coordList.AddRange(coordinates.Select(c => (c.Latitude, c.Longitude)));
        
                coordList.Add(homeCoord);

                var distance = await _routeService.CalculateAllTripsDistancesAsync(coordList);
                journey.TotalMiles = distance;
            }

            await _context.SaveChangesAsync();
        }
    }
}
