using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using MimeKit;
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
    private readonly IConfiguration _configuration;

    public GmailDraftService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<string> CreateDraftAsync(string fromName, IEnumerable<(string Name, string Email)> recipients, string subject, string body, IEnumerable<EmailAttachment> attachments)
    {
        var clientEmail = _configuration["Gmail:ClientEmail"] ?? throw new InvalidOperationException("Gmail client email is missing.");
        
        // Get the gmail service
        var gmailService = await GetGmailServiceAsync();
        
        // Build a mime message
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, clientEmail));

        // Add the recipients
        foreach (var (name, email) in recipients)
        {
            message.To.Add(new MailboxAddress(name, email));
        }
        
        message.Subject = subject;
        
        // Build the body
        var builder = new BodyBuilder{HtmlBody = body};
        foreach (var attachment in attachments)
        {
            await builder.Attachments.AddAsync(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        }
        message.Body = builder.ToMessageBody();
        
        // Convert to base 64
        using var stream = new MemoryStream();
        await message.WriteToAsync(stream);
        var raw = Convert.ToBase64String(stream.ToArray()).Replace('+', '-').Replace('/', '_').Replace("=", "");

        var draft = new Google.Apis.Gmail.v1.Data.Draft()
        {
            Message = new Google.Apis.Gmail.v1.Data.Message { Raw = raw }
        };
        
        var created = await gmailService.Users.Drafts.Create(draft, "me").ExecuteAsync();

        return created.Id!;
    }

    private async Task<GmailService> GetGmailServiceAsync()
    {
        // Get Client secrets
        var clientId = _configuration["Gmail:ClientId"];
        var clientSecret = _configuration["Gmail:ClientSecret"];
        var refreshToken = _configuration["Gmail:RefreshToken"]; 

        // Validate that they exist
        if (string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new InvalidOperationException("Gmail configuration is missing.");
        }
        
        // Get the credentials
        var credential = new UserCredential(
            new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                    },
                    Scopes = new[]
                    {
                        GmailService.Scope.GmailCompose
                    }
                }),
            "me",
            new TokenResponse
            {
                RefreshToken = refreshToken
            }
        );

        await credential.RefreshTokenAsync(CancellationToken.None);

        return new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "WimpeyTrack"
        });
    }
    
}