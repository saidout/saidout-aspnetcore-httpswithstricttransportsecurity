using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SaidOut.DataValidation.ParameterGuard.Extensions;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity
{

    /// <summary>Middleware will make sure the https scheme is used and will add HTTP Strict Transport Security (HSTS) header on all responses to request made using https scheme.</summary>
    public class HttpsWithStrictTransportSecurityMiddleware
    {
        private const string HstsHeaderKey = "Strict-Transport-Security";
        private readonly RequestDelegate _next;
        private readonly HttpsWithHstsOptions _httpsOptions;
        private readonly IControllerAttributeSupport _controllerAttributeSupport;


        /// <summary></summary>
        /// <param name="next">The next middleware in the request execution pipeline that will only be invoked if the HTTP request made is using https scheme.</param>
        /// <param name="httpsOptions">Option controls how the middleware will behave.</param>
        /// <param name="controllerAttributeSupport">
        /// Used to determine if a GET request using http scheme should be redirected to https scheme based on if the controller action
        /// has the <see cref="HttpGetModeAttribute"/> with <see cref="HttpGetModeAttribute.Mode"/> set to <see cref="HttpGetMode.RedirectToHttps"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="next"/> or <paramref name="httpsOptions"/> is <c>null</c>.</exception>
        public HttpsWithStrictTransportSecurityMiddleware(RequestDelegate next, HttpsWithHstsOptions httpsOptions, IControllerAttributeSupport controllerAttributeSupport)
        {
            _next = next.CheckIsNotNull(nameof(next));
            _httpsOptions = httpsOptions.CheckIsNotNull(nameof(httpsOptions));
            _controllerAttributeSupport = controllerAttributeSupport;
        }


        /// <summary>
        /// If a HTTP request is using https scheme then the next middleware in the HTTP request execution pipeline will invoked and when it returns a HTTP Strict Transport Security(HSTS) header will be added to the response.
        /// 
        /// If request is a GET request using http scheme then it will be redirect to https scheme if the <see cref="HttpsWithHstsOptions.HttpsMode"/> is set to <see cref="HttpsMode.AllowedRedirectForGet"/> and
        /// the controller action method that should be invoked has the <see cref="HttpGetModeAttribute"/> with <see cref="HttpGetModeAttribute.Mode"/> value set to <see cref="HttpGetMode.RedirectToHttps"/>.
        /// 
        /// For all other request HTTP Forbidden (403) will be returned.
        /// </summary>
        /// <param name="context">The context associated with a http request.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.IsHttps)
            {
                await _next.Invoke(context);
                AddHttpStrictTransportHeaderToResponse(context);
                return;
            }

            if (_httpsOptions.HttpsMode == HttpsMode.AllowedRedirectForGet
                && string.Equals(context.Request.Method, "GET", StringComparison.Ordinal)
                && string.Equals(context.Request.Scheme, "http"))
            {
                if (_controllerAttributeSupport != null)
                {
                    var httpGetModeAttribute = await _controllerAttributeSupport.GetActionAttributeAsync<HttpGetModeAttribute>(context);
                    if (httpGetModeAttribute != null && httpGetModeAttribute.Mode == HttpGetMode.RedirectToHttps)
                    {
                        var redirectTo = _httpsOptions.HttpsRedirectPort == -1
                            ? $"https://{context.Request.Host.Host}{context.Request.Path}{context.Request.QueryString.Value}"
                            : $"https://{context.Request.Host.Host}:{_httpsOptions.HttpsRedirectPort}{context.Request.Path}{context.Request.QueryString.Value}";
                        context.Response.StatusCode = (int)HttpStatusCode.MovedPermanently;
                        context.Response.Headers.Add("Location", new StringValues(redirectTo));
                        return;
                    }

                }
            }

            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync($"Scheme {context.Request.Scheme} is not allowed. Only https scheme is allowed.", Encoding.UTF8);
        }


        private void AddHttpStrictTransportHeaderToResponse(HttpContext context)
        {
            var hstsValue = _httpsOptions.HstsIncludeSubDomains
                ? $"max-age={_httpsOptions.HstsMaxAgeInSeconds}; includeSubDomains"
                : $"max-age={_httpsOptions.HstsMaxAgeInSeconds}";

            context.Response.Headers.Add(HstsHeaderKey, new[] { hstsValue });
        }
    }
}