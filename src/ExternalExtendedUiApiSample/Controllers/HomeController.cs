using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreApi1Sample.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            var domain = HttpContext.Request.GetDisplayUrl();
            var endpoint = $"{domain}ExtendedUi";

            var hrml =
@$"<strong>ExternalExtendedUiApiSample - <a href='{domain}Swagger'>Swagger UI</a></strong></br></br>
The API endpoint is: {endpoint}<br>
- with 'validate' action: {endpoint}/Validate ";

            return Content(hrml, "text/html");
        }
    }
}
