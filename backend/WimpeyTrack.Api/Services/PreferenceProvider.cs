using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;

namespace WimpeyTrack.Api.Services;

public interface IPreferenceProvider
{
    Task<double> GetMilesAdjustmentFactorAsync();
}

public class PreferenceProvider : IPreferenceProvider
{
    private readonly ApplicationDbContext _context;
    
    public PreferenceProvider(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<double> GetMilesAdjustmentFactorAsync()
    {
        return await _context.Preferences
            .Select(p => p.MilesAdjustmentFactor)
            .SingleAsync();
    }
}