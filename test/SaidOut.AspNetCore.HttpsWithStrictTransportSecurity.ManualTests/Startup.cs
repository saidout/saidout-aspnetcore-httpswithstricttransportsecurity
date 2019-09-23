using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.ManualTests
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var routeAction = new Action<IRouteBuilder>(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/");
            });


            // Note if HttpsMode is set to HttpsMode.Strict then no redirection will be made on GET request using http scheme
            // on action with has the HttpGetModeAttribute with mode set to HttpGetMode.RedirectToHttps.
            var httpsPort = int.TryParse(Configuration["https_port"], out var tmp)
                ? tmp
                : 443;
            app.UseHttpsWithHsts(HttpsMode.AllowedRedirectForGet, httpsRedirectPort: httpsPort, configureRoutes: routeAction);

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvc(routeAction);
        }
    }
}
