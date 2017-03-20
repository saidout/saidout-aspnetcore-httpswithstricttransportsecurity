using Microsoft.AspNetCore.Mvc;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.ManualTests.Controllers
{

    public class HomeController : Controller
    {

        [HttpGet]
        [HttpGetMode(HttpGetMode.RedirectToHttps)]
        public IActionResult Index()
        {
            return Ok(new { Message = "Ok" });
        }
    }
}