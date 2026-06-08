using System.Diagnostics;
using AspNetCoreWsFedSample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCoreWsFedSample.Controllers;

public class HomeController : Controller
{
    private readonly WsFederationSettings wsFederationSettings;

    public HomeController(IOptionsMonitor<WsFederationSettings> wsFederationSettings)
    {
        this.wsFederationSettings = wsFederationSettings.CurrentValue;
    }

    public IActionResult Index()
    {
        ViewBag.WsFederationSettings = wsFederationSettings;
        ViewBag.SiteUrl = $"{Request.Scheme}://{Request.Host.ToUriComponent()}";
        return View();
    }

    [Authorize]
    public IActionResult Secure()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
