using Ices.Crossport.Core;
using Ices.Crossport.Core.Entities;
using Ices.Crossport.Core.Signalling;
using Microsoft.AspNetCore.Mvc;

namespace Ices.Crossport.Controllers;

[Route("sig")]
public class SigController(
    IHostApplicationLifetime applicationLifetime,
    DiagnosticSignallingHandlerFactory diagnosticFactory,
    AppManager appManager) : Controller
{
    private CancellationToken HostShutdown => applicationLifetime.ApplicationStopping;

    [HttpGet("{app}/{component}")] //, HttpConnect("{app}/{component}")]
    public async Task StandardConnect
    (
        [FromRoute] string app,
        [FromRoute] string component,
        [FromQuery] string id,
        [FromQuery] int capacity,
        [FromQuery] string? operation = "standard",
        [FromQuery] bool compatible = false
    )
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var tsc = new TaskCompletionSource();
            using var session = new WebSocketSignalingHandler
            (
                await HttpContext.WebSockets.AcceptWebSocketAsync(),
                tsc,
                HostShutdown
            );
            var config = new CrossportConfig
                         {
                             Application = app,
                             Component = component,
                             Capacity = capacity
                         };
            await appManager.RegisterOrRenew
            (
                operation == "debug" ? diagnosticFactory.Wrap(session) : session,
                id,
                config,
                compatible
            );
            await appManager.ListenExceptions(session.ListenAsync);


            await tsc.Task;
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsync("Only WebSocket Connections are allowed.", HostShutdown);
        }
    }
}