using System;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
#if MSCC
using Microsoft.CookieCompliance;
using Microsoft.CookieCompliance.NetCore;
using Microsoft.CookieCompliance.NetCore.IPAddressResolver;
#endif
using Microsoft.Extensions.Logging;

namespace live.asp.net.Services
{
    public class CookieConsentService : IDisposable
    {
#if MSCC
        private const string _domainName = "live.asp.net";
        private const string _isRequired = "cookieComplianceIsContentRequired";
        private const string _markup = "cookieComplianceMarkup";
        private readonly ICookieConsentClient _cookieConsentClient;
        private readonly IPAddressResolver _ipAddressResolver;
        private bool _isDisposed = false;
#else
        private static readonly HtmlString _emptyHtmlString = new HtmlString("");
#endif

        public CookieConsentService(ILogger<CookieConsentService> logger)
        {
#if MSCC
            _cookieConsentClient = CookieConsentClientFactory.Create(_domainName, logger);
            _ipAddressResolver = IPAddressResolverFactory.Create(_domainName, logger);

            RefreshCookieConsentSettings();
#endif
        }

#if MSCC
        private async void RefreshCookieConsentSettings()
        {
            await _cookieConsentClient.RefreshAsync();
        }
#endif

        public bool IsConsentRequired(HttpContext context)
        {
#if !MSCC
            return false;
#else
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
#endif
        }

        public IHtmlContent GetConsentHtml(HttpContext context)
        {
#if MSCC
            return new HtmlString(GetConsentMarkup(context).Markup);
#else
            return _emptyHtmlString;
#endif
        }

        public string[] GetConsentJavaScript(HttpContext context)
        {
#if MSCC
            return GetConsentMarkup(context).Javascripts;
#else
            return Array.Empty<string>();
#endif
        }

        public string[] GetConsentStylesheets(HttpContext context)
        {
#if MSCC
            return GetConsentMarkup(context).Stylesheets;
#else
            return Array.Empty<string>();
#endif
        }

#if MSCC
        private ConsentMarkup GetConsentMarkup(HttpContext context)
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
            var header = context.Request.Headers["X-Akamai-Edgescape"].FirstOrDefault();
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

#endif

        protected virtual void Dispose(bool disposing)
        {
#if MSCC
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _cookieConsentClient.Dispose();
                    _ipAddressResolver.Dispose();
                }

                _isDisposed = true;
            }
#endif
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}