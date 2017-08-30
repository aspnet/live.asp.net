using Microsoft.AspNetCore.Http;
using Microsoft.CookieCompliance;
using Microsoft.CookieCompliance.NetCore;
using Microsoft.CookieCompliance.NetCore.IPAddressResolver;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace live.asp.net.Services
{
    public class CookieConsentService : IDisposable
    {
        private const string _domainName = "live.asp.net";
        private readonly ICookieConsentClient _cookieConsentClient;
        private readonly IPAddressResolver _ipAddressResolver;
        private bool _isDisposed = false;
        private const string _isRequired = "cookieComplianceIsContentRequired";
        private const string _markup = "cookieComplianceMarkup";

        public CookieConsentService(ILogger<CookieConsentService> logger)
        {
            _cookieConsentClient = CookieConsentClientFactory.Create(_domainName, logger);
            _ipAddressResolver = IPAddressResolverFactory.Create(_domainName, logger);

            RefreshCookieConsentSettings();
        }

        private async void RefreshCookieConsentSettings()
        {
            await _cookieConsentClient.RefreshAsync();
        }

        public bool IsConsentRequired(HttpContext context)
        {
            if (context.Items.ContainsKey(_isRequired))
            {
                return (bool)context.Items[_isRequired];
            }

            try
            {
                var countryCode = GetCountryCode(context);
                var isRequired = string.IsNullOrWhiteSpace(countryCode)
                    ? false
                    : _cookieConsentClient.IsConsentRequiredForRegion(_domainName, countryCode, context) == ConsentStatus.Required;

                context.Items[_isRequired] = isRequired;
                return isRequired;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ConsentMarkup GetConsentMarkup(HttpContext context)
        {
            if (context.Items.ContainsKey(_markup))
            {
                return (ConsentMarkup)context.Items[_markup];
            }

            var markup = _cookieConsentClient.GetConsentMarkup("en-us");

            context.Items[_markup] = markup;
            return markup;
        }

        private string GetCountryCode(HttpContext context)
        {
            // Passing via URL allows us to validate without having to change headers or IP resolution code
            var urlParameter = context.Request.Query["country"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(urlParameter))
            {
                return urlParameter;
            }

            // This header is set (by microsoft.com infrastructure) when the site is accessed via a microsoft.com URL
            var header = context.Request.Headers["X -Akamai-Edgescape"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(header))
            {
                var start = header.IndexOf("country_code=");
                if (start >= 0 && header.Length >= start + 15)
                {
                    return header.Substring(start + 13, 2);
                }
            }

            var ip = context.Connection.RemoteIpAddress.ToString();
            return _ipAddressResolver.GetCountryCode(ip);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _cookieConsentClient.Dispose();
                    _ipAddressResolver.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
