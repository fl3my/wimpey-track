using Microsoft.AspNetCore.Mvc;
using WimpeyTrack.Api.Domain;
using WimpeyTrack.Api.Services;

namespace WimpeyTrack.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportGenerationService _reportService;
        private readonly IReportZipBuilder _zipBuilder;
        
        public ReportController(IReportGenerationService reportService, IReportZipBuilder zipBuilder)
        {
            _reportService = reportService;
            _zipBuilder = zipBuilder;
        }
        
        [HttpGet]
        public async Task<IActionResult> DownloadReport(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be after end date.");
            }
            
            // Generate the report
            var reportArtifacts = await _reportService.GenerateAsync(startDate, endDate);
            
            // Return bad request if nothing is found
            if (!reportArtifacts.Files.Any())
            {
                return BadRequest("No expense documents found.");
            }
            
            var zipBytes = _zipBuilder.BuildZip(reportArtifacts.Files);
            
            // Return a zip file
            return File(
                zipBytes,
                "application/zip",
                $"Expenses_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.zip"
            );
        }
    }
}