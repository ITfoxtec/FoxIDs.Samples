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
            var endpoint = $"{domain}ExternalClaims";

            var hrml =
@$"ExternalClaimsApiSample</br>
The API endpoint is: {endpoint}<br>
- with authentication action: {endpoint}/Authentication ";

            return Content(hrml, "text/html");
        }
    }
}
