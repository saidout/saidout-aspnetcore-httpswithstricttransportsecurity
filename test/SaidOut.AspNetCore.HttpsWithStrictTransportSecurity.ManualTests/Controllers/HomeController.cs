using Microsoft.AspNetCore.Mvc;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.ManualTests.Controllers
{

    public class HomeController : Controller
    {

        // HTTP request using scheme http will be redirected to https since the action has HttpGetModeAttribute with mode set to HttpGetMode.RedirectToHttps.
        [HttpGet]
        [HttpGetMode(HttpGetMode.RedirectToHttps)]
        public IActionResult Index()
        {
            return Ok(new { Message = "Ok", Page = "Home - Index" });
        }
    }
}