using Microsoft.AspNetCore.Mvc;
using WimpeyTrack.Api.Dtos;
using WimpeyTrack.Api.Dtos.Report;
using WimpeyTrack.Api.Models;
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
        
        // GET: api/Reasons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetReports()
        {
            var reports = await _reportService.GetReportsAsync();
            return Ok(reports);
        }

        [HttpPost]
        public async Task<ActionResult<GenerateReportDto>> Generate(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be after end date.");
            }

            // Generate the report
            var reportId = await _reportService.GenerateAndSaveAsync(startDate, endDate);
            if (reportId == null)
                return BadRequest("No journeys in the date range.");

            var dto = new GenerateReportDto()
            {
                reportId = reportId.Value.ToString()
            };
            
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _reportService.DeleteAsync(id);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReportPreviewDto>> GetReportPreview(Guid id)
        {
            var preview = await _reportService.GetReportPreview(id);
            
            if (preview is null)
                return NotFound();
            
            return Ok(preview);
        }
    }
}