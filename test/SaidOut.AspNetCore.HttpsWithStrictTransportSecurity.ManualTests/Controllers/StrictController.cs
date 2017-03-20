using Microsoft.AspNetCore.Mvc;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.ManualTests.Controllers
{

    [HttpGetMode(HttpGetMode.ReturnForbidden)]
    public class StrictController : Controller
    {

        [HttpGet]
        public IActionResult Index()
        {
            return Ok (new { Message = "Ok" });
        }


        [HttpGet]
        [HttpPost]
        [HttpGetMode(HttpGetMode.RedirectToHttps)]
        public IActionResult Redirect()
        {
            return Ok(new { Message = "Ok" });
        }
    }
}