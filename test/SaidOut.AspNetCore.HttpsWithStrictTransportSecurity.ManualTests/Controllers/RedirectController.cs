using Microsoft.AspNetCore.Mvc;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.ManualTests.Controllers
{

    [HttpGetMode(HttpGetMode.RedirectToHttps)]
    public class RedirectController : Controller
    {

        // HTTP request using scheme http will be redirected  to https since the Controller class has HttpGetModeAttribute with mode set to HttpGetMode.RedirectToHttps.
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new { Message = "Ok", Page = "Redirect - Index" });
        }


        // HTTP request using scheme http will result in Forbidden 403 being returned since action has HttpGetModeAttribute with mode set to HttpGetMode.ReturnForbidden.
        [HttpGet]
        [HttpGetMode(HttpGetMode.ReturnForbidden)]
        public IActionResult Forbidden()
        {
            return Ok(new { Message = "Ok", Page = "Redirect - Forbidden" });
        }
    }
}