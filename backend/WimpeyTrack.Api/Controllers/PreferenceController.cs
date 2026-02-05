using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.Preference;

namespace WimpeyTrack.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PreferenceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public PreferenceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PreferenceDto>> GetPreference()
    {
        // Get the single preferences
        var preference = await _context.Preferences.SingleAsync();
        
        // Create the return dto
        var dto = new PreferenceDto()
        {
            MilesAdjustmentFactor = preference.MilesAdjustmentFactor
        };
        
        return Ok(dto);
    }

    [HttpPut]
    public async Task<IActionResult> PutPreference(CreatePreferenceDto dto)
    {
        // get the single preference
        var prefs = await _context.Preferences.SingleAsync();
        
        // Adjust the values
        prefs.MilesAdjustmentFactor = dto.MilesAdjustmentFactor;
        
        await _context.SaveChangesAsync();
        return NoContent();
    }
}