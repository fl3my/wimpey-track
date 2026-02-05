using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.Profile;

namespace WimpeyTrack.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var profile = await _context.Profiles.SingleAsync();

        var dto = new ProfileDto
        {
            FullName = profile.FullName,
            StaffNumber = profile.StaffNumber,
            BusinessUnit = profile.BusinessUnit,
            DepartmentSiteName = profile.DepartmentSiteName,
            VehicleFuelType = profile.VehicleFuelType,
            VehicleEngineSize = profile.VehicleEngineSize,
            VehicleRegistration = profile.VehicleRegistration,
            VehicleMake = profile.VehicleMake,
            HomePostcode = profile.HomePostcode,
            HomeLocationId = profile.HomeLocationId,
        };

        return Ok(dto);
    }

    [HttpPut]
    public async Task<IActionResult> PutProfile(CreateProfileDto dto)
    {
        var profile = await _context.Profiles.SingleAsync();
        
        // Validate that locations exist
        if (await _context.Locations.FindAsync(dto.HomeLocationId) == null)
        {
            return BadRequest(new {message = "Location not found"});
        }
        
        // Change the properties
        profile.FullName = dto.FullName;
        profile.StaffNumber = dto.StaffNumber;
        profile.BusinessUnit = dto.BusinessUnit;
        profile.DepartmentSiteName = dto.DepartmentSiteName;
        profile.VehicleFuelType = dto.VehicleFuelType;
        profile.VehicleEngineSize = dto.VehicleEngineSize;
        profile.VehicleRegistration = dto.VehicleRegistration;
        profile.VehicleMake = dto.VehicleMake;
        profile.HomePostcode = dto.HomePostcode;
        profile.HomeLocationId = profile.HomeLocationId;
        
        // Save
        await _context.SaveChangesAsync();
        return NoContent();
    }
}