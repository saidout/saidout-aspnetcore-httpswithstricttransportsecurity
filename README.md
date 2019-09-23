
# SaidOut.AspNetCore.HttpsWithStrictTransportSecurity [![NuGet Version](https://img.shields.io/nuget/v/SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.svg?style=flat)](https://www.nuget.org/packages/SaidOut.AspNetCore.HttpsWithStrictTransportSecurity/)
ASP.NET Core middleware that enforce that https scheme is used and that adds the HTTP Strict Transport (HSTS) header for all HTTP responses to request made using https scheme.


---
## Table of Content
 * [Middleware behavior](#middleware-behavior)
 * [UseHttpsWithHsts parameters](#usehttpswithhsts-parameters)
 * [Recommendation](#recommendation)
 * [Example Code](#example-code)

# Middleware behavior
If a HTTP request is using https scheme then the next middleware in the HTTP request execution pipeline will invoked and when it returns a HTTP Strict Transport Security(HSTS) header will be added to the response.  

If request is a GET request using http scheme then it will be redirect to https scheme if the `HttpsWithHstsOptions.HttpsMode` is set to `HttpsMode.AllowedRedirectForGet` and
the controller action method that should be invoked has the `HttpGetModeAttribute` with `mode` value set to `HttpGetMode.RedirectToHttps`.  

For all other request HTTP Forbidden (403) will be returned.  


---
## UseHttpsWithHsts parameters
| Parameter | Description |
|--------|-------------|
| `httpsMode` | HttpsMode determine if redirects is allowed for GET request using http scheme. Default value is `HttpsMode.Strict`. |
| `hstsMaxAgeInSeconds` | Value max-age should be set to in HTTP Strict Transport Security (HSTS) header. Default value is one year (`31 536 000` = 365 * 24 * 60 * 60). |
| `hstsIncludeSubDomains` | If `true` the includeSubDomains will be added to HTTP Strict Transport Security (HSTS) header. Default value is `true` |
| `httpsRedirectPort` | If a GET request using http scheme should be redirected to https scheme then if the value is in range [0, 65535] it will be added as port part to the URI. If value is -1 then no port part will be added to the URI. Default value is `-1`. |
| `configureRoutes` | Routes callback should be the same as used by MVC Middleware since the `HttpsWithStrictTransportSecurityMiddleware` does not depend on MVC Middleware pipeline in order to determine if a GET redirection should be done for a GET request using http scheme. |

### Recommendation
Recommend that the `httpsMode` is set to `HpptsMode.Strict` for a pure API Web site. The middleware will return HTTP Forbidden (403) for all HTTP requests
that is not made using https scheme.  
For consumers of the API they will easy detect mistakes where they are using http scheme instead of https as any request will result in a HTTP Forbidden (403) response.  

For a Web site intended to be consumed by end user the recommendation is to set `httpsMode` to `HpptsMode.AllowedRedirectForGet` and for certain GET actions add
the `HttpGetModeAttribute` with mode set to `HttpGetMode.RedirectToHttps` in order for request using http scheme to be redirected to https scheme (for example the Home page of the site).


---
## Example Code

Startup.cs
```
    public class Startup
    {

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var routeAction = new Action<IRouteBuilder>(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Note if HttpsMode is set to HttpsMode.Strict then no redirection will be made on GET request using http scheme
            // on action with has the HttpGetModeAttribute with mode set to HttpGetMode.RedirectToHttps.
            app.UseHttpsWithHsts(HttpsMode.AllowedRedirectForGet, configureRoutes: routeAction);

            // Other middelware setups

            app.UseMvc(routeAction);
        }
```

Home.cs
```
    public class HomeController : Controller
    {

        // HTTP request using scheme http will be redirected to https since the action has HttpGetModeAttribute with mode set to HttpGetMode.RedirectToHttps.
        [HttpGet]
        [HttpGetMode(HttpGetMode.RedirectToHttps)]
        public IActionResult Index()
        {
            return Ok(new { Message = "Ok" });
        }
    }
```

RedirectController.cs
```
    [HttpGetMode(HttpGetMode.RedirectToHttps)]
    public class RedirectController : Controller
    {

        // HTTP request using scheme http will be redirected to https since the Controller class has HttpGetModeAttribute with mode set to HttpGetMode.RedirectToHttps.
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new { Message = "Ok" });
        }


        // HTTP request using scheme http will result in Forbidden 403 being returned since action has HttpGetModeAttribute with mode set to HttpGetMode.ReturnForbidden.
        [HttpGet]
        [HttpGetMode(HttpGetMode.ReturnForbidden)]
        public IActionResult Forbidden()
        {
            return Ok(new { Message = "Ok" });
        }
    }
```
