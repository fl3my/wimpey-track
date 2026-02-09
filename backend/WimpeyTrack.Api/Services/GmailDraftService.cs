using WimpeyTrack.Api.Dtos.EmailRecipient;

namespace WimpeyTrack.Api.Services;

public interface IGmailDraftService
{
    Task<string> CreateDraftAsync(
        string fromName, 
        IEnumerable<(string Name, string Email )> recipients,
        string subject,
        string body, 
        IEnumerable<EmailAttachment> attachments
        );
}

public class GmailDraftService : IGmailDraftService
{
    public Task<string> CreateDraftAsync(string fromName, IEnumerable<(string Name, string Email)> recipients, string subject, string body, IEnumerable<EmailAttachment> attachments)
    {
        throw new NotImplementedException();
    }
}