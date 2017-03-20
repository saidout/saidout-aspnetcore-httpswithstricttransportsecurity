using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.ManualTests
{

    public class Startup
    {


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var routeAction = new Action<IRouteBuilder>(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Note if HttpsMode is set to HttpsMode.Strict then no redirection will be made on GET request using http scheme
            // on action with has the HttpGetModeAttribute with mode set to HttpGetMode.RedirectToHttps.
            app.UseHttpsWithHsts(HttpsMode.AllowedRedirectForGet, httpsRedirectPort: 44366, configureRoutes: routeAction);

            // Other middelware setups

            app.UseMvc(routeAction);
        }
    }
}