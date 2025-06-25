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
            var endpoint = $"{domain}ExtendedUi";

            var hrml =
@$"ExternalExtendedUiApiSample</br>
The API endpoint is: {endpoint}<br>
- with 'validate' action: {endpoint}/Claims ";

            return Content(hrml, "text/html");
        }
    }
}
