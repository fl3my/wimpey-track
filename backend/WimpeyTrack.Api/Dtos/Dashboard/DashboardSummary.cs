namespace WimpeyTrack.Api.Dtos.Dashboard;

public class DashboardSummary
{
    public decimal TotalClaimedThisTaxYear { get; set; }
    public decimal TotalClaimedThisMonth { get; set; }
    public int TotalMileageThisTaxYear { get; set; }
}