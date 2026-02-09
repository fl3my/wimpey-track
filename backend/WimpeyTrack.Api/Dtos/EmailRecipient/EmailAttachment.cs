namespace WimpeyTrack.Api.Dtos.EmailRecipient;

public class EmailAttachment
{
    public string FileName { get; set; }= string.Empty;
    public Stream Content { get; set; } = null!;
    public string ContentType { get; set; } = string.Empty;
    
}