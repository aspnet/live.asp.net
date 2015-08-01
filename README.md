## live.asp.net
Code for the ASP.NET Community Stand-up site.

Staging - http://asp-standup-staging.azurewebsites.net/

Production - http://live.asp.net/

### Local Development Configuration
To run the site locally with live data and login, you'll need some configuration values in your user secrets store.
If the values aren't found, hard-coded YouTube sample data will be used, and the next show details will be saved to
the root of the app in a JSON file.

To enable sign-in to the Admin page, you'll need configuration values in your secret store for an Azure AD endpoint,
plus you'll need to update the `Authorization` section of `config.json` to list the usernames of the Azure AD accounts
you want to allow. 

To configure the secret values, use the `user-secret` command, e.g.:

```
dnu commands install Microsoft.Framework.SecretManager

user-secret set AppSettings:YouTubeApiKey <app-server-key>
  
user-secret set AppSettings:AzureStorageConnectionString <azure-storage-connection-string>

user-secret set Authentication:AzureAd:ClientId <azure-ad-client-id>

user-secret set Authentication:AzureAd:AADInstance <azure-ad-instance-url>

user-secret set Authentication:AzureAd:TenantId <azure-ad-tenant-id>

user-secret set Authentication:AzureAd:Domain <azure-ad-domain-name>

user-secret set Authentication:AzureAd:PostLogoutRedirectUri "https://localhost:44300/"
```
