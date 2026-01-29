using Microsoft.AspNetCore.Mvc;
using WimpeyTrack.Api.Domain;
using WimpeyTrack.Api.Services;

namespace WimpeyTrack.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        
        [HttpPost]
        public async Task<IActionResult> Generate(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be after end date.");
            }
            
            // Generate the report
            var reportId = await _reportService.GenerateAndSaveAsync(startDate, endDate);
            return Ok(new {reportId});
        }
    }
}