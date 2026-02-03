using Microsoft.AspNetCore.Mvc;
using WimpeyTrack.Api.Dtos.Dashboard;
using WimpeyTrack.Api.Services;

namespace WimpeyTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> GetDashboard()
    {
        var response = await _dashboardService.GetDashboardAsync();
        return Ok(response);
    }
}