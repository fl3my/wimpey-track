using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;

namespace WimpeyTrack.Api.Controllers;

[ApiController]
[Route("api/cal")]
public class CalenderController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    
    public CalenderController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("calendar.ics")]
    public async Task<IActionResult> GetCalendar()
    {
        // Load events from the database
        
        // Convert Journeys and Trips, sort by ID until time is implemented
        // Each event is an hour
        var journeys = await _context.Journeys
            .Include(j => j.Trips)
            .ThenInclude(t => t.Location)
            .Include(j => j.Trips)
            .ThenInclude(t => t.Reason)
            .ToListAsync();
        
        // Create the calendar object
        var calendar = new Calendar()
        {
            ProductId = "-//WimpeyTrack//Calendar Feed//EN",
            Method = "PUBLISH"
        };
        
        foreach (var journey in journeys)
        {
            // All day event for total mileage
            calendar.Events.Add( new CalendarEvent()
            {
                Uid = journey.Id.ToString(),
                Summary = "Total Miles: " + journey.TotalMiles,
                Start = new CalDateTime(journey.Date),
                End = new CalDateTime(journey.Date),
                
                DtStamp = new CalDateTime(DateTime.UtcNow),
                Created = new CalDateTime(DateTime.UtcNow),
                LastModified = new CalDateTime(DateTime.UtcNow),
            });

            // As duration has not been added to app, each lasts an hour
            var tripStart = journey.Date.ToDateTime(new TimeOnly(9, 0));
            
            foreach (var trip in journey.Trips)
            {
                var tripEnd = tripStart.AddHours(1);
                
                calendar.Events.Add(new CalendarEvent()
                {
                    Uid = trip.Id.ToString(),
                    Summary = $"{trip.Location.Name} {trip.Reason.Name}",
                    Start = new CalDateTime(tripStart),
                    End = new CalDateTime(tripEnd),
                    
                    DtStamp = new CalDateTime(DateTime.UtcNow),
                    Created = new CalDateTime(DateTime.UtcNow),
                    LastModified = new CalDateTime(DateTime.UtcNow),
                });
                
                tripStart = tripEnd;
            }
        }
        
        // Serialize the calendar
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(calendar);
        
        Response.Headers.CacheControl = "no-cache";
        
        return Content(ics!, "text/calendar");
    }
}