using Anonymous.Crossport.Core;
using Anonymous.MetaCdn.Remoting;
using Microsoft.AspNetCore.Mvc;

namespace Anonymous.Crossport.Controllers;

[Route("/health")]
public class HealthController(
    IHostApplicationLifetime applicationLifetime,
    AppManager appManager) : ControllerBase
{
    public CancellationToken HostShutdown { get; set; } = applicationLifetime.ApplicationStopping;

    [HttpGet("{app}/{component}")] //, HttpConnect("{app}/{component}")]
    public IActionResult GetHealth
    (
        [FromRoute] string app,
        [FromRoute] string component
    )
    {
        var health = appManager.GetHealth(app, component);
        if (health == null) return NotFound();
        return Ok(health);
    }

    [HttpGet("listen")] //, HttpConnect("listen")]
    public async Task Listen()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var tsc = new TaskCompletionSource();
            using var session = IRemoting.CreateFromWebSocket
            (
                await HttpContext.WebSockets.AcceptWebSocketAsync(),
                tsc,
                HostShutdown
            );
            appManager.AddHealthListener(session);
            //await _appManager.ListenExceptions(session.ListenAsync);
            await tsc.Task;
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsync("Only WebSocket Connections are allowed.", HostShutdown);
        }
    }
}