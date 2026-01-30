namespace WimpeyTrack.Api.Dtos.Dashboard;

public class DashboardResponse
{
    public DashboardSummary Summary { get; set; } = null!;
    public List<MonthlyMilesDto> CumulativeMiles { get; set; } = [];
    public List<MonthlyMilesDto> MonthlyMiles { get; set; } = [];
}