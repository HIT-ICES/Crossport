using Ices.Crossport.Core;
using Microsoft.AspNetCore.Mvc;

namespace Ices.Crossport.Controllers;

[Route("app")]
public class AppController(
    AppManager appManager
) : Controller
{
    [HttpGet] public IActionResult Get() { return Ok(appManager.AppInfos.ToArray()); }

    [HttpGet("{app}/{component}/config")]
    public IActionResult GetConfig(string app, string component)
    {
        return Ok(appManager.EnsureAppComponent(new AppInfo(app, component)).Config);
    }
}