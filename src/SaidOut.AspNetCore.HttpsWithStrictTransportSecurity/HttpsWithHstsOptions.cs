using System;
using SaidOut.Common.Extensions;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity
{

    /// <summary>Class is immutable containing options that affect the behavior of <see cref="HttpsWithStrictTransportSecurityMiddleware"/>.</summary>
    public class HttpsWithHstsOptions
    {

        /// <summary></summary>
        /// <param name="httpsMode">HttpsMode determine if redirects is allowed for GET request using http scheme.</param>
        /// <param name="hstsMaxAgeInSeconds">Value max-age should be set to in HTTP Strict Transport Security (HSTS) header.</param>
        /// <param name="hstsIncludeSubDomains">If <c>true</c> the includeSubDomains will be added to HTTP Strict Transport Security (HSTS) header.</param>
        /// <param name="httpsRedirectPort">If a GET request using http scheme should be redirected to https scheme then if the value is in range [0, 65535] it will be added as port part to the URI. If value is -1 then no port part will be added to the URI.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="hstsMaxAgeInSeconds"/> is outside the range of [0, 2 147 483 647] or <paramref name="httpsRedirectPort"/> is outside the range of [-1, 65 535].</exception>
        public HttpsWithHstsOptions(HttpsMode httpsMode, int hstsMaxAgeInSeconds, bool hstsIncludeSubDomains,  int httpsRedirectPort)
        {
            httpsMode.ThrowIfEnumValueIsNotDefined(nameof(httpsMode));
            hstsMaxAgeInSeconds.ThrowIfValueIsOutsideRange(0, int.MaxValue, nameof(hstsMaxAgeInSeconds));
            httpsRedirectPort.ThrowIfValueIsOutsideRange(-1, 65535, nameof(httpsRedirectPort));

            HttpsMode = httpsMode;
            HstsMaxAgeInSeconds = hstsMaxAgeInSeconds;
            HstsIncludeSubDomains = hstsIncludeSubDomains;
            HttpsRedirectPort = httpsRedirectPort;
        }


        /// <summary>HttpsMode determine if redirects is allowed for GET request using http scheme.</summary>
        public HttpsMode HttpsMode { get; private set; }

        /// <summary>Value max-age should be set to in HTTP Strict Transport Security (HSTS) header.</summary>
        public int HstsMaxAgeInSeconds { get; private set; }

        /// <summary>If <c>true</c> then includeSubDomains will be added to HTTP Strict Transport Security (HSTS) header.</summary>
        public bool HstsIncludeSubDomains { get; private set; }

        /// <summary>If a GET request using http sheme should be redirected to https seheme then if the value is in range [0, 65535] it will be added as port to the URI. If value is -1 then no port value will be added to the URI.</summary>
        public int HttpsRedirectPort { get; private set; }
    }
}