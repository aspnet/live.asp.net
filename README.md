## live.asp.net
Code for the ASP.NET Community Stand-up site.

Staging - https://ms-asp-standup-staging.azurewebsites.net/

Production - https://live.asp.net/

### Local Development Configuration
To run the site locally with live data and login, you'll need some configuration values in your user secrets store.
If the values aren't found, hard-coded YouTube sample data will be used, and the next show details will be saved to
the root of the app in a JSON file.

To enable sign-in to the Admin page, you'll need configuration values in your secret store for an Azure AD endpoint,
plus you'll need to update the `Authorization` section of `appsettings.json` to list the usernames of the Azure AD accounts
you want to allow. 

To configure the secret values, use the `user-secret` command, e.g.:

```
dotnet user-secrets set AppSettings:YouTubeApiKey <app-server-key>
  
dotnet user-secrets set AppSettings:AzureStorageConnectionString <azure-storage-connection-string>

dotnet user-secrets set Authentication:AzureAd:Domain <azure-ad-domain-name>

dotnet user-secrets set Authentication:AzureAd:PostLogoutRedirectUri "https://localhost:44300/"
```
