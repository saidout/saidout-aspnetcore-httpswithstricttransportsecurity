using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity
{

    /// <summary>Extension contains method that helps in setting up a <see cref="HttpsWithStrictTransportSecurityMiddleware"/>.</summary>
    public static class HttpsWithStrictTransportSecurityExtension
    {

        /// <summary>Adds <see cref="HttpsWithStrictTransportSecurityMiddleware"/> to the <see cref="IApplicationBuilder"/> request execution pipeline. The <see cref="HttpsWithStrictTransportSecurityMiddleware"/> should be placed as early as possible in the request execution pipeline.</summary>
        /// <param name="app">The <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" />.</param>
        /// <param name="httpsMode">HttpsMode determine if redirects is allowed for GET request using http scheme. Default value is <see cref="HttpsMode.Strict"/>.</param>
        /// <param name="hstsMaxAgeInSeconds">Value max-age should be set to in HTTP Strict Transport Security (HSTS) header. Default value is one year (31 536 000 = 365 * 24 * 60 * 60).</param>
        /// <param name="hstsIncludeSubDomains">If <c>true</c> the includeSubDomains will be added to HTTP Strict Transport Security (HSTS) header.</param>
        /// <param name="httpsRedirectPort">
        /// If a GET request using http scheme should be redirected to https scheme then if the value is in range [0, 65535] it will be added as port part to the URI. If value is -1 then no port part will be added to the URI.
        /// Default value is -1.
        /// </param>
        /// <param name="configureRoutes">
        /// Routes callback should be the same as used by MVC Middleware since the <see cref="HttpsWithStrictTransportSecurityMiddleware"/> does not depend on
        /// MVC Middleware pipeline in order to determine if a GET redirection should be done for a GET request using http scheme.
        /// </param>
        /// <returns>An <see cref="IApplicationBuilder"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="hstsMaxAgeInSeconds"/> is outside the range of [0, 2 147 483 647] or <paramref name="httpsRedirectPort"/> is outside the range of [-1, 65 535].</exception>
        public static IApplicationBuilder UseHttpsWithHsts(this IApplicationBuilder app,
            HttpsMode httpsMode = HttpsMode.Strict,
            int hstsMaxAgeInSeconds = 31536000,
            bool hstsIncludeSubDomains = true,
            int httpsRedirectPort = -1,
            Action<IRouteBuilder> configureRoutes = null)
        {
            IControllerAttributeSupport controllerAttributeSupport = null;
            if (httpsMode == HttpsMode.AllowedRedirectForGet)
            {
                // Default route extracted from Microsoft.AspNetCore.Builder.MvcApplicationBuilderExtensions
                configureRoutes = configureRoutes ?? (routes => routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}"));

                var routeBuilder = new RouteBuilder(app)
                {
                    DefaultHandler = app.ApplicationServices.GetRequiredService<MvcRouteHandler>()
                };

                configureRoutes(routeBuilder);
                routeBuilder.Routes.Insert(0, AttributeRouting.CreateAttributeMegaRoute(app.ApplicationServices));
                var router = routeBuilder.Build();
                controllerAttributeSupport = new RouterControllerAttributeSupport(router);
            }

            return app.UseMiddleware<HttpsWithStrictTransportSecurityMiddleware>(new HttpsWithHstsOptions(httpsMode,
                hstsMaxAgeInSeconds,
                hstsIncludeSubDomains,
                httpsRedirectPort),
                controllerAttributeSupport);
        }
    }
}