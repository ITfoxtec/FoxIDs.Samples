using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreApi1Sample.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            var domain = HttpContext.Request.GetDisplayUrl();
            return Content($"ExternalApiLoginSample</br> The API endpoint is: {domain}ExternalApiLogin", "text/html");
        }
    }
}
