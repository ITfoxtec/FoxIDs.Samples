using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticatorAppApiSample.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    [Route("")]
    public ActionResult Index()
    {
        var baseUrl = HttpContext.Request.GetDisplayUrl();
        var endpoint = $"{baseUrl}notification";
        var html =
$@"<strong>AuthenticatorAppApiSample - <a href='{baseUrl}swagger'>Swagger UI</a></strong><br><br>
The API endpoint is: {endpoint}";

        return Content(html, "text/html");
    }
}
