using Microsoft.AspNetCore.Mvc;

namespace DirectoryConnectorApiSample.Controllers;

public class HomeController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => Redirect("/swagger");
}
