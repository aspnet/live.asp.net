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
        private const string _domainName = "live.asp.com";
        private readonly ICookieConsentClient _cookieConsentClient;
        private readonly IPAddressResolver _ipAddressResolver;
        private bool _isDisposed = false;

        public CookieConsentService(ILogger<CookieConsentService> logger)
        {
            _cookieConsentClient = CookieConsentClientFactory.Create(_domainName, logger);
            _ipAddressResolver = IPAddressResolverFactory.Create(_domainName, logger);
        }

        public bool IsConsentRequired(HttpContext context)
        {
            try
            {
                var countryCode = GetCountryCode(context);

                return string.IsNullOrWhiteSpace(countryCode)
                    ? false
                    : _cookieConsentClient.IsConsentRequiredForRegion(_domainName, countryCode, context) == ConsentStatus.Required;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetCountryCode(HttpContext context)
        {
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
