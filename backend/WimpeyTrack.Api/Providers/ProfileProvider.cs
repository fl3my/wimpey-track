using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;

namespace WimpeyTrack.Api.Providers;

public interface IProfileProvider
{
    Task<ProfileSnapshot> GetProfileAsync();
    Task<int> GetHomeLocationIdAsync();
}

public class ProfileProvider : IProfileProvider
{
    private readonly ApplicationDbContext _context;
    public ProfileProvider(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<ProfileSnapshot> GetProfileAsync()
    {
        return await _context.Profiles
            .Select(p => new ProfileSnapshot(
                p.FullName,
                p.StaffNumber,
                p.BusinessUnit,
                p.DepartmentSiteName,
                p.VehicleFuelType,
                p.VehicleEngineSize,
                p.VehicleRegistration,
                p.VehicleMake,
                p.HomePostcode,
                p.HomeLocationId
            ))
            .FirstAsync();
    }

    public async Task<int> GetHomeLocationIdAsync()
    {
        return await _context.Profiles
            .Select(p => p.HomeLocationId)
            .FirstAsync();
    }
}