using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreApi2Sample.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return Content("AspNetCoreApi2Sample");
        }
    }
}
