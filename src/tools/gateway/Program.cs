using Anonymous.Crossport.Core;
using Anonymous.Crossport.Core.Connecting;
using Anonymous.Crossport.Core.Diagnostics;
using Anonymous.Crossport.Core.Signalling;
using Serilog;
using Steeltoe.Extensions.Configuration.Placeholder;

Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .CreateBootstrapLogger(); // <-- Change this line!

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddPlaceholderResolver();

builder.Host.UseSerilog
(
    (context, services, configuration) => configuration
                                         .ReadFrom.Configuration(context.Configuration)
                                         .ReadFrom.Services(services)
                                         .Enrich.FromLogContext()
);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
       .AddSingleton<AppManager>()
       .AddSingleton<DiagnosticSignallingHandlerFactory>()
       .AddSingleton<ExperimentManager>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var webSocketOptions = new WebSocketOptions
                       {
                           KeepAliveInterval = TimeSpan.FromSeconds(1)
                       };

Peer.LostPeerLifetime = builder.Configuration.GetSection("Crossport:ConnectionManagement")
                               .GetValue<int>(nameof(Peer.LostPeerLifetime));
NonPeerConnection.OfferedConnectionLifetime = builder.Configuration.GetSection("Crossport:ConnectionManagement")
                                                     .GetValue<int>
                                                      (
                                                          nameof(NonPeerConnection.OfferedConnectionLifetime)
                                                      );

app.UseWebSockets(webSocketOptions);


app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();


app.Run();