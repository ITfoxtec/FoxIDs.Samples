using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ExternalPasswordApiSample.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    [Route("")]
    public ActionResult Index()
    {
        var domain = HttpContext.Request.GetDisplayUrl();
        var endpoint = $"{domain}ExternalPassword";

        var hrml =
@$"<strong>ExternalPasswordApiSample - <a href='{domain}Swagger'>Swagger UI</a></strong></br></br>
The API endpoint is: {endpoint}<br>
- with 'validation' action: {endpoint}/Validation<br>
- with 'notification' action: {endpoint}/Notification ";

        return Content(hrml, "text/html");
    }
}

