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
            return Content($"ExternalLoginApiSample</br> The API endpoint is: {domain}ExternalLoginApi", "text/html");
        }
    }
}
