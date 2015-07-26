## live.asp.net
Home of the codez for the ASP.NET Community Stand-up site.

Staging - http://asp-standup-staging.azurewebsites.net/

Production - http://live.asp.net/

### Development User Secrets
You'll need the following keys with suitable values configured in your user secrets store to develop locally:
``` JSON
"AppSettings": {
  "YouTubeApiKey": "<app-server-key>"
},
"Authentication": {
  "AzureAd": {
    "ClientId": "<client-id>",
    "AADInstance": "https://login.microsoftonline.com/",
    "PostLogoutRedirectUri": "https://localhost:44300/",
    "Domain": "microsoft.onmicrosoft.com",
    "TenantId": "<tenant-id>"
  }
}
```
