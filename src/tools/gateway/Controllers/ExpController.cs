using Ices.Crossport.Core.Diagnostics;
using Ices.Crossport.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Ices.Crossport.Controllers;

[Route("exp")]
public class ExpController(ExperimentManager experimentManager) : Controller
{
    [HttpPost("{app}")]
    public IActionResult StartExperiment([FromRoute] string app, [FromBody] ExperimentConfig config)
    {
        if (experimentManager.StartExperiment(app, config)) return NoContent();
        return BadRequest(CrossportEvents.EvaluationAlreadyRunning.Name);
    }

    [HttpPost("{app}/stats")]
    public IActionResult UploadStats([FromRoute] string app, [FromBody] ClientExpStats stats)
    {
        if (experimentManager.UploadStats(app, stats)) return NoContent();
        return BadRequest(CrossportEvents.EvaluationStatsDropped);
    }

    [HttpGet("{app}")]
    public IActionResult GetReport([FromRoute] string app)
    {
        var result = experimentManager.GetResult(app);
        return result is null ? NotFound() : Ok(result);
    }
}