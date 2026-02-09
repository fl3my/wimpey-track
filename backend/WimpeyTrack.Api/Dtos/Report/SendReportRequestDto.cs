namespace WimpeyTrack.Api.Dtos.Report;

public class SendReportRequestDto
{
    public List<Guid> RecipientIds { get; set; } = []; 
}