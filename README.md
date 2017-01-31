## live.asp.net
Code for the ASP.NET Community Stand-up site.

Staging - https://ms-asp-standup-staging.azurewebsites.net/

Production - https://live.asp.net/

### Local Development Configuration
This project now requires [Visual Studio 2017 RC build 15.0.0-RC.3+26127.0](https://www.microsoft.com/net/core#windowsvs2017) or higher, or if using the .NET CLI with VS Code or an OmniSharp enabled editor, [.NET Core SDK 1.0 RC3 build 004530](https://github.com/dotnet/core/blob/master/release-notes/rc3-download.md) or higher.

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
