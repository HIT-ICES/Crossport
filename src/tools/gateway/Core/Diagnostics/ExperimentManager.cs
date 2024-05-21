using Anonymous.Crossport.Utils;

namespace Anonymous.Crossport.Core.Diagnostics;

public class ExperimentManager(AppManager appManager, ILogger<ExperimentManager> logger)
{
    private string? _currentApp;
    private ExperimentConfig? _currentConfig;
    private ExperimentResult? _result;
    public List<ClientExpStats> ClientExpStats { get; set; } = new();
    public bool IsRunning { get; private set; }

    public bool StartExperiment(string app, ExperimentConfig config)
    {
        if (IsRunning)
        {
            logger.LogCrossport
            (
                CrossportEvents.EvaluationAlreadyRunning,
                "Experiment #{index} for application '{app}' failed to start. Another experiment {lastApp} #{lastIndex} is running.",
                config.Index,
                app,
                _currentApp,
                _currentConfig?.Index
            );
            return false;
        }

        foreach (var (component, appConfig) in config.Components)
            appManager.EnsureAppComponent(new AppInfo(app, component))
                      .Config = appConfig;

        _currentConfig = config;
        _currentApp = app;
        ClientExpStats.Clear();
        IsRunning = true;
        logger.LogCrossport
        (
            CrossportEvents.EvaluationStarted,
            "Experiment #{index} for application '{app}' started with configuration [{user} users,{components} components]",
            config.Index,
            app,
            config.Users,
            config.Components.Count
        );
        return true;
    }

    public bool UploadStats(string app, ClientExpStats stats)
    {
        if (!IsRunning)
        {
            logger.LogCrossport
            (
                CrossportEvents.EvaluationStatsDropped,
                "Client Experiment Stats Dropped: No experiment running."
            );
            return false;
        }

        if (app != _currentApp)
        {
            logger.LogCrossport
            (
                CrossportEvents.EvaluationStatsDropped,
                "Client Experiment Stats Dropped: App not Matched."
            );
            return false;
        }

        ClientExpStats.Add(stats);
        logger.LogCrossport
        (
            CrossportEvents.EvaluationStatsReceived,
            "Client Experiment Stats Received: {data}.",
            stats
        );
        return true;
    }

    public void StopExperiment()
    {
        if (!IsRunning) return;
        if (ClientExpStats.Count != 0)
        {
            _result = new ExperimentResult
            (
                _currentApp ?? "<Empty>",
                _currentConfig?.Users ?? 0,
                _currentConfig?.Index ?? -1,
                new ExpStats
                (
                    Latency.Conclude(ClientExpStats.Select(c => c.NetLatency)),
                    Latency.Conclude(ClientExpStats.Select(c => c.MtpLatency)),
                    FrameRate.Conclude(ClientExpStats.Select(c => c.LocalFrameRate)),
                    FrameRate.Conclude(ClientExpStats.Select(c => c.RemoteFrameRate))
                )
            );
            IsRunning = false;
            logger.LogCrossport
            (
                CrossportEvents.EvaluationStopped,
                "Experiment #{index} for application '{app}' stopped.",
                _currentConfig?.Index,
                _currentApp
            );
        }
        else
        {
            IsRunning = false;
            logger.LogCrossport
            (
                CrossportEvents.EvaluationStoppedWithoutResult,
                "Experiment #{index} for application '{app}' stopped, but no result was generated.",
                _currentConfig?.Index,
                _currentApp
            );
        }
    }

    public ExperimentResult? GetResult(string app)
    {
        if (_currentApp != app) return null;
        StopExperiment();
        return _result;
    }
}