using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;

Console.WriteLine("Starting Gmail OAuth flow...");

var secrets = GoogleClientSecrets.FromFile("/Users/rossfleming/Projects/wimpey-track/backend/WimpeyTrack.GmailTokenTool/client_secret.json");

var scopes = new[]
{
    GmailService.Scope.GmailCompose
};

var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
    secrets.Secrets,
    scopes,
    "user",
    CancellationToken.None,
    new FileDataStore("token-store", true)
);

Console.WriteLine();
Console.WriteLine("Authorization successful.");
Console.WriteLine();

if (string.IsNullOrWhiteSpace(credential.Token.RefreshToken))
{
    Console.WriteLine("No refresh token returned.");
    Console.WriteLine("You may need to revoke access and re-run with prompt=consent.");
}
else
{
    Console.WriteLine($"GMAIL_CLIENT_ID={secrets.Secrets.ClientId}");
    Console.WriteLine($"GMAIL_CLIENT_SECRET={secrets.Secrets.ClientSecret}");

    if (!string.IsNullOrWhiteSpace(credential.Token.RefreshToken))
    {
        Console.WriteLine($"GMAIL_REFRESH_TOKEN={credential.Token.RefreshToken}");
    }
    else
    {
        Console.WriteLine("GMAIL_REFRESH_TOKEN=<NOT RETURNED>");
    }

}


Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();